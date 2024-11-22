import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const readInputText = () => {
    try {
        return fs.readFileSync(path.join(__dirname, 'SYSTEM_PROMPT.txt'), 'utf-8').trim();
    } catch (error) {
        console.error('Error reading input file:', error);
        return 'Default message if file read fails';
    }
};

export const conversationEvents = {
    sampleMessage: {
        type: 'conversation.item.create',
        item: {
            type: 'message',
            role: 'user',
            content: [
                {
                    type: 'input_text',
                    text: readInputText()
                }
            ]
        }
    }
};