import os
import json
from datetime import datetime

class LogManager:
    def __init__(self):
        self.log_dir = "./Logs"
        self.ensure_log_directory()
        
        # Clear existing log files
        self.python_to_openai = open(f"{self.log_dir}/python_to_openai.log", "w")
        self.openai_to_python = open(f"{self.log_dir}/openai_to_python.log", "w")
        self.unity_to_python = open(f"{self.log_dir}/unity_to_python.log", "w")
        self.python_to_unity = open(f"{self.log_dir}/python_to_unity.log", "w")

    def ensure_log_directory(self):
        if not os.path.exists(self.log_dir):
            os.makedirs(self.log_dir)

    def validate_and_format_json(self, message):
        try:
            if isinstance(message, str):
                # Try to parse if it's a string
                json_obj = json.loads(message)
            else:
                # If it's already a dict/object, verify it can be serialized
                json_obj = message
                json.dumps(json_obj)  # This will raise an error if not JSON serializable
            return json.dumps(json_obj, indent=2)  # Return pretty formatted JSON
        except json.JSONDecodeError as e:
            return f"INVALID JSON: {message}\nError: {str(e)}"

    def log_message(self, file, message):
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
        formatted_json = self.validate_and_format_json(message)
        file.write(f"[{timestamp}]\n{formatted_json}\n{'='*50}\n")
        file.flush()

    def get_last_message(self, log_type: str):
        file_mapping = {
            'to_openai': 'python_to_openai.log',
            'from_openai': 'openai_to_python.log',
            'from_unity': 'unity_to_python.log',
            'to_unity': 'python_to_unity.log'
        }
        
        if log_type not in file_mapping:
            return f"Invalid log type. Choose from: {', '.join(file_mapping.keys())}"
            
        filepath = os.path.join(self.log_dir, file_mapping[log_type])
        if not os.path.exists(filepath):
            return f"No log file found at {filepath}"
            
        try:
            with open(filepath, 'r') as f:
                content = f.read().strip().split('='*50)
                return content[-2].strip() if len(content) > 1 else "No messages logged yet"
        except Exception as e:
            return f"Error reading log: {str(e)}"

    def cleanup(self):
        self.python_to_openai.close()
        self.openai_to_python.close()
        self.unity_to_python.close()
        self.python_to_unity.close()