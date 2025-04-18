import base64
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad


def encrypt_message(plain_text, iv, aes_bytes):
    try:
        cipher = AES.new(aes_bytes, AES.MODE_CBC, iv)  # Create AES cipher
        padded_message = pad(plain_text.encode('utf-8'), AES.block_size)  # Pad message
        encrypted_message = cipher.encrypt(padded_message)  # Encrypt the padded message
        final_message = base64.b64encode(iv + encrypted_message).decode("utf-8")  # Prepend IV and encode
        return final_message
    except Exception as e:
        print(f"Encryption error: {e}")
        return None


def format_message(encrypted_message):
    message_length = len(encrypted_message)  # 4-byte big-endian length
    byte = encrypted_message.encode("utf-8")
    message = str(message_length).encode("utf-8") + b"_" + byte
    return message
