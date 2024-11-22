
# Real-time Audio Processing System

A Node.js application for real-time audio processing using OpenAI's WebSocket API.

## Features

- Real-time audio streaming and processing
- WebSocket-based communication
- Support for WAV audio format
- Base64 audio encoding/decoding
- Modular architecture for easy extension

## Prerequisites

- Node.js v14 or higher
- OpenAI API key
- WebSocket support
- Required NPM packages:
  - ws
  - dotenv
  - audio-decode

## Setup

1. Clone the repository
2. Install dependencies:
3. Create a .env file with your OpenAI API Key:
   `OPENAI_API_KEY=your_api_key_here`

## Project Structure
- `main.js` - Main application entry point and WebSocket handler
- `audioEncoder.js` - Audio encoding/decoding utilities
- `audioStreamProcessor.js` - Stream processing and WebSocket communication

## Usage
1. Place your input audio file in the `input_audio` directory
2. Run the application:
   `node main.js`
3. Processed audio will be saved in `output_audio` directory