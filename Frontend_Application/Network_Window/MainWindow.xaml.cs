using Network_Window.Info_button;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Network_Window
{
    public partial class MainWindow : Window
    {
        public int Counter = 1;
        public int CurrentNetworkRowNumber = 0;
        private int currentlyCheckedCount = 0;
        private int id = 0;
        private int SelectedInternetRow;
        int CounterStopper = 0;
        Boolean Maschinennetz = false;
        Boolean Switcher = false;
        public string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        public string[] interfaces;
        public string CurrentlySelectedNetwork;
        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }

        Dictionary<int, Tuple<string, string, string, string>> NetworkSpecification = new Dictionary<int, Tuple<string, string, string, string>>();
        Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        Dictionary<ComboBox, object> previousSelections = new Dictionary<ComboBox, object>();
        private Dictionary<int, string> comboBoxSelections = new Dictionary<int, string>();

        private TerminalCommand terminalCommand = new TerminalCommand();
        private RegularExpressions regularExpressions = new RegularExpressions();
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
        }
        //private void Hinzufügen_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    GetInputFields();

        //    if (!Regex.IsMatch(ImpIp, pattern))
        //    {
        //        output.Text = "IP Adresse ist ungültig";
        //    }
        //    else if (!Regex.IsMatch(ImpMask, pattern))
        //    {
        //        output.Text = "Subnetzmaske ist ungültig";
        //    }
        //    else if (!Regex.IsMatch(ImpGateway, pattern))
        //    {
        //        output.Text = "Gateway ist ungültig";
        //    }
        //    else if (Counter == 5)
        //    {
        //        output.Text = "Maximale Anzahl an Routen erreicht, bitte löschen sie Routen.";
        //    }
        //    else
        //    {
        //        ClearFields();

        //        if (!string.IsNullOrWhiteSpace(ImpIndex))
        //        {
        //            terminalCommand.CommandShell("route add " + ImpIp + " mask " + ImpMask + " " + ImpGateway + " if " + ImpIndex);
        //        }
        //        else
        //        {
        //            terminalCommand.CommandShell("route add " + ImpIp + " mask " + ImpMask + " " + ImpGateway);
        //        }

        //        if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
        //        {
        //            output.Text = $"Error: {terminalCommand.Error}";
        //        }
        //        else
        //        {
        //            Added_Routes[Counter] = Tuple.Create(ImpIp, ImpMask, ImpGateway, ImpIndex);
        //            output.Text = terminalCommand.Output;

        //            string text = $"IP Adresse: {Added_Routes[Counter].Item1}\nSubnetMaske: {Added_Routes[Counter].Item2}\nGateway: {Added_Routes[Counter].Item3}\nSchnittstellenindex: {Added_Routes[Counter].Item4}\n";

        //            switch (Counter)
        //            {
        //                case 1:
        //                    Route_1.Text = text;
        //                    break;
        //                case 2:
        //                    Route_2.Text = text;
        //                    break;
        //                case 3:
        //                    Route_3.Text = text;
        //                    break;
        //                case 4:
        //                    Route_4.Text = text;
        //                    break;
        //            }
        //            Counter++;
        //        }
        //    }
        //}
        private void NetworkRefreshButton(object sender, RoutedEventArgs e)
        {
            // Speichere die aktuellen Auswahlen der ComboBoxen
            SaveComboBoxSelections();

            GridContainer.Children.Clear();
            Switcher = false;
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

            // Erstelle die ComboBoxes nur einmal
            if (previousSelections.Count == 0)
            {
                CreateComboBoxes(CommandRawOutputAsRows.Count);
            }

            for (int j = 0; j < CommandRawOutputAsRows.Count; j++)
            {
                int Runner = j;
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
                        // Füge die vorhandene ComboBox hinzu
                        AddExistingComboBox(Runner, RowIndex, ColumnIndex);
                        RowIndex++;
                        ColumnIndex = 0;
                    }
                }
                NetworkSpecification[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }

            CheckGateWayButtonVisibilityRequirement();

            // Stelle die Auswahlen der ComboBoxen wieder her
            RestoreComboBoxSelections();
        }
        private void RestoreComboBoxSelections()
        {
            foreach (var child in GridContainer.Children)
            {
                if (child is ComboBox comboBox)
                {
                    int runner = int.Parse(comboBox.Name.Split('_')[1]);
                    if (comboBoxSelections.TryGetValue(runner, out string selectedValue))
                    {
                        comboBox.SelectedItem = selectedValue;
                    }
                }
            }
        }
        private void SaveComboBoxSelections()
        {
            comboBoxSelections.Clear();
            foreach (var child in GridContainer.Children)
            {
                if (child is ComboBox comboBox)
                {
                    int runner = int.Parse(comboBox.Name.Split('_')[1]);
                    comboBoxSelections[runner] = comboBox.SelectedItem.ToString();
                }
            }
        }
        private void CreateComboBoxes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Name = $"ComboBox_{i}";
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

                previousSelections[comboBox] = comboBox.SelectedItem.ToString();
            }
        }
        private void AddExistingComboBox(int runner, int rowIndex, int columnIndex)
        {
            ComboBox comboBox = previousSelections.Keys.FirstOrDefault(cb => cb.Name == $"ComboBox_{runner}");
            if (comboBox != null)
            {
                if (rowIndex > 1)
                {
                    Grid.SetRow(comboBox, rowIndex);
                    Grid.SetColumn(comboBox, columnIndex);
                    GridContainer.Children.Add(comboBox);
                }
            }
        }
        private void ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            ComboBox selectedComboBox = sender as ComboBox;
            if (selectedComboBox == null) return;

            string selectedItem = selectedComboBox.SelectedItem?.ToString();
            CurrentlySelectedNetwork = selectedItem;

            SelectedInternetRow = Grid.GetRow(selectedComboBox);

            if (selectedComboBox.SelectedIndex == 0)
            {
                // Reaktivieren aller ComboBoxes
                foreach (UIElement element in GridContainer.Children)
                {
                    if (element is ComboBox comboBox)
                    {
                        comboBox.IsEnabled = true;
                    }
                }
            }
            else if (selectedItem == "Internet" || selectedItem == "Maschinenetz")
            {
                bool isAlreadySelected = false;
                Switcher = true;

                foreach (UIElement element in GridContainer.Children)
                {
                    if (element is ComboBox comboBox && comboBox != selectedComboBox)
                    {
                        if (comboBox.SelectedItem?.ToString() == selectedItem)
                        {
                            isAlreadySelected = true;
                            break;
                        }
                        comboBox.IsEnabled = false;
                    }
                }

                if (isAlreadySelected)
                {
                    MessageBox.Show($"{selectedItem} ist schon ausgewählt");
                    selectedComboBox.SelectedItem = previousSelections[selectedComboBox];
                }
                else
                {
                    previousSelections[selectedComboBox] = selectedItem;
                }
            }
            else
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
            textBlock.Padding = new Thickness(5);
            textBlock.Height = 30;
            textBlock.Margin = new Thickness(1);
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;


            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            GridContainer.Children.Add(textBlock);
        }
        private void GenericRouteDelete_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = sender as Button;
            if (deleteButton == null) return ;

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
                //terminalCommand.CommandShell($"route delete {Added_Routes[index].Item1} mask {Added_Routes[index].Item2} {Added_Routes[index].Item3} if {Added_Routes[index].Item4}");
                
                    Added_Routes.Remove(Counter);
                    switch (index)
                    {
                        case 1:
                            Route_1.Text = "";
                        foreach (var child in GridContainer.Children)
                        {
                            if (child is ComboBox comboBox)
                            {
                                comboBox.Items.Remove(Added_Routes[1].Item4);
                            }
                        }
                        break;
                        case 2:
                            Route_2.Text = "";
                        foreach (var child in GridContainer.Children)
                        {
                            if (child is ComboBox comboBox)
                            {
                                comboBox.Items.Remove(Added_Routes[2].Item4);
                            }
                        }
                        break;
                        case 3:
                            Route_3.Text = "";
                        foreach (var child in GridContainer.Children)
                        {
                            if (child is ComboBox comboBox)
                            {
                                comboBox.Items.Remove(Added_Routes[3].Item4);
                            }
                        }
                        break;
                        case 4:
                            Route_4.Text = "";
                        foreach (var child in GridContainer.Children)
                        {
                            if (child is ComboBox comboBox)
                            {
                                comboBox.Items.Remove(Added_Routes[4].Item4);
                            }
                        }
                        break;
                        default:
                            break;
                    }
                    Counter = index;
            }
        }
        private void ShowRoutesButton(object sender, RoutedEventArgs e)
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
        private void DeleteButton(object sender, RoutedEventArgs e)
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

            regularExpressions.ImpIp = IP_Adresse_Block.Text;
            regularExpressions.ImpMask = Mask_Block.Text;
            regularExpressions.ImpGateway = Gate_Block.Text;
            regularExpressions.ImpIndex = Index_Block.Text;
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
            NetworkRefreshButton(null, null);
        }
        private void CheckGateWayButtonVisibilityRequirement()
        {
            bool anyInternetOrMachineNetSelected = GridContainer.Children
                .OfType<ComboBox>()
                .Any(cb => cb.SelectedItem?.ToString() == "Internet" || cb.SelectedItem?.ToString() == "Maschinenetz");

            if (anyInternetOrMachineNetSelected && Switcher)
            {
                GateWayButton.Visibility = Visibility.Visible;
                Löschen_Button.Visibility = Visibility.Hidden;
                Eingabe_Button.Visibility = Visibility.Hidden;
                MessageBox.Show("Bitte geben Sie ein Gateway ein.");
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
            GetInputFields();
            if (!string.IsNullOrEmpty(ImpGateway) && Regex.IsMatch(ImpGateway, pattern))
            {
                if (CurrentlySelectedNetwork == "Internet")
                {
                    terminalCommand.CommandShell($"route add 0.0.0.0 mask 0.0.0.0 {ImpGateway} if {NetworkSpecification[SelectedInternetRow].Item4}");
                    //MessageBox.Show("Internet");

                    if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                    {
                        output.Text = $"Error: {terminalCommand.Error}";
                    }
                    else
                    {
                        output.Text = terminalCommand.Output;
                        Gate_Block.Clear();

                        EnableComboBox();
                    }

                }
                else if (CurrentlySelectedNetwork == "Maschinenetz")
                {
                    terminalCommand.CommandShell($"route add {NetworkSpecification[SelectedInternetRow].Item1} mask {NetworkSpecification[SelectedInternetRow].Item2} {ImpGateway} if {NetworkSpecification[SelectedInternetRow].Item4}");
                    MessageBox.Show("Maschinenetz");

                    if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                    {
                        output.Text = $"Error: {terminalCommand.Error}";
                    }
                    else
                    {
                        output.Text = terminalCommand.Output;
                        Gate_Block.Clear();

                        EnableComboBox();
                    }

                }
                
            }
            else MessageBox.Show("Bitte geben Sie ein Gateway ein.");

        }
        private void EnableComboBox()
        {
            foreach (ComboBox comboBox in GridContainer.Children.OfType<ComboBox>())
            {
                comboBox.IsEnabled = true;
            }
        }
        private void GateWayButton_Click(object sender, EventArgs e)
        {
            CheckGateWayButtonVisibilityRequirement(); // Verstecke den Button nach dem Klicken
        }//Check
        private void AddedCustomRoute(object sender, RoutedEventArgs e)
        {
            GetInputFields();
            if (regularExpressions.PatternValiduation())
            {
                foreach (var child in GridContainer.Children)
                {
                    if (child is ComboBox comboBox)
                    {
                        comboBox.Items.Add(ImpIndex);
                    }
                }
                output.Text = "Route zur Liste hinzugefügt";

                Added_Routes[Counter] = Tuple.Create(ImpIp, ImpMask, ImpGateway, ImpIndex);

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
                ClearFields();
            }
            else if (!regularExpressions.PatternValiduation() )
            {
                output.Text=regularExpressions.Output;
            }
        }
        // To Do gateway fenster taucht áuf auch nachdem ein netzwerk ausgewählt wurde und die tabelle refresht wurde
        //Mit dem delete button soll die custom route gelöscht werden und auch vom combobox

    }
}