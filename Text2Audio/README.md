
# Text2Audio - Real-time Text-to-Speech using OpenAI API

A Node.js application that connects to OpenAI's WebSocket API to perform real-time text-to-speech conversion and saves the audio output as WAV files.

## Features

- Real-time text-to-speech conversion using OpenAI's WebSocket API
- WAV file encoding with configurable sample rate
- Saves both individual audio chunks and combined audio
- Generates detailed metadata for each conversion
- Automatic output directory management

## Prerequisites

- Node.js (v14 or higher)
- An OpenAI API key with access to real-time TTS features

## Setup

1. Clone the repository:
2. Install dependencies:
   `npm install`
3. Create a `.env` file in the project root directory:
   `OPENAI_API_KEY=your_openai_api_key_here`
## Usage
1. Run the application:
    `node main.js`
2. The application will:
- Connect to OpenAI's WebSocket API
- Process the text-to-speech conversion
- Save audio files in the /output_audio directory

## Output
The application creates the following in the /output_audio directory:

- Complete WAV audio file
- Individual audio chunks in a timestamped subdirectory
- Metadata JSON file with audio details
- Output log file containing all WebSocket messages

## Project Structure
- main.js: WebSocket client and main application logic
- audioProcessor.js: Handles audio processing and file management
- audioUtils.js: Contains WAV encoding and file management utilities

## Note
Make sure to keep your OpenAI API key confidential and never commit the .env file to version control.