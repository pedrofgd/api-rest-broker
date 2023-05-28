resource "random_integer" "rand" {
  min = 10000
  max = 99999
}

locals {
  name_prefix = "tcc-mack"

  common_tags = {
    organization = var.organization
    project      = "${var.project}-${var.organization}"
  }
}