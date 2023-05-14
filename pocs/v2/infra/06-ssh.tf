resource "aws_key_pair" "key_pair" {
  key_name   = "aws_key"
  public_key = file("${abspath(path.cwd)}/keys/aws_key.pub")
}