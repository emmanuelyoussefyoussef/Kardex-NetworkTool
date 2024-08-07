using System.Diagnostics;

namespace Network_Window
{
    public class TerminalCommand
    {
        public string Command { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }

        private string GenerateNetwork = @"
                    Get-NetAdapter | ForEach-Object {
    $Interface = $_
    $InterfaceIndex = $Interface.InterfaceIndex

    try {
        $NetIPInterface = Get-NetIPInterface -InterfaceIndex $InterfaceIndex -ErrorAction Stop
    } catch {
        return
    }

    try {
        $IPConfiguration = Get-NetIPConfiguration -InterfaceIndex $InterfaceIndex -ErrorAction Stop
    } catch {
        return
    }

    $DNS = ($IPConfiguration.DNSServer.ServerAddresses | Where-Object { $_ -like '*.*.*.*' }) -join ','
    if (-not $DNS) {
        $DNS = 'EMPTY'
    }

    $SubnetMaskCIDR = $IPConfiguration.IPv4Address.PrefixLength
    $SubnetMaskDottedDecimal = (([math]::pow(2, $SubnetMaskCIDR) - 1) -shl (32 - $SubnetMaskCIDR)) -band 0xFFFFFFFF
    if ($SubnetMaskDottedDecimal -lt 0) {
        $SubnetMaskDottedDecimal += [math]::pow(2, 32)
    }
    $SubnetMaskDottedDecimal = ([ipaddress]$SubnetMaskDottedDecimal).IPAddressToString

    $InterfaceAlias = $Interface.InterfaceAlias -replace ' ', '_'
    $DNS = $DNS -replace ' ', '_'
    $Gateway = if ($IPConfiguration.IPv4DefaultGateway.NextHop) { $IPConfiguration.IPv4DefaultGateway.NextHop -replace ' ', '_' } else { 'EMPTY' }
    $IPAddress = if ($IPConfiguration.IPv4Address.IPAddress) { $IPConfiguration.IPv4Address.IPAddress -replace ' ', '_' } else { 'EMPTY' }
    $SubnetMask = if ($SubnetMaskDottedDecimal) { $SubnetMaskDottedDecimal -replace ' ', '_' } else { 'EMPTY' }
    $Status = if ($Interface.Status) { $Interface.Status -replace ' ', '_' } else { 'EMPTY' }

    [PSCustomObject]@{
        Index = $InterfaceIndex
        InterfaceAlias = $InterfaceAlias
        Status = $Status
        IPAddress = $IPAddress
        SubnetMask = $SubnetMask
        Gateway = $Gateway
        DNS = $DNS
    }
} | Format-Table -AutoSize | Out-String -Width 4096
";
        public void CommandShell(string _Command)
        {
            Process process = new Process();

            ProcessStartInfo processStart = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{_Command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = processStart;

            process.Start();

            process.WaitForExit();

            Output = process.StandardOutput.ReadToEnd();
            Error = process.StandardError.ReadToEnd();

            process.WaitForExit();
        }
        public void GenerateNetworks()
        {
            CommandShell(GenerateNetwork);
        }
    }
}