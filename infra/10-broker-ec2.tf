resource "aws_network_interface" "broker" {
  subnet_id       = aws_subnet.public_subnets[1].id
  security_groups = [aws_security_group.broker.id]
  private_ips     = [var.private_ip_broker]

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-broker-net-interface"
  })
}

resource "aws_instance" "broker" {
  ami           = var.default_ec2_ami
  instance_type = var.default_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.broker.id
    device_index         = 0
  }

  user_data = templatefile("${path.module}/startup-broker.tpl", {
    dns_provedor = aws_instance.provedor-limiter,
    dns_influx   = aws_instance.influx.public_dns,
    token_influx = var.influx_admin_token
  })

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-ec2-broker"
  })
}

output "broker" {
  value = aws_instance.broker.public_dns
}