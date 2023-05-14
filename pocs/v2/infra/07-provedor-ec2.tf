resource "aws_network_interface" "provedor" {
  subnet_id       = aws_subnet.public_subnets[0].id
  security_groups = [aws_security_group.provedor.id]

  tags = local.common_tags
}

resource "aws_instance" "provedor" {
  ami           = "ami-0c6c29c5125214c77" # Ubuntu Server 22.04 LTS (HVM), SSD Volume Type (64 bits (Arm))
  instance_type = var.default_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.provedor.id
    device_index         = 0
  }

  user_data = file("${path.module}/startup-provedor.tpl")

  tags = local.common_tags
}

output "provedor" {
  value = aws_instance.provedor.public_dns
}