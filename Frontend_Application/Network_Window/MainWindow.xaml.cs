using Network_Window.Info_button;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Network_Window
{
    public partial class MainWindow : Window
    {
        public int Counter = 1;
        public int CurrentNetworkRowNumber = 0;
        private int currentlyCheckedCount = 0;
        private int id = 0;
        Boolean InternetIsChecked = false;
        Boolean Maschinennetz = false;
        public string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        public string[] interfaces;
        Boolean IsChecked = false;
       
        Dictionary<int, Tuple<string, string, string, string>> NetworkSpecification = new Dictionary<int, Tuple<string, string, string, string>>();
        Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        //private DispatcherTimer timer;
        //private int counterr = 0;
        //private int limit = 10;

        private TerminalCommand terminalCommand = new TerminalCommand();
        public MainWindow()
        {
            InitializeComponent();
            NetworkRefreshButton(null, null);
            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(1); // Set the interval to 1 second
            //timer.Tick += Timer_Tick;
            //timer.Start();
            GateWayButton.Click += GateWayButtonConfirm;
        }
        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    Counter++; // Increment the Counter by 1 every second
        //    if (Counter >= limit)
        //    {
        //        // Clear the TextBlock and stop the timer
        //        output.Text = ""; // Assuming 'output' is the name of your TextBlock
        //        timer.Stop(); // Optional: Stop the timer if you don't need it to run again
        //    }
        //}
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
            GetInputFields();

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
            else if (Counter == 5)
            {
                output.Text = "Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.";
            }
            else 
            {
                ClearFields();

                //terminalCommand.CommandShell("route delete 0.0.0.0 mask 0.0.0.0 " + ImpGateway);

                //if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                //{
                //    output.Text = $"Error: {terminalCommand.Error}";
                //}
                //else output.Text = terminalCommand.Output;

                if (!string.IsNullOrWhiteSpace(ImpIndex))
                {
                    terminalCommand.CommandShell("route add " + ImpIp + " mask " + ImpMask + " " + ImpGateway + " if " + ImpIndex);
                }
                else {
                    terminalCommand.CommandShell("route add " + ImpIp + " mask " + ImpMask + " " + ImpGateway);
                }

                if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                {
                    output.Text = $"Error: {terminalCommand.Error}";
                }
                else
                {
                    Added_Routes[Counter] = Tuple.Create(ImpIp, ImpMask, ImpGateway, ImpIndex);
                    output.Text = terminalCommand.Output;

                    string text = $"IP Adresse: {Added_Routes[Counter].Item1}\nSubnetMaske: {Added_Routes[Counter].Item2}\nGateway: {Added_Routes[Counter].Item3}\nSchnittstellenindex: {Added_Routes[Counter].Item4}\n";

                    switch (Counter)
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
                    Counter++;
                    //timer.Start();
                }
            }
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

            int Count = CommandRawOutputAsRows.Count;

            for (int j = 0; j < CommandRawOutputAsRows.Count; j++)
            {
                int Runner = j;
                int RunnerForCheckBox = j;
                string[] OneNetworkRowData = CommandRawOutputAsRows[j];

                for (int i = 0; i < OneNetworkRowData.Length; i++)
                {
                    ColumnValue = OneNetworkRowData[i];

                    Index = OneNetworkRowData[0];

                    Ip = OneNetworkRowData[3];

                    SubnetMask = OneNetworkRowData[4];

                    Gateway = OneNetworkRowData[5];

                    CreateTextBlocks(ColumnValue, RowIndex, ColumnIndex);

                    ColumnIndex++;

                    if (ColumnIndex >= 7)
                    {
                        CreateCheckBox(RunnerForCheckBox, RowIndex, ColumnIndex);
                        RowIndex++;
                        ColumnIndex = 0;
                    }
                }
                NetworkSpecification[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }
        }//finished
        private void CreateCheckBox(int runner,int rowIndex, int columnIndex) {
            
            CheckBox checkBox = new CheckBox();

            checkBox.Name = $"CheckBox_{runner}";

            checkBox.Content = "";

            checkBox.FontSize = 12;

            checkBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");

            checkBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);

            checkBox.Checked += (sender, e) => CheckBox_Checked(runner);
            checkBox.Unchecked += (sender, e) => CheckBox_Unchecked(runner);

            if (rowIndex >1)
            {
                Grid.SetRow(checkBox, rowIndex);

                Grid.SetColumn(checkBox, columnIndex);

                GridContainer.Children.Add(checkBox);
            }
        }//finished
        private void CheckBox_Checked(int value)
        {
            MessageBox.Show($"IP Adresse: {NetworkSpecification[value].Item1}\n" +
                $"Subnetzmaske: {NetworkSpecification[value].Item2}\n" +
                $"Gateway: {NetworkSpecification[value].Item3}\n" +
                $"Schnittstelle: {NetworkSpecification[value].Item4}");

            CurrentNetworkRowNumber = value;
            if (!IsChecked) 
            {
                IsChecked = !IsChecked;
            }
            CheckGateWayButtonVisibilityRequirement();
            currentlyCheckedCount++;
            ManageCheckBoxesAvailability();


        }
        private void ManageCheckBoxesAvailability()
        {
            bool shouldEnable = currentlyCheckedCount < 2;

            foreach (UIElement element in GridContainer.Children)
            {
                if (element is CheckBox checkBox)
                {
                    if (checkBox.IsChecked == false)
                    {
                        checkBox.IsEnabled = shouldEnable;
                    }
                }
            }
        }
        private void CheckBox_Unchecked(int value)
        {
            if (IsChecked)
            { 
                IsChecked = !IsChecked; 
            }
            CheckGateWayButtonVisibilityRequirement();
            currentlyCheckedCount--;
            ManageCheckBoxesAvailability();
        }
        private void CreateTextBlocks(string value, int row, int column)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = $"{value}";
            textBlock.FontSize = 12;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Width = 100;
            textBlock.Height = 30;
            textBlock.Margin = new Thickness(1);
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;


            Grid.SetRow(textBlock,row);
            Grid.SetColumn(textBlock, column);
            GridContainer.Children.Add(textBlock);
        }
        private void Route_1_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRoute(1, Route_1.Text);
        }
        private void Route_2_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRoute(2, Route_2.Text);
        }
        private void Route_3_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRoute(3, Route_3.Text);
        }
        private void Route_4_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteRoute(4, Route_4.Text);
        }
        private void DeleteRoute(int index, string boxnr)
        {

            if (string.IsNullOrWhiteSpace(boxnr))
            {
                output.Text = "keine route";
            }
            else
            {
                terminalCommand.CommandShell($"route delete {Added_Routes[index].Item1} mask {Added_Routes[index].Item2} {Added_Routes[index].Item3} if {Added_Routes[index].Item4}");
                
                

                if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                {
                    output.Text = terminalCommand.Error;
                }
                else
                {
                    output.Text = terminalCommand.Output;
                    Added_Routes.Remove(Counter);
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
                    Counter = index;
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
            GetInputFields();
            string command = BuildCommand();
            ExecuteCommand(command);
        }
        private void GetInputFields()
        {
            ImpIp = IP_Adresse_Block.Text;
            ImpMask = Mask_Block.Text;
            ImpGateway = Gate_Block.Text;
            ImpIndex = Index_Block.Text;
        }
        private void ClearFields()
        {
            IP_Adresse_Block.Clear();
            Mask_Block.Clear();
            Gate_Block.Clear();
            Index_Block.Clear();
        }
        private string BuildCommand()
        {
            if (string.IsNullOrEmpty(ImpIndex))
            {
                return $"route delete {ImpIp} mask {ImpMask} {ImpGateway}";
            }
            else if (string.IsNullOrEmpty(ImpMask))
            {
                return $"route delete {ImpIp}";
            }
            else
            {
                return $"route delete {ImpIp} mask {ImpMask} {ImpGateway} if {ImpIndex}";
            }
        }
        private void ExecuteCommand(string command)
        {
            terminalCommand.CommandShell(command);

            if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
            {
                output.Text = terminalCommand.Error;
            }
            else
            {
                output.Text = terminalCommand.Output;
                ClearFields();
            }
        }
        private void Alle_Routen_Löschen(object sender, RoutedEventArgs e)
        {
            ShellCommand("route -f");
            Route_1.Text = "";
            Route_2.Text = "";
            Route_3.Text = "";
            Route_4.Text = "";
            Counter = 1;
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
                InternetIsChecked = false;
            }
            else
            {
                Internet_box.IsEnabled = true;
            }
        }//ToDo implement logic
        private void CheckGateWayButtonVisibilityRequirement()
        {
            if (InternetIsChecked && IsChecked)
            {
                GateWayButton.Visibility= Visibility.Visible;
                Löschen_Button.Visibility = Visibility.Hidden;
                Eingabe_Button.Visibility = Visibility.Hidden;
            }
            else
            {
                GateWayButton.Visibility = Visibility.Hidden;
                Löschen_Button.Visibility = Visibility.Visible;
                Eingabe_Button.Visibility = Visibility.Visible;
            }
        }
        private void InternetCheckboxChecked(object sender, RoutedEventArgs e)
        {
            InternetIsChecked = !InternetIsChecked;
            if (InternetIsChecked)
            {
                Maschinen_netz_box.IsEnabled = false;
                Maschinennetz = false;

                if (IsChecked)
                {
                    MessageBox.Show("Please enter a Gateway in the Gate field");
                    CheckGateWayButtonVisibilityRequirement();

                }
            }
            else
            {
                Maschinen_netz_box.IsEnabled = true;
            }
        }//ToDo implement logic
        private void InternetCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (InternetIsChecked)
            {
                InternetIsChecked = !InternetIsChecked;
                Maschinen_netz_box.IsEnabled = true;
                if (IsChecked)
                {
                    CheckGateWayButtonVisibilityRequirement();
                }
            }
        }//ToDo implement logic
        private void GateWayButtonConfirm(object sender, RoutedEventArgs e)
        {
            ImpGateway = Gate_Block.Text;
            if (IsChecked && InternetIsChecked && !string.IsNullOrEmpty(ImpGateway))
            {
                terminalCommand.CommandShell($"route add 0.0.0.0 mask 0.0.0.0 {ImpGateway} if {NetworkSpecification[CurrentNetworkRowNumber].Item4}");
                if(!string.IsNullOrWhiteSpace(terminalCommand.Error))
                {
                    output.Text = $"Error: {terminalCommand.Error}";
                }
                else
                {
                    output.Text = terminalCommand.Output;
                    Gate_Block.Clear();
                }
            }
        }
    }
}