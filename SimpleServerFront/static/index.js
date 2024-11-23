const log = document.getElementById('log');

function formatJSON(obj) {
    const json = JSON.stringify(obj, null, 2);
    return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
        let cls = 'json-number';
        if (/^"/.test(match)) {
            if (/:$/.test(match)) {
                cls = 'json-key';
            } else {
                cls = 'json-string';
            }
        } else if (/true|false/.test(match)) {
            cls = 'json-boolean';
        } else if (/null/.test(match)) {
            cls = 'json-null';
        }
        return `<span class="${cls}">${match}</span>`;
    });
}

function appendLog(message) {
    const item = document.createElement('div');
    item.classList.add('log-item');
    if (typeof message === 'object') {
        item.innerHTML = formatJSON(message);
    } else {
        item.textContent = message;
    }
    log.appendChild(item);
    log.scrollTop = log.scrollHeight;
}

// Connect to WebSocket server
const ws = new WebSocket(`ws://${window.location.host}/ws`);

ws.onopen = () => {
    appendLog('WebSocket connection established');
};

ws.onmessage = (event) => {
    try {
        const data = JSON.parse(event.data);
        appendLog(data);
    } catch (e) {
        appendLog(event.data);
    }
};

ws.onclose = () => {
    appendLog('WebSocket connection closed');
};

ws.onerror = (error) => {
    appendLog(`WebSocket error: ${error}`);
};

// Add button click handler
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');

sendButton.addEventListener('click', () => {
    const message = messageInput.value.trim();
    if (message) {
        ws.send(message);
        messageInput.value = '';
    }
});

// Add enter key handler
messageInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        sendButton.click();
    }
});