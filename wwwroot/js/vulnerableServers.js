const API_URL = 'https://localhost:5001/api/TestServer/serversvulnerable';

window.addEventListener('DOMContentLoaded', async () => {
    try {
        const responseBody = await fetch(API_URL, {
            headers: {
                'Accept': 'application/json'
            }
        }).then(res => res.json());

        // check for invalid response
        for (const field of ['servers', 'serversReachable', 'serversVulnerable']) {                
            if (!Object.prototype.hasOwnProperty.call(responseBody, field)) {
                throw new Error('Received invalid response from server.');
            }
        }

        processResponse(responseBody);
    } catch (err) {
        processError(err);
    }
});

/**
 * 
 * @param {{ servers: number, serversReachable: number, serversVulnerable: number }} body
 */
function processResponse({ servers, serversReachable, serversVulnerable }) {
    const el = document.getElementById('vulnerable-servers');
    
    if (servers === 0) {
        el.innerHTML = `Still indexing. Check back later.`;
        return;
    }


    el.innerHTML = `Currently ${serversVulnerable} out of ${serversReachable} Arma 3 servers are vulnerable. That is over ${Math.round((serversVulnerable / serversReachable) * 100)}%`;
}

/**
 * 
 * @param {Error} err
 */
function processError(err) {
    // TODO:
    console.error(err);
}