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
        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }
        Boolean IsChecked = false;
       
        Dictionary<int, Tuple<string, string, string, string>> NetworkSpecification = new Dictionary<int, Tuple<string, string, string, string>>();
        Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        Dictionary<ComboBox, object> previousSelections = new Dictionary<ComboBox, object>();

        private TerminalCommand terminalCommand = new TerminalCommand();
        public MainWindow()
        {
            InitializeComponent();
            NetworkRefreshButton(null, null);
            GateWayButton.Click += GateWayButton_Click;//Check
            GateWayButton.Click += GateWayButtonConfirm;//Check
        }
        private void HelpButton_Click(object sender, RoutedEventArgs e)
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

                    CreateTextBlocksFields(ColumnValue, RowIndex, ColumnIndex);

                    ColumnIndex++;

                    if (ColumnIndex >= 7)
                    {
                        CreateComboBox(RunnerForCheckBox, RowIndex, ColumnIndex);
                        RowIndex++;
                        ColumnIndex = 0;
                    }
                }
                NetworkSpecification[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }
        }//finished
        private void CreateComboBox(int runner,int rowIndex, int columnIndex) {

            ComboBox comboBox = new ComboBox();
            comboBox.Name = $"ComboBox_{runner}";
            comboBox.FontSize = 12;
            comboBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            comboBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            comboBox.Items.Add("-Auswahl-");
            comboBox.Items.Add("Internet");
            comboBox.Items.Add("Maschinenetz");
            comboBox.SelectedIndex = 0;
            comboBox.Width = 100;
            comboBox.Margin = new Thickness(5);
            comboBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            comboBox.SelectionChanged += ComboBox_SelectionChanged;

            if (rowIndex > 1)
            {
                Grid.SetRow(comboBox, rowIndex);

                Grid.SetColumn(comboBox, columnIndex);

                GridContainer.Children.Add(comboBox);
            }
            previousSelections[comboBox] = comboBox.SelectedItem.ToString();
        }//finished
        private void ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ComboBox selectedComboBox = sender as ComboBox;
            string selectedItem = selectedComboBox.SelectedItem?.ToString();

            if (selectedComboBox != null && (selectedItem == "Internet" || selectedItem == "Maschinenetz"))
            {
                bool isAlreadySelected = false;
                foreach (UIElement element in GridContainer.Children)
                {
                    if (element is ComboBox comboBox && comboBox != selectedComboBox && comboBox.SelectedItem?.ToString() == selectedItem)
                    {
                        isAlreadySelected = true;
                        break;
                    }
                }

                if (isAlreadySelected)
                {
                    MessageBox.Show($"{selectedItem} ist schon ausgewählt");
                    selectedComboBox.SelectedItem = previousSelections[selectedComboBox]; // Zurücksetzen zum letzten gespeicherten Wert
                }
                else
                {
                    previousSelections[selectedComboBox] = selectedItem; // Aktualisierung zu neuem Wert
                }
            }
            else if (selectedComboBox != null)
            {
                previousSelections[selectedComboBox] = selectedItem;
            }

            CheckGateWayButtonVisibilityRequirement();
        }
        private void CreateTextBlocksFields(string value, int row, int column)
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
        private void GenericRouteDelete_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = sender as Button;
            if (deleteButton == null) return;

            int index = 0;
            string boxText = "";

            if (deleteButton.Name == "Route_1_Delete")
            {
                index = 1;
                boxText = Route_1.Text;
            }
            else if (deleteButton.Name == "Route_2_Delete")
            {
                index = 2;
                boxText = Route_2.Text;
            }
            else if (deleteButton.Name == "Route_3_Delete")
            {
                index = 3;
                boxText = Route_3.Text;
            }
            else if (deleteButton.Name == "Route_4_Delete")
            {
                index = 4;
                boxText = Route_4.Text;
            }

            DeleteRoute(index, boxText);
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
        private void LöschenButton_Click(object sender, RoutedEventArgs e)
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
        private void AlleRoutenLöschen_Click(object sender, RoutedEventArgs e)
        {
            terminalCommand.CommandShell("route -f");
            Route_1.Text = "";
            Route_2.Text = "";
            Route_3.Text = "";
            Route_4.Text = "";
            Counter = 1;
        }
        private void CheckGateWayButtonVisibilityRequirement()
        {
            bool anyInternetOrMachineNetSelected = GridContainer.Children
                .OfType<ComboBox>()
                .Any(cb => cb.SelectedItem?.ToString() == "Internet" || cb.SelectedItem?.ToString() == "Maschinenetz");

            if (anyInternetOrMachineNetSelected)
            {
                GateWayButton.Visibility = Visibility.Visible;
                Löschen_Button.Visibility = Visibility.Hidden;
                Eingabe_Button.Visibility = Visibility.Hidden;
            }
            else
            {
                GateWayButton.Visibility = Visibility.Hidden;
                Löschen_Button.Visibility = Visibility.Visible;
                Eingabe_Button.Visibility = Visibility.Visible;
            }
        }//Check
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
            }else MessageBox.Show("Bitte geben Sie ein Gateway ein.");
        }//Check
        private void GateWayButton_Click(object sender, EventArgs e)
        {
            CheckGateWayButtonVisibilityRequirement(); // Verstecke den Button nach dem Klicken
        }//Check
    }
}