import subprocess
import os

# Split the output into lines and remove any leading or trailing whitespace
network_names = subprocess.run(["powershell", "Get-NetAdapter | Select-Object -Property Name"], capture_output=True, text=True).stdout.strip().split('\n')

print("zu verfügung stehen folgende Netzwerken zur Verfügung:")

#create a list of network names from the output with IP-Addresses
powershell_command = r'''
Get-NetAdapter | ForEach-Object {
    $Interface = $_
    $IPAddress = Get-NetIPAddress -InterfaceIndex $Interface.InterfaceIndex
    [PSCustomObject]@{
        Name = $Interface.Name
        Status = $Interface.Status
        IPAddress = $IPAddress.IPAddress
    }
} | Format-Table -AutoSize
'''
result = subprocess.run(f"powershell {powershell_command}")

interface_name = input("Welches Netzwerk möchten Sie konfigurieren?")
#checks if the input is in the list of network names
if interface_name not in network_names:
    print("Das Netzwerk existiert nicht.")
    os._exit(1)


print("Wie soll die IP-Adresse lauten?")
new_ip = input()

print(f"Die IP-Adresse von {interface_name} wird auf {new_ip} geändert.")
if subprocess.run(f'netsh interface ipv4 set address name="{interface_name}" static {new_ip}', shell=True):
    print("successful")
