const API_URL = '/api/TestServer/testserver';

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

        toggleLoader(true);

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
        } finally {
            toggleLoader(false);
        }
    });
});

/**
 * @param {boolean} state
 */
function toggleLoader(state) {
    const el = document.getElementById('test');

    const loader = el.querySelector(':scope > .loader');

    if (state) {
        loader.style.display = '';
    } else {
        loader.style.display = 'none';
    }

}

/**
 * @param {{ hostname: string, port: number }} server
 * @param {{ isVulnerable: boolean, isReachable: boolean, requestFailed: boolean }} response
 */
function processResponse({ hostname, port}, { isVulnerable, isReachable, requestFailed }) {
    if (requestFailed) {
        setResult(`Failed to test <pre>${hostname}:${port}</pre>`);
        return;
    }

    if (!isReachable) {
        setResult(`Failed to test <pre>${hostname}:${port}</pre>. It is not reachable.`);
        return;
    }

    if (isVulnerable) {
        setResult(`<pre>${hostname}:${port}</pre> is <strong style="color: rgb(201, 17, 6);">vulnerable</strong> to reflection attacks.<br/><br/>Check out <a href="#how-to-fix">our guide</a> on the bottom of this page on how to fix this.`);
    } else {
        setResult(`<pre>${hostname}:${port}</pre> is <strong style="color: #66AA66;">not vulnerable</strong> to reflection attacks.`);
    }
}

/**
 * @param {Error} err
 */
function processError(err) {
    setResult(`An error occurred.`);
}


/**
 * @param {string} text
 */
function setResult(text) {
    const el = document.getElementById('test');

    let result = el.querySelector('.result');
    let resultContent;
    if (result === null) {
        result = document.createElement('div');
        result.classList = 'result';

        resultContent = document.createElement('p');
        resultContent.style.justifySelf = 'center';
        resultContent.style.alignSelf = 'center';

        const btn = document.createElement('button');
        btn.type = "button";
        btn.innerText = 'Test another server';
        btn.addEventListener('click', e => result.style.display = 'none');

        result.appendChild(resultContent);
        result.appendChild(btn);


        el.appendChild(result);
    } else {
        result.style.display = '';
        resultContent = result.querySelector(':scope > p');
    }

    resultContent.innerHTML = text;
}