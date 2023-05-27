import http from 'k6/http';
import { group, check } from 'k6';

export let options = {
  scenarios: {
    GET_10_CEP_10s: {
      vus: 10,
      duration: '10s',
      exec: 'enviarRequestGetCep',
    },
    GET_40_CEP_10s: {
      vus: 40,
      duration: '10s',
      exec: 'enviarRequestGetCep',
    },
    GET_50_CEP_10s: {
      vus: 50,
      duration: '10s',
      exec: 'enviarRequestGetCep',
    },
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)', 'count'],
    thresholds: {},
  },
};

for (let key in options.scenarios) {
  let thresholdName = `http_req_duration{scenario:${key}}`;

  if (!options.thresholds[thresholdName]) {
    options.thresholds[thresholdName] = [];
  }

  // 'max>=0' is a bogus condition that will always be fulfilled
  options.thresholds[thresholdName].push('max >= 0');
}

export function enviarRequestGetCep(data) {
  let hostUrl = 'http://localhost:5070';
  let getUrl = '/api/cep-java/01222020';

  group('Enviando requisições para o provedor fake', () => {
    const res = http.get(hostUrl + getUrl);
    check(res, {
      'status is 200': (r) => r.status === 200,
      'response attributes are not null or empty': (r) =>
        r.json().rua !== null &&
        r.json().rua !== '' &&
        r.json().uf !== null &&
        r.json().uf !== '' &&
        r.json().cidade !== null &&
        r.json().cidade !== '' &&
        r.json().bairro !== null &&
        r.json().bairro !== '' &&
        r.json().provedor !== null &&
        r.json().provedor !== '',
    });
  });
}
