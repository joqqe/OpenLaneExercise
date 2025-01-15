import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';
import crypto from "k6/crypto";
import encoding from "k6/encoding";

export let options = {
    stages: [
        { duration: '30s', target: 20 }, // Ramp-up
        { duration: '1m', target: 20 },  // Stay at
        { duration: '20s', target: 0 },  // Ramp-down
    ],
};

const algToHash = {
  HS256: "sha256",
  HS384: "sha384",
  HS512: "sha512"
};

function sign(data, hashAlg, secret) {
  let hasher = crypto.createHMAC(hashAlg, secret);
  hasher.update(data);
  return hasher.digest("base64rawurl");
}

function encode(payload, secret, algorithm = "HS256") {
  let header = encoding.b64encode(JSON.stringify({ typ: "JWT", alg: algorithm }), "rawurl");
  payload = encoding.b64encode(JSON.stringify(payload), "rawurl");
  let sig = sign(header + "." + payload, algToHash[algorithm], secret);
  return [header, payload, sig].join(".");
}

function decode(token, secret, algorithm) {
  let parts = token.split('.');
  let header = JSON.parse(encoding.b64decode(parts[0], "rawurl", "s"));
  let payload = JSON.parse(encoding.b64decode(parts[1], "rawurl", "s"));
  algorithm = algorithm || algToHash[header.alg];
  if (sign(parts[0] + "." + parts[1], algorithm, secret) != parts[2]) {
    throw Error("JWT signature verification failed");
  }
  return payload;
}


export function setup() {
    const userId = "27e3f9d5-a32c-4d9a-94cb-79a3fea2368a";
    const message = {
        "sub": userId
    };
    const token = encode(message, "your_very_long_secret_key_that_is_at_least_32_characters_long");

    return { userId: userId, token: token };
}

export default function (data) {
    const body = {
        BidObjectId: uuidv4(),
        OfferObjectId: "47e3f9d5-a32c-4d9a-94cb-79a3fea2368a",
        Price: 100 + exec.scenario.iterationInTest,
        UserObjectId: data.userId
    }

    const res = http.post(
        'https://localhost:7043/Api/Bid',
        JSON.stringify(body), 
        {
            headers: { 
                'Content-Type': 'application/json', 
                'Idempotency-Key': uuidv4(),
                'Authorization': 'Bearer ' + data.token
            }
        });

    check(res, {
        'status is 202': (r) => r.status === 202,
    });
    sleep(1);
}