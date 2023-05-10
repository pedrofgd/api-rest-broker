resource "aws_key_pair" "key_pair" {
  key_name   = "aws_key"
  public_key = file("${abspath(path.cwd)}/keys/aws_key.pub")
}

resource "aws_network_interface" "broker" {
  subnet_id       = aws_subnet.public_subnet_1.id
  security_groups = [aws_security_group.broker.id]

  tags = local.common_tags
}

resource "aws_instance" "broker" {
  ami           = "ami-0c6c29c5125214c77" # Ubuntu Server 22.04 LTS (HVM), SSD Volume Type (64 bits (Arm))
  instance_type = var.broker_ec2_instance_type
  key_name      = aws_key_pair.key_pair.key_name

  network_interface {
    network_interface_id = aws_network_interface.broker.id
    device_index         = 0
  }

  user_data = file("${path.module}/startup-broker.tpl")

  tags = local.common_tags
}

output "broker" {
  value = aws_instance.broker.public_dns
}