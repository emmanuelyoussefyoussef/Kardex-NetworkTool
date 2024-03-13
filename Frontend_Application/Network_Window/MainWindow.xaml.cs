using Network_Window.Info_button;
using System;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Network_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string impIP;
        private string impMask;
        private string impGateway;
        private string impIndex;
        private string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        public MainWindow()
        {
            //ManipulateIPAddress();
            InitializeComponent();


        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Info_Fenster objInfo_Fenster = new Info_Fenster();
            this.Visibility = Visibility.Visible;
            objInfo_Fenster.Show();
        }

        private void Eingabe_Button_Click(object sender, RoutedEventArgs e)
        {
            impIP = IP_Adresse_Block.Text;
            impMask = Mask_Block.Text;
            impGateway = Gate_Block.Text;
            impIndex = Index_Block.Text;


            if (!Regex.IsMatch(impIP, pattern))
            {
                MessageBox.Show("IP Adresse ist ungültig");
            }
            else if (!Regex.IsMatch(impMask, pattern))
            {
                MessageBox.Show("Subnetzmaske ist ungültig");
            }
            else if (!Regex.IsMatch(impGateway, pattern))
            {
                MessageBox.Show("Subnetzmaske ist ungültig");
            }
            else
            {
                IP_Adresse_Block.Clear();
                Mask_Block.Clear();
                Gate_Block.Clear();
                Index_Block.Clear();

                Process route_delete = new Process();

                string command_route_delete = ("route delete 0.0.0.0 mask 0.0.0.0 "+impGateway+" if "+impIndex);

                // Set up the process start info
                ProcessStartInfo startInfo_route_delete = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                route_delete.StartInfo = startInfo_route_delete;

                // Start the process
                route_delete.Start();

                route_delete.WaitForExit();

                string output_route_delete = route_delete.StandardOutput.ReadToEnd();

                route_delete.WaitForExit();

                MessageBox.Show(output_route_delete);




                Process route_add = new Process();

                string command_route_add = ("route add "+impIP+" mask "+impMask+" "+impGateway+" if "+impIndex);

                // Set up the process start info
                ProcessStartInfo startInfo_route_add = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_add}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                route_add.StartInfo = startInfo_route_add;

                // Start the process
                route_add.Start();

                route_add.WaitForExit();

                string output_route_add = route_add.StandardOutput.ReadToEnd();

                route_add.WaitForExit();

                MessageBox.Show(output_route_add);
            }
            

            
            
        }



        

        private void Netzwerk_Button(object sender, RoutedEventArgs e)
        {
            Process process = new Process();

            // Specify the PowerShell command to execute
            string command = @"
                Get-NetAdapter | ForEach-Object {
                $Interface = $_
                $IPConfiguration = Get-NetIPConfiguration -InterfaceIndex $Interface.InterfaceIndex
                $DNS = ($IPConfiguration.DNSServer.ServerAddresses | Where-Object { $_ -like '*.*.*.*' })
                if (-not $DNS) {
                    $DNS = 'NaN'
                }
                [PSCustomObject]@{
                    Index = $Interface.InterfaceIndex
                    InterfaceAlias = $Interface.InterfaceAlias
                    Status = $Interface.Status
                    IPAddress = $IPConfiguration.IPv4Address.IPAddress
                    SubnetMask = $IPConfiguration.IPv4Address.PrefixLength
                    Gateway = $IPConfiguration.IPv4DefaultGateway.NextHop
                    DNS = $DNS
                }
            } | Format-Table -AutoSize | Out-String -Width 4096;pause";
           
            // Set up the process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"", // Pass the command as argument
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo;

            // Start the process
            process.Start();
        }
        

    }
}
            //process.WaitForExit();

            //Console.WriteLine(output);

            //// Prompt user for network interface name
            //Console.Write("Enter the name of the network interface you want to configure: ");
            //string interfaceName = Console.ReadLine();

            // Prompt user for new IP address
            //Console.Write("Which IP_Address do you want to add? ");
            //string newIP = Console.ReadLine();

            //// Validate IP address format using regex
            //string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            //if (!Regex.IsMatch(newIP, pattern))
            //{
            //    Console.WriteLine("Invalid IP address format.");
            //    return;
            //}

            //// Prompt user for new subnet mask
            //Console.Write("Which Subnetmask do you want to use? ");
            //string newSubnetMask = Console.ReadLine();

            //if (!Regex.IsMatch(newSubnetMask, pattern))
            //{
            //    Console.WriteLine("Invalid SubnetMask format.");
            //    return;
            //}


            //// Prompt user for new gateway
            //Console.Write("Which Gateway do you want to use? ");
            //string newGateway = Console.ReadLine();
            //if (!Regex.IsMatch(newGateway, pattern))
            //{
            //    Console.WriteLine("Invalid Gateway format.");
            //    return;
            //}

            //// Prompt user for the InterfaceIndex
            //Console.WriteLine("Which interface do you want to use?");
            //string interfaceIndex = Console.ReadLine();



            //Process process2 = Process.Start("route", $"delete 0.0.0.0 mask 0.0.0.0 {newGateway} if {interfaceIndex}");
            //Process process3 = Process.Start("route", $"add {newIP} mask {newSubnetMask} {newGateway} if {interfaceIndex}");
            //process2.WaitForExit();
            //process3.WaitForExit();



            //Console.WriteLine("Do you want to add another Route? (y/n)");
            //string answer = Console.ReadLine();
            //if (answer == "y")
            //{
            //    Main();
            //}
            //else
            //{
            //    return;
            //}
        
