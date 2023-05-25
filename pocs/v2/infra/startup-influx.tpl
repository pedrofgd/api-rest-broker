#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

echo "Creating Docker network tik_network..."
sudo docker network create tik_network

echo "Creating InfluxDB container..."
sudo docker run -d \
   --name=influxdb \
   --network=tik_network \
   -p 8086:8086 \
   influxdb:1.8

echo "Setting up InfluxDB container..."
sleep 5
# Criar o banco de dados de logs, utilizado pelo Broker
sudo docker exec influxdb influx -execute 'CREATE DATABASE logs'

echo "Creating Kapacitor container..."
sudo docker run -d \
   --name=kapacitor \
   --network=tik_network \
   -e KAPACITOR_WEBHOOK_URL=http://${broker_ip}/webhook/kapacitor \
   pedrofgd/tcc-monitor-kapacitor:latest

echo "Setting up Kapacitor container..."
sleep 10
sudo docker exec -w /home/kapacitor kapacitor sh apply.sh
