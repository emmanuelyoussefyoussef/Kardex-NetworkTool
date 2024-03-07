import subprocess
import os
import re

def manipulate_ip_address():
    """
    This function allows the user to configure network settings by changing the IP address, subnet mask, and gateway of a selected network interface.

    It retrieves a list of available network interfaces and their IP addresses using PowerShell commands. Then, it prompts the user to select a network interface and enter the new IP address, subnet mask, and gateway.

    If the input is valid, it updates the network interface with the new settings using the `netsh` command and displays the updated network addresses.

    Note: This function requires PowerShell to be installed on the system.

    Returns:
        None
    """
    # Split the output into lines and remove any leading or trailing whitespace
    network_names = subprocess.run(["powershell", "Get-NetAdapter | Select-Object -Property Name"], capture_output=True, text=True).stdout.strip().split('\n')

    print("zu verfügung stehen folgende Netzwerken zur Verfügung:")

    # create a list of network names from the output with IP-Addresses
    powershell_command = r'''
    Get-NetAdapter | ForEach-Object {
        $Interface = $_
        $IPConfiguration = Get-NetIPConfiguration -InterfaceIndex $Interface.InterfaceIndex
        [PSCustomObject]@{
            Name = $Interface.Name
            Status = $Interface.Status
            IPAddress = $IPConfiguration.IPv4Address.IPAddress
            SubnetMask = $IPConfiguration.IPv4Address
            Gateway = $IPConfiguration.IPv4DefaultGateway.NextHop
        }
    } | Format-Table -AutoSize | Out-String -Width 4096
    '''

    result = subprocess.run(["powershell", "-Command", powershell_command], capture_output=True, text=True)
    print(result.stdout)

    interface_name = input("Welches Netzwerk möchten Sie konfigurieren?")
    # checks if the input is in the list of network names
    if interface_name not in network_names:
        print("Das Netzwerk existiert nicht.")
        os._exit(1)

    new_ip = input("Wie soll die IP-Adresse lauten? ")

    # checks if the input is a valid IP-Address
    pattern="^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
    if not re.match(pattern, new_ip):
        print("Die IP-Adresse ist nicht gültig.")
        os._exit(1)

    new_subnetmask=input("wie soll die neue subnetmaske lauten? ")

    new_gateway=input("wie soll die neue gateway lauten? ")

    print(f"Die IP-Adresse von {interface_name} wird auf {new_ip} geändert.")
    if subprocess.run(f'netsh interface ipv4 set address name="{interface_name}" static {new_ip} {new_subnetmask} {new_gateway}', shell=True):
        print("successful")
        print("Aktualisierte Adressen")
        subprocess.run(f"powershell {powershell_command}")

# Call the function to execute the IP address manipulation
manipulate_ip_address()
