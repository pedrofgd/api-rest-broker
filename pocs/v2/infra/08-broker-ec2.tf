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
  ami                  = nonsensitive(data.aws_ssm_parameter.ami.value)
  instance_type        = var.broker_ec2_instance_type
  key_name             = aws_key_pair.key_pair.key_name
  iam_instance_profile = aws_iam_instance_profile.broker_profile.name
  depends_on           = [aws_iam_role_policy.allow_s3_all]

  network_interface {
    network_interface_id = aws_network_interface.broker.id
    device_index         = 0
  }

  user_data = <<EOF
#! /bin/bash
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum -y install dotnet-sdk-7.0
cd ~
export HOME=/root
aws s3 sync s3://${aws_s3_bucket.bucket.id}/broker broker/
cd broker/src/ApiBroker.API/
sudo dotnet publish -c Release -o /var/www/myapp
cd /var/www/myapp && dotnet ApiBroker.API.dll
  EOF

  tags = local.common_tags
}

output "broker" {
  value = aws_instance.broker.public_dns
}