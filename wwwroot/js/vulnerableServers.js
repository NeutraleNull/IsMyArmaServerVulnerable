const API_URL = '/api/TestServer/serversvulnerable';

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
        console.error(err);
        const el = document.getElementById('vulnerable-servers');
        el.remove();
    }
});

/**
 * 
 * @param {{ servers: number, serversReachable: number, serversVulnerable: number }} body
 */
function processResponse({ servers, serversReachable, serversVulnerable }) {
    const el = document.getElementById('vulnerable-servers');

    const percent = Math.round((serversVulnerable / serversReachable) * 100);

    el.innerHTML = `${serversVulnerable} out of ${serversReachable} Arma 3 servers are vulnerable to UDP Reflection Attacks<br/>That is approx ${Number.isNaN(percent) ? 100 : percent}% of all Arma 3 servers.`;
}
