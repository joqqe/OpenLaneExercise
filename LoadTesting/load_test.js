import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';

export let options = {
    stages: [
        { duration: '1m', target: 40 }, // Ramp-up
        { duration: '2m', target: 40 },  // Stay at
        { duration: '20s', target: 0 },  // Ramp-down
    ],
};

export default function () {
    const body = {
        OfferObjectId: "47e3f9d5-a32c-4d9a-94cb-79a3fea2368a",
        Price: 100 + exec.scenario.iterationInTest,
        UserObjectId: "27e3f9d5-a32c-4d9a-94cb-79a3fea2368a"
    }

    const res = http.post(
        'https://localhost:7043/Api/Bid',
        JSON.stringify(body), 
        {
        headers: { 'Content-Type': 'application/json' }
      });

    check(res, {
        'status is 202': (r) => r.status === 202,
    });
    sleep(1);
}