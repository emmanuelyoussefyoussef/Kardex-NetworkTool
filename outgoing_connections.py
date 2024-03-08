import psutil
import socket

def get_outgoing_connections():
    connections = psutil.net_connections(kind='inet')
    outgoing_connections = []

    for conn in connections:
        if conn.status == psutil.CONN_ESTABLISHED and conn.raddr:
            outgoing_connections.append((conn.laddr.ip, conn.raddr.ip))

    return outgoing_connections

if __name__ == "__main__":
    outgoing_connections = get_outgoing_connections()
    print("Outgoing Connections:")
    for local_ip, remote_ip in outgoing_connections:
        print(f"Local IP: {local_ip}\t Remote IP: {remote_ip}")