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
using System.Collections.Generic;
using System.Data;
using System.Reflection;

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
        Dictionary<int,Tuple<string,string,string,string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        int counter = 1;

        public MainWindow()
        {
            InitializeComponent();
            Netzwerk_Button(null, null);


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
                output.Text = "IP Adresse ist ungültig";
            }
            else if (!Regex.IsMatch(impMask, pattern))
            {
                output.Text = "Subnetzmaske ist ungültig";
            }
            else if (!Regex.IsMatch(impGateway, pattern))
            {
                output.Text = "Gateway ist ungültig";
            }
            else if (counter==5)
            {
                output.Text = "Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.";
            }
            else
            {
                IP_Adresse_Block.Clear();
                Mask_Block.Clear();
                Gate_Block.Clear();
                Index_Block.Clear();

                

                Process route_delete = new Process();

                string command_route_delete = ("route delete 0.0.0.0 mask 0.0.0.0 " + impGateway);

                // Set up the process start info
                ProcessStartInfo startInfo_route_delete = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete};route delete {impIP} mask {impMask} {impGateway}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                route_delete.StartInfo = startInfo_route_delete;

                // Start the process
                route_delete.Start();

                route_delete.WaitForExit();

                string output_route_delete = route_delete.StandardOutput.ReadToEnd();
                string error_route_delete = route_delete.StandardError.ReadToEnd();

                route_delete.WaitForExit();
                
                
                if (!string.IsNullOrWhiteSpace(error_route_delete))
                {
                    output.Text = $"Error: {error_route_delete}";
                }
                else
                {
                    output.Text = output_route_delete;
                }


                Process route_add = new Process();

                string command_route_add = ("route add " + impIP + " mask " + impMask + " " + impGateway + " if " + impIndex);

                // Set up the process start info
                ProcessStartInfo startInfo_route_add = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_add}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                route_add.StartInfo = startInfo_route_add;

                // Start the process
                route_add.Start();

                route_add.WaitForExit();

                string output_route_add = route_add.StandardOutput.ReadToEnd();
                string error_route_add = route_add.StandardError.ReadToEnd();

                route_add.WaitForExit();


                if (!string.IsNullOrWhiteSpace(error_route_add))
                {
                    output.Text = $"Error: {error_route_add}";
                }
                else
                {
                    Added_Routes[counter]=Tuple.Create(impIP, impMask, impGateway, impIndex);
                    output.Text = output_route_add;

                    string text = $"IP Adresse: {Added_Routes[counter].Item1}\nSubnetMaske: {Added_Routes[counter].Item2}\nGateway: {Added_Routes[counter].Item3}\nSchnittstellenindex: {Added_Routes[counter].Item4}\n";

                    switch (counter)
                    {
                        case 1:
                            Route_1.Text = text;
                            break;
                        case 2:
                            Route_2.Text = text;
                            break;
                        case 3:
                            Route_3.Text = text;
                            break;
                        case 4:
                            Route_4.Text = text;
                            break;
                    }
                    counter++;
                }
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
                } | Format-Table -AutoSize | Out-String -Width 4096";

            // Set up the process start info
            ProcessStartInfo Netzwerke = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"", // Pass the command as argument
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = Netzwerke;

            // Start the process
            process.Start();

            // Read the output and display it in a TextBlock
            string Tabelle = process.StandardOutput.ReadToEnd();
            Netzwerk_Tabelle.Text = Tabelle;

            process.WaitForExit();

        }

        private void Route_1_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(1,Route_1.Text);
        }
        private void Route_2_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(2,Route_2.Text);
        }
        private void Route_3_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(3,Route_3.Text);
        }
        private void Route_4_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(4,Route_4.Text);
        }
        private void deleteRoute(int index, string boxnr)
        {

            if (string.IsNullOrWhiteSpace(boxnr))
            {
                output.Text = "keine route";
            }
            else
            {
                Process route_delete_box1 = new Process();

                string command_route_delete_box1 = ($"route delete {Added_Routes[index].Item1} mask {Added_Routes[index].Item2} {Added_Routes[index].Item3} if {Added_Routes[index].Item4}");

                //Set up the process start info
                ProcessStartInfo startInfo_route_delete_box_1 = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete_box1}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                route_delete_box1.StartInfo = startInfo_route_delete_box_1;

                // Start the process
                route_delete_box1.Start();

                route_delete_box1.WaitForExit();

                string output_route_delete_box1 = route_delete_box1.StandardOutput.ReadToEnd();
                string error_route_delete_box1 = route_delete_box1.StandardError.ReadToEnd();

                route_delete_box1.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error_route_delete_box1))
                {
                    output.Text = error_route_delete_box1;
                }
                else
                {
                    output.Text = output_route_delete_box1;
                    Added_Routes.Remove(counter);
                    switch (index)
                    {
                        case 1:
                            Route_1.Text = "";
                            break;
                        case 2:
                            Route_2.Text = "";
                            break;
                        case 3:
                            Route_3.Text = "";
                            break;
                        case 4:
                            Route_4.Text = "";
                            break;
                        default:
                            break;
                    }
                    counter = index;
                    

                }
            }
        }

        
        private void Routen_Button(object sender, RoutedEventArgs e)
        {
            Process process = new Process();

            // Specify the PowerShell command to execute
            string command = "route print -4;pause";

            // Set up the process start info
            ProcessStartInfo startInfo_routen_button = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"[Console]::SetWindowSize(100, 30);{command}\"", // Pass the command as argument
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            process.StartInfo = startInfo_routen_button;

            // Start the process
            process.Start();
        }

        private void Löschen_Button_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();

            // Specify the PowerShell command to execute
            string command_only_IP = $"route delete {impIP}";
            string command_no_index = $"route delete {impIP} mask {impMask} {impGateway}";
            string command_with_index = $"route delete {impIP} mask {impMask} {impGateway} if {impIndex}";

            
            //Wenn kein index angegeben wurde
            if (string.IsNullOrEmpty(impIndex))
            {
                ProcessStartInfo startInfo_routen_delete_no_index = new ProcessStartInfo
                {
                    FileName = "powershell.exe",  // Specify PowerShell executable
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_no_index}\"", // Pass the command as argument
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.StartInfo = startInfo_routen_delete_no_index;

                // Start the process
                process.Start();

                process.WaitForExit();

                string error_route_delete_no_index = process.StandardError.ReadToEnd();
                string output_route_delete_no_index = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error_route_delete_no_index))
                {
                    output.Text = error_route_delete_no_index;
                }
                else
                {
                    output.Text = output_route_delete_no_index;
                }

            }
            //Wenn keine Maske angegeben wurde
            else if (string.IsNullOrEmpty(impMask))
            {
                ProcessStartInfo routen_delete_only_IP = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_only_IP}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                process.StartInfo = routen_delete_only_IP;

                // Start the process
                process.Start();

                process.WaitForExit();

                string error_route_delete_only_IP = process.StandardError.ReadToEnd();
                string output_route_delete_only_IP = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error_route_delete_only_IP))
                {
                    output.Text = error_route_delete_only_IP;
                }
                else
                {
                    output.Text = output_route_delete_only_IP;
                }
            }
            else
            {
                ProcessStartInfo startInfo_routen_delete_index = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_with_index}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.StartInfo = startInfo_routen_delete_index;

                // Start the process
                process.Start();

                process.WaitForExit();

                string error_route_delete_index = process.StandardError.ReadToEnd();
                string output_route_delete_index = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error_route_delete_index))
                {
                    output.Text = $"Error: {error_route_delete_index}";
                }
                else
                {
                    output.Text = output_route_delete_index;
                }
            }
        }

        //Diese methode ist für die Routen Löschen, sie löscht alle einträge in den Routen vereichnis und setzt alle meine Routeboxes auf leer.
        private void Alle_Routen_Löschen(object sender, RoutedEventArgs e)
        {
            Process route_delete = new Process();

            string command_route_delete = ("route -f");

            // Set up the process start info
            ProcessStartInfo startInfo_route_delete = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            route_delete.StartInfo = startInfo_route_delete;

            // Start the process
            route_delete.Start();

            Route_1.Text = "";
            Route_2.Text = "";
            Route_3.Text = "";
            Route_4.Text = "";
        }
    }
}
