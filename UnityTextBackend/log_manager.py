
import os
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

    def log_message(self, file, message):
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
        file.write(f"[{timestamp}] {message}\n")
        file.flush()

    def cleanup(self):
        self.python_to_openai.close()
        self.openai_to_python.close()
        self.unity_to_python.close()
        self.python_to_unity.close()