#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

# Criar container para o provedor fake limiter
sudo docker run -d \
  --name=provedor-limiter \
  -p 80:80 \
  -e PermitLimitApiCep='300' \
  -e WindowFromSecondsLimitApiCep='60' \
  pedrofgd/tcc-provedor-fake-limiter:latest

