
import readline from 'readline';

export class UserInputHandler {
    constructor() {
        this.rl = readline.createInterface({
            input: process.stdin,
            output: process.stdout
        });
    }

    async getUserInput() {
        return new Promise((resolve) => {
            this.rl.question('Enter your message: ', (input) => {
                resolve(input);
            });
        });
    }

    close() {
        this.rl.close();
    }
}