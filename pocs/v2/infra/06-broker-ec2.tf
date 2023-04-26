resource "aws_network_interface" "broker" {
  subnet_id       = aws_subnet.public_subnet_1.id
  security_groups = [aws_security_group.broker.id]

  tags = local.common_tags
}

resource "aws_instance" "broker" {
  ami           = nonsensitive(data.aws_ssm_parameter.ami.value)
  instance_type = var.broker_ec2_instance_type

  network_interface {
    network_interface_id = aws_network_interface.broker.id
    device_index         = 0
  }

  tags = local.common_tags
}

output "broker" {
  value = aws_instance.broker.public_dns
}