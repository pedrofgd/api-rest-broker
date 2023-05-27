resource "aws_network_interface" "influx" {
  subnet_id       = aws_subnet.public_subnets[1].id
  security_groups = [aws_security_group.influx.id]

  tags = local.common_tags
}

resource "aws_instance" "influx" {
  ami           = "ami-0bf606f6236128bd0" # Ubuntu Server 20.04 LTS (HVM), SSD Volume Type (64 bits (Arm))
  instance_type = var.default_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.influx.id
    device_index         = 0
  }

  user_data = templatefile("${path.module}/startup-influx.tpl", {
    broker_ip = var.private_ip_broker
  })

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-ec2-influx"
  })
}

output "influx" {
  value = aws_instance.influx.public_dns
}