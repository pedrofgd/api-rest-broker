resource "aws_s3_bucket" "bucket" {
  bucket        = local.s3_bucket_name
  force_destroy = true
}

resource "aws_s3_object" "broker" {
  for_each = fileset("/Users/pedrodias/dev/tcc-mack/pocs/v2/ApiBroker/", "**")
  bucket = aws_s3_bucket.bucket.id
  key    =  "broker/${each.value}"
  source = "/Users/pedrodias/dev/tcc-mack/pocs/v2/ApiBroker/${each.value}"
}

output "bucket" {
  value = aws_s3_bucket.bucket.id
}