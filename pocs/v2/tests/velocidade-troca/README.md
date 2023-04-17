# Teste: medir a velocidade de troca de um provedor indisponível

Esse teste vai utilizar provedores fake configurados para retornar erros a partir de X requisições. 

O sistema sendo testado (SUT - System Under Tests) - nosso ApiBroker e os potenciais "concorrentes, como o 'cep-promise' - enviarão requisições constantes para os provedores e esperamos ver **quantas requisições falham até o SUT identificar que o provedor está indisponível e trocar o provedor ativo. Quanto menor o número de requisições com falha, melhor o resultado.**

**O SUT deve ser configurado para NÃO tentar todos os provedores antes de retornar erro para o cliente.** Deve retornar para o cliente exatamente a resposta do primeiro provedor acionado.
