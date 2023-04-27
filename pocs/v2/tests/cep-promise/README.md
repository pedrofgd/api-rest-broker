Clonado do repositório original: [BrasilAPI/cep-promise](https://github.com/BrasilAPI/cep-promise)

A estratégia utilizada por essa ferramenta pode ser uma alternativa ao sistema que estamos propondo.

O propósito de clonar e testar o `cep-promise` é:
1. Avaliar se realmente pode ser considerado uma alternativa
2. Comparar resultados de testes com a nossa proposta

O projeto nesse diretório poderá ser submetido aos mesmos cenários de teste que a aplicação proposta. Poderá também ser ajustado para utilizar provedores fake e exportar métricas para análise.

## Funcionamento

O que chamamos de provedores na nossa proposta, aqui são implementados `[services](./src/services/)`. Nesses arquivos as requisições são montadas e enviadas usando `fetch`, equivalente ao `HttpClient` na aplicação ApiBroker em .NET. Nós já estamos considerando na PoC os mesmos provedores de CEP (correios-alt, viacep, widenet).

O "orquestrador" é o `[cep-promise.js](./src/cep-promise.js)`, que faz alguns tratamentos no valor recebido na consulta, obtem os provedores **cadastrados**.

O usuário do pacote pode especificar quais provedores deseja utilizar dentre os disponíveis no catálogo. Caso informe uma lista de provedores (`configuration.providers`), então a consulta será disparada entre esses selecionados. Caso contrário, todos os provedores cadastrados serão acionados.

A estratégia para consulta é: disparar ao mesmo tempo uma requisições para cada provedor. A resposta mais rápida será retornada para o cliente. Isso é implementado através do `[promise-any.js](./src/utils/promise-any.js)` e do método `[fetchCepFromServices](./src/cep-promise.js)`.

O que chamamos de mapeamento da resposta é feito aqui também. A resposta específica dos provedores é mapeada para um objeto padrão, porém isso é implementado por provedor e "hard-coded", não dinamicamente.

Esse pacote faz parte do projeto [BrasilAPI](). Esse é um exemplo completo de código que consome o `cep-promise`: [BrasilAPI/pages/api/cep/v2
/[cep].js](https://github.com/BrasilAPI/BrasilAPI/blob/main/pages/api/cep/v2/%5Bcep%5D.js).

Resumindo, o pacote é utilizado dessa forma:
```
import cepPromise from 'cep-promise';

// ...

const providers = ['correios', 'viacep', 'widenet', 'correios-alt'];

async function getCepFromCepPromise(requestedCep) {
  return cepPromise(requestedCep, { providers });
}

// ...
```

O usuário do pacote poderia implementar uma lógica para alterar dinamicamente a lista de provedores disponíveis, para excluir os que estiverem disponíveis.

## Considerações

* Todos os provedores são acionados em todas as requisições, o que poderia gerar custos mais altos
* O algoritmo implementado já é capaz de "trazer" sempre o melhor provedor, sem precisar ranquear
* Nossa proposta foi implementada com C#, então avaliar quais métricas fazem sentido comparar entre as duas soluções
