# Teste: medir a latência adicionada pelo sistema e o consumo de memória e CPU

Os sistemas em teste (SUT - System Under Test) constantemente vão enviar requisições para os provedores fake. Serão coletadas métricas dos SUT para identificar a latência adicionada à requisição (quantos ms o sistema adicionou além do tempo que o provedor levou para responder a requisição) e o consumo de memória e CPU no servidor.

Os resultados serão comparados entre cada SUT. Os sistemas a serem testados são:
* ApiBroker (nosso sistema)
* Aplicação utilizando o pacote cep-promise
* Nginx load balancer utilizando o módulo lua e jq (pendente de poc)
