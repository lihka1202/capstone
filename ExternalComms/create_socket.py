import socket


def create_socket(host, port):
    """Create, connect, and return a socket for the given host and port."""
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        sock.connect((host, port))
        print(f"Connected to {host}:{port}")
        return sock
    except Exception as e:
        print(f"Error connecting to {host}:{port} - {e}")
        return None
