This is broken down in several different parts.

```
Text2Audio
CSharpInterface
```

Next steps:
Implement 
- [ ] Proper Eval for Text2Audio
  - [ ] Going through multiple questions and see if they make sense or not!
- [ ] Multi-step Text2Audio
  - [ ] Hooking it up to the console and go back and forth from there!
- [ ] Multi-step Audio2Audio conversion
  - [ ] Hooking it up to the console and go back and forth from there!
- [ ] Audio2Audio conversion
  - [x] Basic implementation (available in /Audio2Audio)
  - [x] Can I prompt it for Audio2Audio?
- [ ] Voice Activity Detection (VAD) implementation
    - [ ] On/Off toggle functionality
- [ ] Multiple voice support
- [ ] Chunk to audio conversion
- [ ] Function calling for RAG
- [ ] Documentation
    - [x] Event documentation for Text2Audio
    - [x] Event documentation for Audio2Audio
    - [ ] README updates
- [ ] Backend development
    - [x] WebSocket implementation
    - [x] Frontend integration testing - Super simplistic
    - [x] Make a button for sending the request on WebSocket from the FrontEnd
    - [ ] Make button for session update
    - [ ] Unity Frontend integration testing