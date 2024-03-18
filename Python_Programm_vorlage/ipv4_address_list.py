import netifaces
from requests import get


# Netzwerkschnittstellen abrufen
interface_list = netifaces.interfaces()

# Adressinformationen für jede Schnittstelle abrufen
for iface in interface_list:
    # Schnittstellenname im Klartext erhalten
    interface_name = netifaces.ifaddresses(iface)[netifaces.AF_LINK][0]['addr']
    
    # Adresseninformationen für IPv4 abrufen
    addresses = netifaces.ifaddresses(iface)
    
    # Nur IPv4-Adressen betrachten
    if netifaces.AF_INET in addresses:
        ipv4_address = addresses[netifaces.AF_INET][0]['addr']
        print(f"Interface: {interface_name}, IPv4 Address: {ipv4_address}")

#ip = get('https://api.ipify.org').content.decode('utf8')
#print('My public IP address is: {}'.format(ip))
