#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

# Criar container para o provedor fake
sudo docker run -d \
  --name=provedor-fake \
  -p 80:8080 \
  pedrofgd/tcc-provedor-fake:latest