resource "random_integer" "rand" {
  min = 10000
  max = 99999
}

locals {
  common_tags = {
    organization = var.organization
    project      = "${var.project}-${var.organization}"
  }
}