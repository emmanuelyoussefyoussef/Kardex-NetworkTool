using Network_Window.Info_button;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;

namespace Network_Window
{
    public partial class MainWindow : Window // wenn ein combobox geändert wurde und derselbe wieder aus etwas außer -Auswahl-
                                             // gesetzt wird dann beinhaltet meine AktiveRoutes Liste immernoch den Wert von vorher
                                             //GateWayButton mit Enter klciken lassen (optional)
    {
        private int Counter = 1;
        private int CurrentNetworkRowNumber = 0;
        private int SelectedInternetRow;

        private bool Switcher = false;

        private string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        private string CurrentlySelectedNetwork;
        private string[] interfaces;

        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }

        private Dictionary<int, Tuple<string, string, string, string>> NetworkSpecification = new Dictionary<int, Tuple<string, string, string, string>>();
        private Dictionary<int, Tuple<string, string, string, string>> Added_Routes = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        private Dictionary<ComboBox, object> previousSelections = new Dictionary<ComboBox, object>();
        private Dictionary<int, string> comboBoxSelections = new Dictionary<int, string>();
        
        private List<string> ActiveNetworks = new List<string>();
        private List<string> addedRouteNames = new List<string> { "Internet", "Maschinenetz" };



        private TerminalCommand terminalCommand = new TerminalCommand();
        private RegularExpressions regularExpressions = new RegularExpressions();



        public MainWindow()
        {
            InitializeComponent();
            NetworkRefreshButton(null, null);
            GateWayButton.Click += GateWayButton_Click;//Check
            GateWayButton.Click += GateWayButtonConfirm;//Check
        }

        private void CreateTable(string value, int row, int column)
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

