#! /bin/bash
echo "Installing and starting docker..."
sudo apt-get update
sudo apt-get install -y docker.io git
sudo systemctl enable docker.service
sudo systemctl start docker.service

sudo docker info

echo "Creating API Broker container..."

# Aguardar um pouco para o provedor também ser inicializado
# por completo antes do broker começar a chamá-lo e
# aguardar as configurações de rede na instância
sleep 60

sudo docker run -d \
  --name=broker \
  -p 80:80\
  -e InfluxDbSettings__Url=http://${dns_influx}:8086 \
  -e InfluxDbSettings__Token=${token_influx} \
  -e Recursos__0__provedores__0__rota=http://${dns_provedor}/correios-alt/{cep} \
  -e Recursos__0__provedores__0__healthcheck__rotaHealthcheck=http://${dns_provedor}/correios-alt/01222020 \
  -e Recursos__0__provedores__1__rota=http://${dns_provedor}/via-cep/{cep} \
  -e Recursos__0__provedores__1__healthcheck__rotaHealthcheck=http://${dns_provedor}/via-cep/01222020 \
  -e Recursos__0__provedores__2__rota=http://${dns_provedor}/widenet/{cep} \
  -e Recursos__0__provedores__2__healthcheck__rotaHealthcheck=http://${dns_provedor}/widenet/01222020 \
  -e PortalSettings__Host=http://portal:3000 \
  pedrofgd/tcc-broker:v0.1.1

echo "Setting up API Broker container..."
sleep 5