import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,            // 100 eş zamanlı sanal kullanıcı
    iterations: 1000,    // Toplam 1000 istek
};

export default function () {
    const url = 'http://localhost:7000/products';

    const params = {
        headers: {
            'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBZG1pbjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkFkbWluNyIsImp0aSI6IjIxMDBjNDQ3LWEwNmItNDZmMS1iZWM4LWUwNDM1MzMyMmY1OSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwicm9sZSI6IkFkbWluIiwiZXhwIjoxNzUyNTgxNzM2LCJpc3MiOiJFQ29tbWVyY2UuSWRlbnRpdHkiLCJhdWQiOiJFQ29tbWVyY2UuVXNlciJ9.RBNNCUBofbV4WLgiWb5zcjg2s47b6sooLHEWgjLlFus'
        }
    };

    const res = http.get(url, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(1);
}
