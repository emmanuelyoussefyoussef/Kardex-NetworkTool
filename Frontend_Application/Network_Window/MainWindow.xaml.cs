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
{
    public partial class MainWindow : Window
    {
        public int counter = 1;
        public int CurrentNetworkRowNumber = 0;
        Boolean Internet = false;
        Boolean Maschinennetz = false;
        public string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        public string[] interfaces;
       
        Dictionary<int, Tuple<string, string, string, string>> NetworkSpecification = new Dictionary<int, Tuple<string, string, string, string>>();
        Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };


        private TerminalCommand terminalCommand = new TerminalCommand();
        public MainWindow()
        {
            InitializeComponent();
            NetworkRefreshButton(null, null);
        }
        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Info_Fenster objInfo_Fenster = new Info_Fenster();
            this.Visibility = Visibility.Visible;
            objInfo_Fenster.Show();
        }//finished
        private void Hinzufügen_Button_Click(object sender, RoutedEventArgs e)
        {
            ImpIp = IP_Adresse_Block.Text;
            ImpMask = Mask_Block.Text;
            ImpGateway = Gate_Block.Text;
            ImpIndex = Index_Block.Text;




            if (!Regex.IsMatch(ImpIp, pattern))
            {
                output.Text = "IP Adresse ist ungültig";
            }
            else if (!Regex.IsMatch(ImpMask, pattern))
            {
                output.Text = "Subnetzmaske ist ungültig";
            }
            else if (!Regex.IsMatch(ImpGateway, pattern))
            {
                output.Text = "Gateway ist ungültig";
            }
            else if (counter == 5)
            {
                output.Text = "Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.";
            }
            else 
            {
                IP_Adresse_Block.Clear();
                Mask_Block.Clear();
                Gate_Block.Clear();
                Index_Block.Clear();

                terminalCommand.CommandShell("route delete 0.0.0.0 mask 0.0.0.0 " + ImpGateway);

                if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                {
                    output.Text = $"Error: {terminalCommand.Error}";
                }
                else output.Text = terminalCommand.Output;



                terminalCommand.CommandShell("route add " + ImpIp + " mask " + ImpMask + " " + ImpGateway + " if " + ImpIndex);


                if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                {
                    output.Text = $"Error: {terminalCommand.Error}";
                }
                else
                {
                    Added_Routes[counter] = Tuple.Create(ImpIp, ImpMask, ImpGateway, ImpIndex);
                    output.Text = terminalCommand.Output;

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

            //finished
            //finished
        }//finished
        private void NetworkRefreshButton(object sender, RoutedEventArgs e)
        {
            GridContainer.Children.Clear();

            terminalCommand.GenerateNetworks();

            int ColumnIndex = 0;
            int RowIndex = 0;
            string Index = "";
            string Ip = "";
            string SubnetMask = "";
            string Gateway = "";
            string ColumnValue = "";
            string[] CommandRawOutput = terminalCommand.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<string[]> CommandRawOutputAsRows = new List<string[]>();

            foreach (string RowRunner in CommandRawOutput)
            {
                string[] SingleNetworkAsRow = RowRunner.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                CommandRawOutputAsRows.Add(SingleNetworkAsRow);
            }

            int Count = CommandRawOutputAsRows.Count - 2;

            for (int j = 2; j < CommandRawOutputAsRows.Count; j++)
            {
                int Runner = j - 1;
                string[] OneNetworkRowData = CommandRawOutputAsRows[j];

                for (int i = 0; i < OneNetworkRowData.Length; i++)
                {
                    ColumnValue = OneNetworkRowData[i];

                    Index = OneNetworkRowData[0];

                    Ip = OneNetworkRowData[3];

                    SubnetMask = OneNetworkRowData[4];

                    Gateway = OneNetworkRowData[5];

                    CreateRows(ColumnValue, RowIndex, ColumnIndex);

                    ColumnIndex++;

                    if (ColumnIndex >= 7)
                    {
                        CreateCheckBox(Runner, RowIndex, ColumnIndex);
                        RowIndex++;
                        ColumnIndex = 0;
                    }
                }
                NetworkSpecification[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }
        }//finished
        private void CreateCheckBox(int runner,int rowIndex, int columnIndex) {
            
            CheckBox checkBox = new CheckBox();

            checkBox.Content = "Hinzufügen";

            checkBox.FontSize = 12;

            checkBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");

            checkBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);

            checkBox.Checked += (sender, e) => CheckBox_Checked(runner);

            Grid.SetRow(checkBox, rowIndex);

            Grid.SetColumn(checkBox, columnIndex + 1);

            GridContainer.Children.Add(checkBox);
        }//finished
        private void CheckBox_Checked(int value)
        {
            MessageBox.Show($"{NetworkSpecification[value].Item1} mask {NetworkSpecification[value].Item2} {NetworkSpecification[value].Item3} if {NetworkSpecification[value].Item4}");
            CurrentNetworkRowNumber = value;
        }//finished
        private void CreateRows(string value, int row, int column)
        {
            int Row = row;
            int Column = column;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = $"{value}";
            textBlock.FontSize = 12;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Width = 100;
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            Grid.SetRow(textBlock, Row);
            Grid.SetColumn(textBlock, Column);
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
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete_box1}\"", // Pass the Command as argument
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

            string command_only_IP = $"route delete {ImpIp}";
            string command_no_index = $"route delete {ImpIp} mask {ImpMask} {ImpGateway}";
            string command_with_index = $"route delete {ImpIp} mask {ImpMask} {ImpGateway} if {ImpIndex}";

            //Wenn kein Index angegeben wurde
            if (string.IsNullOrEmpty(ImpIndex))
            {
                output.Text = ShellCommand(command_no_index);
            }
            //Wenn keine Maske angegeben wurde
            else if (string.IsNullOrEmpty(ImpMask))
            {
                output.Text = ShellCommand(command_only_IP);
            }
            else
            {
                output.Text = ShellCommand(command_with_index);
            }
        }
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
                Route_Add_Automatically(CurrentNetworkRowNumber);
            }
            else
            {
                Maschinen_netz_box.IsEnabled = true;
            }
        }
        private void Route_Add_Automatically(int value)
        {
            Process route_delete = new Process();

            string command_route_delete = ("route delete 0.0.0.0 mask 0.0.0.0 " + NetworkSpecification[value].Item3);

            // Set up the process start info
            ProcessStartInfo startInfo_route_delete = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_delete};route delete {NetworkSpecification[value].Item1} mask {NetworkSpecification[value].Item2} {NetworkSpecification[value].Item3}\"", // Pass the Command as argument
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

            string command_route_add = ("route add " + NetworkSpecification[value].Item1 + " mask " + NetworkSpecification[value].Item2 + " " + NetworkSpecification[value].Item3 + " if " + NetworkSpecification[value].Item4);

            // Set up the process start info
            ProcessStartInfo startInfo_route_add = new ProcessStartInfo
            {
                FileName = "powershell.exe",  // Specify PowerShell executable
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command_route_add}\"", // Pass the Command as argument
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
                Added_Routes[counter] = Tuple.Create(NetworkSpecification[value].Item1, NetworkSpecification[value].Item2, NetworkSpecification[value].Item3, NetworkSpecification[value].Item4);
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