const API_URL = 'https://localhost:5001/api/TestServer/testserver';

window.addEventListener('DOMContentLoaded', () => {
    /** @type {HTMLFormElement} */
    const form = document.getElementById('test');

    form.addEventListener('submit', async (event) => {
        event.preventDefault();

        // construct request body
        const body = {};
        const formData = new FormData(event.target);
        for (const [name, value] of formData.entries()) {
            body[name] = value;
        }

        try {
            const responseBody = await fetch(API_URL, {
                method: 'POST',
                body: JSON.stringify(body),
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            }).then(res => res.json());

            // check for invalid response
            for (const field of ['isVulnerable', 'isReachable', 'requestFailed']) {                
                if (!Object.prototype.hasOwnProperty.call(responseBody, field)) {
                    throw new Error('Received invalid response from server.');
                }
            }

            processResponse(body, responseBody);
        } catch (err) {
            processError(err);
        }
    });
});

/**
 * 
 * @param {{ hostname: string, port: number }} server
 * @param {{ isVulnerable: boolean, isReachable: boolean, requestFailed: boolean }} response
 */
function processResponse({ hostname, port}, { isVulnerable, isReachable, requestFailed }) {
    // TODO:
    console.log(isVulnerable, isReachable, requestFailed);

    if (requestFailed) {
        setResult(`Failed to test ${hostname}:${port}`);
        return;
    }

    if (!isReachable) {
        setResult(`Failed to test ${hostname}:${port}. It is not reachable.`);
        return;
    }

    if (isVulnerable) {
        setResult(`${hostname}:${port} is vulnerable to reflection attacks.`);
    } else {
        setResult(`${hostname}:${port} is not vulnerable to reflection attacks.`);
    }
}

/**
 * 
 * @param {Error} err
 */
function processError(err) {
    // TODO:
    console.error(err);
}


/**
 * @param {string} text
 */
function setResult(text) {
    const el = document.getElementById('test');

    el.innerHTML = text;
}