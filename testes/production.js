import http from 'k6/http';
import { group, check } from 'k6'

export const options = {
  stages: [
    { target: 100, duration: '5m' },
    // { target: 500, duration: '5m' },
    // { target: 1000, duration: '5m' },
    // { target: 2000, duration: '5m' },
  ]
};

const brokerURL = `http://${__ENV.DNS_BROKER}/api/cep/01222020`;

export default () => {
  
  group('Send requests to GET CEP', () => {
    const res = http.get(brokerURL);
    check(res, {
      'status is 200': (r) => r.status === 200,
      'response attributes are not null or empty': (r) =>
        r.json().rua && r.json().rua !== null && r.json().rua !== '' &&
        r.json().uf && r.json().uf !== null && r.json().uf !== '' &&
        r.json().cidade && r.json().cidade !== null && r.json().cidade !== '' &&
        r.json().bairro && r.json().bairro !== null && r.json().bairro !== '' &&
        r.json().provedor && r.json().provedor !== null && r.json().provedor !== '',
    });
  });
};