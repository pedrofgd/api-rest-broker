import http from 'k6/http';
import { group, check, sleep } from 'k6'

export const options = {
  stages: [
    { target: 100, duration: '1m' },
  ]
};

const brokerURL = "http://localhost:5070/api/cep-java/01222020";

export default () => {
  
  group('Send GET requests to cep-java', () => {
    const res = http.get(brokerURL);
    const checkRes = check(res, {
      'status is 200': (r) => r.status === 200,
    });

    sleep(2)
  });
};


