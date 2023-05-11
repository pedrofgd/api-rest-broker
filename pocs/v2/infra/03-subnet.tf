resource "aws_subnet" "public_subnets" {
  count                   = 2
  cidr_block              = var.vpc_subnets_cidr_block[count.index]
  vpc_id                  = aws_vpc.main.id
  map_public_ip_on_launch = true
  availability_zone       = data.aws_availability_zones.available.names[0]
}