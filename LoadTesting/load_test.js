import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    stages: [
        { duration: '30s', target: 20 }, // Ramp-up
        { duration: '1m', target: 20 },  // Stay at
        { duration: '10s', target: 0 },  // Ramp-down
    ],
};

export default function () {
    const body = {
        "OfferObjectId": "57e3f9d5-a32c-4d9a-94cb-79a3fea2368a",
        "Price": "",
        "UserObjectId": "37e3f9d5-a32c-4d9a-94cb-79a3fea2368a"
    }
    const res = http.post('http://127.0.0.1:5092/api/bid', body);
    check(res, {
        'status is 200': (r) => r.status === 200,
    });
    sleep(1);
}