                comboBox.SelectionChanged += ComboBoxSelectionChanged;

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
        private void ComboBoxSelectionChanged(object sender, EventArgs e)
        {
            ComboBox selectedComboBox = sender as ComboBox;
            if (selectedComboBox == null) return;

            string selectedItem = selectedComboBox.SelectedItem?.ToString();
            if (selectedItem == "-Auswahl-")
            {
                if (previousSelections[selectedComboBox] != "-Auswahl-")
                {
                    DeleteCurrentSelectedNetworks(CurrentlySelectedNetwork);
                }
            }
            CurrentlySelectedNetwork = selectedItem;
            if (selectedItem != "-Auswahl-")
            {
                ModifyCurrentSelectedNetworks(selectedItem);
            }

            SelectedInternetRow = Grid.GetRow(selectedComboBox);

            if (selectedComboBox.SelectedIndex == 0)
            {
                foreach (UIElement element in GridContainer.Children)
                {
                    if (element is ComboBox comboBox)
                    {
                        comboBox.IsEnabled = true;
                    }
                }
                CheckGateWayButtonVisibilityRequirement();
            }
            else if (addedRouteNames.Contains(selectedItem))
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
                    MessageBox.Show("Bitte geben Sie ein Gateway ein.");
                    //ActiveNetworks.Add(CurrentlySelectedNetwork);
                    CheckGateWayButtonVisibilityRequirement();
                }
            }
            else
            {
                previousSelections[selectedComboBox] = selectedItem;
                CheckGateWayButtonVisibilityRequirement();
            }
        }
        private void EnableComboBox()
        {
            foreach (ComboBox comboBox in GridContainer.Children.OfType<ComboBox>())
            {
                comboBox.IsEnabled = true;
            }
        }
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
                addedRouteNames.Add(ImpIndex);

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
                ClearInputFields();
            }
            else if (!regularExpressions.PatternValiduation())
            {
                output.Text = regularExpressions.Output;
            }
        }


        private void HelpButton(object sender, RoutedEventArgs e)
        {
            Info_Fenster objInfo_Fenster = new Info_Fenster();
            this.Visibility = Visibility.Visible;
            objInfo_Fenster.Show();
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
        private void DeleteAllRoutesButton(object sender, RoutedEventArgs e)
        {
            terminalCommand.CommandShell("route -f");
            Route_1.Text = "";
            Route_2.Text = "";
            Route_3.Text = "";
            Route_4.Text = "";
            Counter = 1;
            NetworkRefreshButton(null, null);
        }
        private void GateWayButton_Click(object sender, EventArgs e)
        {
            CheckGateWayButtonVisibilityRequirement();
            // Verstecke den Button nach dem Klicken
        }
        private void GateWayButtonConfirm(object sender, RoutedEventArgs e)
        {
            GetInputFields();
            if (!string.IsNullOrEmpty(ImpGateway) && Regex.IsMatch(ImpGateway, pattern))
            {
                if (CurrentlySelectedNetwork == "Internet")
                {
                    terminalCommand.CommandShell($"route add 0.0.0.0 mask 0.0.0.0 {ImpGateway} if {NetworkSpecification[SelectedInternetRow].Item4}");

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
                //else if (CurrentlySelectedNetwork == "Maschinenetz")
                else if (addedRouteNames.Contains(CurrentlySelectedNetwork))
                {
                    string modifiedIP = ReplaceAfterThirdDotWithZero(NetworkSpecification[SelectedInternetRow].Item1);

                    //terminalCommand.CommandShell($"route add {modifiedIP} mask 0.0.0.0 {ImpGateway} if {NetworkSpecification[SelectedInternetRow].Item4}");
                    MessageBox.Show($"route add {modifiedIP} mask {NetworkSpecification[SelectedInternetRow].Item2} {ImpGateway} if {NetworkSpecification[SelectedInternetRow].Item4}");

                    if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
                    {
                        //output.Text = $"Error: {terminalCommand.Error}";
                    }
                    else
                    {
                        //output.Text = terminalCommand.Output;
                        Gate_Block.Clear();
                        Switcher = false;
                        CheckGateWayButtonVisibilityRequirement();
                        EnableComboBox();
                    }

                }

            }
            else MessageBox.Show("Bitte geben Sie den Gateway erneut ein.");

        }
        private void NetworkRefreshButton(object sender, RoutedEventArgs e)
        {
            SaveComboBoxSelections();

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

                    CreateTable(ColumnValue, RowIndex, ColumnIndex);

                    ColumnIndex++;

                    if (ColumnIndex >= 7)
                    {
                        AddExistingComboBox(Runner, RowIndex, ColumnIndex);
                        RowIndex++;
                        ColumnIndex = 0;
                    }
                }
                NetworkSpecification[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }

            CheckGateWayButtonVisibilityRequirement();

            RestoreComboBoxSelections();
        }
        private void ModifyCurrentSelectedNetworks(string name)
        {
            if (!ActiveNetworks.Contains(name))
            {
                ActiveNetworks.Add(name);

            }
        }
        private void DeleteCurrentSelectedNetworks(string name)
        {
            ActiveNetworks.Remove(name);
        }

        private string ReplaceAfterThirdDotWithZero(string ipAddress)
        {
            string[] parts = ipAddress.Split('.');
            if (parts.Length == 4)
            {
                parts[3] = "0";
            }
            return string.Join(".", parts);
        }
        private void GenericRouteDelete(object sender, RoutedEventArgs e)
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

            DeleteManualAddedRoute(index, boxText);
        }
        private void DeleteManualAddedRoute(int index, string boxnr)
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
        private void ClearInputFields()
        {
            IP_Adresse_Block.Clear();
            Mask_Block.Clear();
            Gate_Block.Clear();
            Index_Block.Clear();
        }
        private void CheckGateWayButtonVisibilityRequirement()
        {
            bool anyInternetOrMachineNetSelected = GridContainer.Children
               .OfType<ComboBox>()
               .Any(cb => cb.SelectedItem != null && addedRouteNames.Contains(cb.SelectedItem.ToString()));


            if (anyInternetOrMachineNetSelected && Switcher)
            {
                GateWayButton.Visibility = Visibility.Visible;
                Löschen_Button.Visibility = Visibility.Hidden;
                Eingabe_Button.Visibility = Visibility.Hidden;
                Gate_Block.Focus();
                GateWayButton.
            }
            else
            {
                GateWayButton.Visibility = Visibility.Hidden;
                Löschen_Button.Visibility = Visibility.Visible;
                Eingabe_Button.Visibility = Visibility.Visible;
            }
        }//Check
    }
}
