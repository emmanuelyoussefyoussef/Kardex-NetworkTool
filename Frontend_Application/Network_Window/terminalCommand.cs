using System.Diagnostics;

namespace Network_Window
{
    public class TerminalCommand
    {
        public string Command { get; set;}
        public string Output { get; set;}
        public string Error { get; set; }

        private string GenerateNetwork = @"
                    Get-NetAdapter | ForEach-Object {
                    $Interface = $_
                    $IPConfiguration = Get-NetIPConfiguration -InterfaceIndex $Interface.InterfaceIndex
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

                    [PSCustomObject]@{
                        Index = $Interface.InterfaceIndex
                        InterfaceAlias = $InterfaceAlias
                        Status = if ($Interface.Status) { $Interface.Status } else { 'EMPTY' }
                        IPAddress = if ($IPConfiguration.IPv4Address.IPAddress) { $IPConfiguration.IPv4Address.IPAddress } else { 'EMPTY' }
                        SubnetMask = if ($SubnetMaskDottedDecimal) { $SubnetMaskDottedDecimal } else { 'EMPTY' }
                        Gateway = if ($IPConfiguration.IPv4DefaultGateway.NextHop) { $IPConfiguration.IPv4DefaultGateway.NextHop } else { 'EMPTY' }
                        DNS = $DNS
                    }
                } | Format-Table -AutoSize | Out-String -Width 4096";
        public void CommandShell(string _Command) {
            
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
