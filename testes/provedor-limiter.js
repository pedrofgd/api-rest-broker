import http from 'k6/http';
import { group, check } from 'k6'

export const options = {
  stages: [
    { target: 10, duration: '1m' },
    // { target: 50, duration: '10s' },
    // { target: 100, duration: '10s' },
    // { target: 200, duration: '10s' },
  ]
};

const brokerURL = "http://localhost:5011/viacep/01222020";

export default () => {
  
  group('Send requests to provider with rate limit', () => {
    const res = http.get(brokerURL);
    check(res, {
      'status is 200': (r) => r.status === 200
    });
  });
};