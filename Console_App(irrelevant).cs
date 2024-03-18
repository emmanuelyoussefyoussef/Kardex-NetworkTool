using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        ManipulateIPAddress();
    }

    static void ManipulateIPAddress()
    {
        Console.WriteLine("Available network interfaces:");

        // Run PowerShell command to get network information
        string powershellCommand = @"
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

        // Prompt user for network interface name
        Console.Write("Enter the name of the network interface you want to configure: ");
        string interfaceName = Console.ReadLine();

        // Prompt user for new IP address
        Console.Write("Enter the new IP address: ");
        string newIP = Console.ReadLine();

        // Validate IP address format using regex
        string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        if (!Regex.IsMatch(newIP, pattern))
        {
            Console.WriteLine("Invalid IP address format.");
            return;
        }

        // Prompt user for new subnet mask
        Console.Write("Enter the new subnet mask: ");
        string newSubnetMask = Console.ReadLine();

        // Prompt user for new gateway
        Console.Write("Enter the new gateway: ");
        string newGateway = Console.ReadLine();

        // Run netsh command to set new IP address, subnet mask, and gateway
        Process.Start("netsh", $"interface ipv4 set address name=\"{interfaceName}\" static {newIP} {newSubnetMask} {newGateway}");

        Console.WriteLine($"The IP address of {interfaceName} has been changed to {newIP}.");
        Console.WriteLine("Updated network addresses:");
        Process.Start("powershell", powershellCommand);
    }
}
