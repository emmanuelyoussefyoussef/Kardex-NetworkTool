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
using System.Collections.ObjectModel;

namespace Network_Window
    //test
{
    public partial class MainWindow : Window
    {
        private terminalCommand terminalCommand = new terminalCommand();
        
        //private string impIP;
        //private string impMask;
        //private string impGateway;
        //private string impIndex;
        private string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        
        
        Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        Dictionary<int, Tuple<string, string, string, string>> Netzwerke = new Dictionary<int, Tuple<string, string, string, string>>();

        public int counter = 1;
        public string[] interfaces;
        Boolean Maschinennetz = false;
        Boolean Internet = false;
        int Current_Network = 0;
        regularExpressions regularExpressions = new regularExpressions();
        public MainWindow()
        {
            InitializeComponent();
            regularExpressions regex = new regularExpressions(this);
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
            regularExpressions.setImpIP(IP_Adresse_Block.Text);
            regularExpressions.setImpMask(Mask_Block.Text);
            regularExpressions.setImpGateway(Gate_Block.Text);
            regularExpressions.setImpIndex(Index_Block.Text);

            regularExpressions.patternValiduation();
            
            output.Text = regularExpressions.getOutput();


            if (regularExpressions.getIsValid())
            {
                IP_Adresse_Block.Clear();
                Mask_Block.Clear();
                Gate_Block.Clear();
                Index_Block.Clear();

                terminalCommand.setCommand("route delete 0.0.0.0 mask 0.0.0.0 " + regularExpressions.getImpGateway());

                if (!string.IsNullOrWhiteSpace(terminalCommand.getError()))
                {
                    output.Text = $"Error: {terminalCommand.getError()}";
                }
                else output.Text = terminalCommand.getOutput();



                terminalCommand.setCommand("route add " + regularExpressions.getImpIP() + " mask " + regularExpressions.getImpMask() + " " + regularExpressions.getImpGateway() + " if " + regularExpressions.getImpIndex());


                if (!string.IsNullOrWhiteSpace(terminalCommand.getError()))
                {
                    output.Text = $"Error: {terminalCommand.getError()}";
                }
                else
                {
                    Added_Routes[counter] = Tuple.Create(regularExpressions.getImpIP(), regularExpressions.getImpMask(), regularExpressions.getImpGateway(), regularExpressions.getImpIndex());
                    output.Text = terminalCommand.getOutput();

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
            }else output.Text = regularExpressions.getOutput();
            
        }
        private void Netzwerk_Button(object sender, RoutedEventArgs e)
        {
            GridContainer.Children.Clear();
            string command = @"
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

            string output = ShellCommand(command);

            //Unterteilt die Ausgabe in Zeilen
            string[] rows = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> rowVariables = new List<string[]>();

            //Untereilt die Zeilen in Spalten
            foreach (string row in rows)
            {
                string[] columns = row.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                rowVariables.Add(columns);
            }

            string test = null;

            int count = rowVariables.Count - 2;





            //ButtonItems = new ObservableCollection<string>();


            int x = 0; // Column index
            int v = 0;


            string column_value = null;
            string index = "";
            string ip = "";
            string subnetMask = "";
            string gateway = "";

            for (int j = 2; j < rowVariables.Count; j++)
            {
                int runner = j - 1;
                string[] testRow = rowVariables[j];



                for (int i = 0; i < testRow.Length; i++)
                {
                    test += $"{testRow[i]} ";
                    column_value = testRow[i];
                    index = testRow[0];
                    ip = testRow[3];
                    subnetMask = testRow[4];
                    gateway = testRow[5];
                    CreateRows(column_value, v, x);
                    x++;
                    if (x >= 7)
                    {
                        CheckBox checkBox = new CheckBox();
                        checkBox.Content = "Hinzuf gen";
                        checkBox.FontSize = 12;
                        checkBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                        checkBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                        checkBox.Checked += (sender, e) => CheckBox_Checked(runner);
                        Grid.SetRow(checkBox, v);
                        Grid.SetColumn(checkBox, x + 1);
                        GridContainer.Children.Add(checkBox);
                        x = 0;
                        v++;

                    }


                }

                Tuple<string, string, string, string> value = Tuple.Create($"{ip}", $"{subnetMask}", $"{gateway}", $"{index}");
                Netzwerke[runner] = value;
                //ButtonItems.Add(test);
                test = null;
            }

            //ButtonContainer.ItemsSource = ButtonItems;

        }

        private void CheckBox_Checked(int value)
        {
            MessageBox.Show($"{Netzwerke[value].Item1} mask {Netzwerke[value].Item2} {Netzwerke[value].Item3} if {Netzwerke[value].Item4}");
            Current_Network = value;

        }
        private void CreateRows(string value, int row, int column)
        {
            int x = row;
            int y = column;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = $"{value}";
            textBlock.FontSize = 12;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Width = 100;
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            Grid.SetRow(textBlock, x);
            Grid.SetColumn(textBlock, y);
            GridContainer.Children.Add(textBlock);
        }


        private void Route_1_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(1, Route_1.Text);
        }
        private void Route_2_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(2, Route_2.Text);
        }
        private void Route_3_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(3, Route_3.Text);
        }
        private void Route_4_Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteRoute(4, Route_4.Text);
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

            string command = "route print -4;pause";

            ProcessStartInfo startInfo_routen_button = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"[Console]::SetWindowSize(100, 30);{command}\"",
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };
            process.StartInfo = startInfo_routen_button;

            process.Start();
        }

        private void Löschen_Button_Click(object sender, RoutedEventArgs e)
        {

            string command_only_IP = $"route delete {regularExpressions.getImpIP()}";
            string command_no_index = $"route delete {regularExpressions.getImpIP()} mask {regularExpressions.getImpMask()} {regularExpressions.getImpGateway()}";
            string command_with_index = $"route delete {regularExpressions.getImpIP()} mask {regularExpressions.getImpMask()} {regularExpressions.getImpGateway()} if {regularExpressions.getImpIndex()}";

            //Wenn kein index angegeben wurde
            if (string.IsNullOrEmpty(regularExpressions.getImpIndex()))
            {
                output.Text = ShellCommand(command_no_index);
            }
            //Wenn keine Maske angegeben wurde
            else if (string.IsNullOrEmpty(regularExpressions.getImpMask()))
            {
                output.Text = ShellCommand(command_only_IP);
            }
            else
            {
                output.Text = ShellCommand(command_with_index);
            }
        }

        //Diese methode ist f r die Routen L schen, sie l scht alle eintr ge in den Routen vereichnis und setzt alle meine Routeboxes auf leer.
        private void Alle_Routen_Löschen(object sender, RoutedEventArgs e)
        {
            ShellCommand("route -f");
            Route_1.Text = "";
            Route_2.Text = "";
            Route_3.Text = "";
            Route_4.Text = "";
            counter = 1;
        }

        private string ShellCommand(string command)
        {
            Process process = new Process();

            ProcessStartInfo argument = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            process.StartInfo = argument;

            process.Start();

            process.WaitForExit();

            string error = process.StandardError.ReadToEnd();
            string response = process.StandardOutput.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(error))
            {
                string output = error;

                return output;
            }
            else
            {
                string output = response;
                return output;
            }
        }

        private void Maschinen_netz_box_Click(object sender, RoutedEventArgs e)
        {
            Maschinennetz = !Maschinennetz;
            if (Maschinennetz)
            {
                Internet_box.IsEnabled = false;
                Internet = false;
            }
            else
            {
                Internet_box.IsEnabled = true;
            }
        }

        private void Internet_box_Click(object sender, RoutedEventArgs e)
        {
            Internet = !Internet;
            if (Internet)
            {
                Maschinen_netz_box.IsEnabled = false;
                Maschinennetz = false;
                Route_Add_Automatically(Current_Network);
            }
            else
            {
                Maschinen_netz_box.IsEnabled = true;
            }
        }

        private void Route_Add_Automatically(int value)
        {
            Process route_delete = new Process();

            string command_route_delete = ("route delete 0.0.0.0 mask 0.0.0.0 " + Netzwerke[value].Item3);

            // Set up the process start info
            ProcessStartInfo startInfo_route_delete = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete};route delete {Netzwerke[value].Item1} mask {Netzwerke[value].Item2} {Netzwerke[value].Item3}\"", // Pass the command as argument
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

            string command_route_add = ("route add " + Netzwerke[value].Item1 + " mask " + Netzwerke[value].Item2 + " " + Netzwerke[value].Item3 + " if " + Netzwerke[value].Item4);

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
                Added_Routes[counter] = Tuple.Create(Netzwerke[value].Item1, Netzwerke[value].Item2, Netzwerke[value].Item3, Netzwerke[value].Item4);
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
}