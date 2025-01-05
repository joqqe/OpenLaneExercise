import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';

export let options = {
    stages: [
        { duration: '30s', target: 20 }, // Ramp-up
        { duration: '1m', target: 40 },  // Stay at
        { duration: '10s', target: 0 },  // Ramp-down
    ],
};

export default function () {
    const body = {
        OfferObjectId: "57e3f9d5-a32c-4d9a-94cb-79a3fea2368a",
        Price: 100 + exec.scenario.iterationInTest,
        UserObjectId: "37e3f9d5-a32c-4d9a-94cb-79a3fea2368a"
    }

    const res = http.post(
        'https://localhost:7043/Api/Bid',
        JSON.stringify(body), 
        {
        headers: { 'Content-Type': 'application/json' }
      });

    check(res, {
        'status is 201': (r) => r.status === 201,
    });
    sleep(1);
}