#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

# Rede para habilitar comunicação entre os container do broker
echo "Creating docker network..."
sudo docker network create tcc-network

echo "Creating InfluxDB container..."
sudo docker run -d \
  --name=influxdb \
  --network=tcc-network \
  -p 8086:8086 \
  -e DOCKER_INFLUXDB_INIT_MODE=setup \
  -e DOCKER_INFLUXDB_INIT_USERNAME=admin \
  -e DOCKER_INFLUXDB_INIT_PASSWORD=01Senha! \
  -e DOCKER_INFLUXDB_INIT_ORG=broker \
  -e DOCKER_INFLUXDB_INIT_BUCKET=logs \
  -e DOCKER_INFLUXDB_INIT_RETENTION=7d \
  influxdb:2.6

echo "Setting up InfluxDB container..."
sleep 5