import netifaces
import subprocess
# Liste aller Netzwerkschnittstellen
interfaces = netifaces.interfaces()

for interface in interfaces:
    print(f"Interface: {interface}")
    
    try:
        # Versuchen Sie, die IP-Adresse der Schnittstelle zu bekommen
        ip_address = netifaces.ifaddresses(interface)[netifaces.AF_INET][0]['addr']
        print(f"  IP-Adresse: {ip_address}")
    except KeyError:
        print("  IP-Adresse: Nicht vorhanden")
    
    print()
