#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

echo "Creating Redis container..."
sleep 20
docker run --name redis -d -p 6379:6379 redis


echo "Creating API Broker container..."

# Aguardar um pouco para o provedor também ser inicializado
# por completo antes do broker começar a chamá-lo e
# aguardar as configurações de rede na instância
sleep 40

sudo docker run -d \
  --name=broker \
  -p 80:80\
  -e InfluxDbSettings__Url=http://${dns_influx}:8086 \
  -e InfluxDbSettings__Token=${token_influx} \
  -e Recursos__0__provedores__0__rota=http://${dns_provedor[0].public_dns}/correios-alt/{cep} \
  -e Recursos__0__provedores__0__healthcheck__rotaHealthcheck=http://${dns_provedor[0].public_dns}/correios-alt/01222020 \
  -e Recursos__0__provedores__1__rota=http://${dns_provedor[1].public_dns}/via-cep/{cep} \
  -e Recursos__0__provedores__1__healthcheck__rotaHealthcheck=http://${dns_provedor[1].public_dns}/via-cep/01222020 \
  -e Recursos__0__provedores__2__rota=http://${dns_provedor[2].public_dns}/widenet/{cep} \
  -e Recursos__0__provedores__2__healthcheck__rotaHealthcheck=http://${dns_provedor[2].public_dns}/widenet/01222020 \
  -e PortalSettings__Host=http://portal:3000 \
  -e RedisCache__Host=localhost \
  pedrofgd/tcc-broker:v0.1.1

echo "Broker is now good to go!"