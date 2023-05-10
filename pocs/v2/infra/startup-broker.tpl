#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

# Criar rede para habilitar comunicação entre o Broker e o InfluxDB
echo "Creating docker network..."
docker network create tcc-network

# Criar container para o InfluxDB
echo "Creating InfluxDB container..."
docker run -d \
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

# Obter token de acesso do InfluxDB
INFLUX_ACCESS_TOKEN=`docker exec influxdb influx auth list | awk '/admin/ {print $4}'`

# Criar container para a aplicação do Broker
echo "Creating API Broker container..."
docker run -d \
  --name=broker \
  --network=tcc-network \
  -p 80:80\
  -e InfluxDbSettings__Url=http://influxdb:8086 \
  -e InfluxDbSettings__Token=$INFLUX_ACCESS_TOKEN \
  pedrofgd/tcc-broker:v0.1.0
