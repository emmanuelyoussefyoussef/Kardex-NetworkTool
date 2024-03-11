using System;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//route add 10.222.222.0 mask 255.255.255.0 10.222.222.1 if 2  --> das leitet alles an die gateway vom maschinennetz weiter falls die IP-Adresse im bereich von 10.222.222.1/254
class Main_Programy
{
    static void Main()
    {
            ManipulateIPAddress();
    }

    static void ManipulateIPAddress()
    {
        
        Console.WriteLine(AddedRoutes);

        Console.WriteLine("Available network interfaces:");

        // Run PowerShell command to get network information
        string powershellCommand = @"
            Get-NetAdapter | ForEach-Object {
                $Interface = $_
                $IPConfiguration = Get-NetIPConfiguration -InterfaceIndex $Interface.InterfaceIndex
                $DNS = ($IPConfiguration.DNSServer.ServerAddresses | Where-Object { $_ -like '*.*.*.*' })
                if (-not $DNS) {
                    $DNS = 'NaN'
                }
                [PSCustomObject]@{
                    InterfaceIndex = $Interface.InterfaceIndex
                    InterfaceAlias = $Interface.InterfaceAlias
                    Status = $Interface.Status
                    IPAddress = $IPConfiguration.IPv4Address.IPAddress
                    SubnetMask = $IPConfiguration.IPv4Address.PrefixLength
                    Gateway = $IPConfiguration.IPv4DefaultGateway.NextHop
                    DNS = $DNS
                }
            } | Format-Table -AutoSize | Out-String -Width 4096
            ";

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "powershell";
        psi.Arguments = "-Command " + powershellCommand;
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;

        Process process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Console.WriteLine(output);

        //// Prompt user for network interface name
        //Console.Write("Enter the name of the network interface you want to configure: ");
        //string interfaceName = Console.ReadLine();

        // Prompt user for new IP address
        Console.Write("Which IP_Address do you want to add? ");
        string newIP = Console.ReadLine();

        // Validate IP address format using regex
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        if (!Regex.IsMatch(newIP, pattern))
        {
            Console.WriteLine("Invalid IP address format.");
            return;
        }

        // Prompt user for new subnet mask
        Console.Write("Which Subnetmask do you want to use? ");
        string newSubnetMask = Console.ReadLine();
        
        if (!Regex.IsMatch(newSubnetMask, pattern))
        {
            Console.WriteLine("Invalid SubnetMask format.");
            return;
        }
       

        // Prompt user for new gateway
        Console.Write("Which Gateway do you want to use? ");
        string newGateway = Console.ReadLine();
        if (!Regex.IsMatch(newGateway, pattern))
        {
            Console.WriteLine("Invalid Gateway format.");
            return;
        }

        // Prompt user for the InterfaceIndex
        Console.WriteLine("Which interface do you want to use?");
        string interfaceIndex = Console.ReadLine();

        

        Process process2 = Process.Start("route", $"delete 0.0.0.0 mask 0.0.0.0 {newGateway} if {interfaceIndex}");
        Process process3 = Process.Start("route", $"add {newIP} mask {newSubnetMask} {newGateway} if {interfaceIndex}");
        process2.WaitForExit();
        process3.WaitForExit();

        

        Console.WriteLine("Do you want to add another Route? (y/n)");
        string answer = Console.ReadLine();
        if (answer == "y")
        {
            Main();
        }
        else
        {
            return;
        }

        //// Run netsh command to set new IP address, subnet mask, and gateway
        //Process.Start("netsh", $"interface ipv4 set address name=\"{interfaceName}\" static {newIP} {newSubnetMask} {newGateway}");

        //Console.WriteLine($"The IP address of {interfaceName} has been changed to {newIP}.");
        //Console.WriteLine("Updated network addresses:");
        //Process.Start("powershell", powershellCommand);



    }
}
