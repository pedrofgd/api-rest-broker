resource "aws_network_interface" "provedor-limiter" {
  count           = 3
  subnet_id       = aws_subnet.public_subnets[0].id
  security_groups = [aws_security_group.provedor.id]

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-provedor-limiter-net-interface"
  })
}

resource "aws_instance" "provedor-limiter" {
  count         = 3
  ami           = var.default_ec2_ami
  instance_type = var.default_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.provedor-limiter[count.index].id
    device_index         = 0
  }

  user_data = file("${path.module}/startup-provedor-limiter.tpl")

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-ec2-provedor-limiter"
  })
}

output "provedor" {
  value = aws_instance.provedor[*].public_dns
}

