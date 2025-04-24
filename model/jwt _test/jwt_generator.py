import jwt
from cryptography.hazmat.primitives import serialization
import time
import secrets
import os 
from dotenv import load_dotenv

class JWTGenerator:

    def __init__(self):
        load_dotenv('../.env')  # loads the .env file from one directory up

        # Access variables
        self.db_user = os.getenv("DB_USER")
        print(f"DB user is: {self.db_user}")

        # Fetch values from exported environment variables
        self.key_name = os.getenv('KEY_NAME')  
        self.key_secret = os.getenv('KEY_SECRET') 
        self.request_method = os.getenv('REQUEST_METHOD')  
        self.request_host = os.getenv('REQUEST_HOST')  
        self.request_path = os.getenv('REQUEST_PATH')  

    def build_jwt(self):
        uri = f"{self.request_method} {self.request_host}{self.request_path}"
        private_key_bytes = self.key_secret.encode('utf-8')
        private_key = serialization.load_pem_private_key(private_key_bytes, password=None)
        jwt_payload = {
            'sub': self.key_name,
            'iss': "cdp",
            'nbf': int(time.time()),
            'exp': int(time.time()) + 120,
            'uri': uri,
        }
        jwt_token = jwt.encode(
            jwt_payload,
            private_key,
            algorithm='ES256',
            headers={'kid': self.key_name, 'nonce': secrets.token_hex()},
        )
        return jwt_token