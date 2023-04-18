import http from 'k6/http';
import { group, check } from 'k6'

export const options = {
  stages: [
    { target: 200, duration: '1m' },
  ]
};

const brokerURL = "http://localhost:5070/api/cep-promise/01222-020";

export default () => {
  
  group('Send GET requests to cep-promise', () => {
    const res = http.get(brokerURL);
    const checkRes = check(res, {
      'status is 200': (r) => r.status === 200,
    });
  });
};


