resource "aws_network_interface" "broker" {
  subnet_id       = aws_subnet.public_subnets[1].id
  security_groups = [aws_security_group.broker.id]

  tags = local.common_tags
}

resource "aws_instance" "broker" {
  ami           = "ami-0bf606f6236128bd0" # Ubuntu Server 20.04 LTS (HVM), SSD Volume Type (64 bits (Arm))
  instance_type = var.default_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.broker.id
    device_index         = 0
  }

  user_data = templatefile("${path.module}/startup-broker.tpl", {
    dns_provedor = aws_instance.provedor.public_dns,
    dns_influx = aws_instance.influx.public_dns,
    token_influx = var.influx_admin_token
  })

  tags = local.common_tags
}

output "broker" {
  value = aws_instance.broker.public_dns
}