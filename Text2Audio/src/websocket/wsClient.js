import WebSocket from "ws";

export class WebSocketClient {
    constructor(url, headers, messageHandler) {
        this.ws = new WebSocket(url, { headers });
        this.connectionPromise = new Promise((resolve, reject) => {
            this.ws.once('open', () => resolve());
            this.ws.once('error', reject);
        });
        this.setupEventHandlers(messageHandler);
    }

    setupEventHandlers(messageHandler) {
        this.ws.on("open", () => console.log("Connected to server."));
        this.ws.on("message", messageHandler);
        this.ws.on("error", (err) => console.error('WebSocket error:', err));
        this.ws.on("close", () => console.log('WebSocket connection closed'));
    }

    async connect() {
        await this.connectionPromise;
    }

    send(message) {
        if (this.ws.readyState !== WebSocket.OPEN) {
            throw new Error('WebSocket is not open');
        }
        this.ws.send(JSON.stringify(message));
    }

    close() {
        this.ws.close();
    }
}