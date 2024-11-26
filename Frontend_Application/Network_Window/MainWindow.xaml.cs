using Network_Window.Info_button;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using System.Collections;
using System.Buffers;
using System.Security.Policy;

namespace Network_Window // To activate the programm remove comment from 331 & 351 and remove the MessageBox.Show
{
    public partial class MainWindow : Window
    //wenn beim eingeben von gateway und hinzufügen von route es nicht klappt dann erneut fragen nach gateway
    //route soll gelöscht werden nachdem combobox sich ändert(FIX)
    //nachdem eine manual geaddete route gelöscht wird dann soll es vom combobox entfernt werden
    //wenn ein manualgeaddedes netzwerk ausgewählt wird dann werden alle comboboxes deaktiviert und nicht mehr aktiviert(FIX)
    //route delete messagebox soll erst erscheinen wenn ein gateway hinzugefügt wurde
    //wenn ich zu einem manual geaddetes netzwerk wechsel und dann zu einem anderen nicht manual geadded und dann wieder zu manual geadded dann erscheint kein messagebox
    {
        private int Counter = 1;
        private int CurrentNetworkRowNumber = 0;
        private int SelectedInternetRow;
        private int index = 0;
        private ComboBox selectedComboBox;


        private bool SwitchToDisableOverwritingCombobox = true;

        private bool ShowGateWay = false;

        private string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        private string CurrentlySelectedNetwork;
        private string[] interfaces;

        public string ImpIp { get; set; }
        public string ImpMask { get; set; }
        public string ImpGateway { get; set; }
        public string ImpIndex { get; set; }

        private Dictionary<int, string> ActiveRoutes = new Dictionary<int, string>();


        private Dictionary<int, Tuple<string, string, string, string>> ImportedNetworksFromPowershell = new Dictionary<int, Tuple<string, string, string, string>>();
        private Dictionary<int, Tuple<string, string, string, string>> ManualAddedNetworks = new Dictionary<int, Tuple<string, string, string, string>>
            {
            { 1, Tuple.Create("", "", "", "") },
            { 2, Tuple.Create("", "", "", "") },
            { 3, Tuple.Create("", "", "", "") },
            { 4, Tuple.Create("", "", "", "") }
            };
        private Dictionary<ComboBox, string> PreviousComboboxSelections = new Dictionary<ComboBox, string>();
        private Dictionary<int, string> CurrentComboBoxSelections = new Dictionary<int, string>();

        private List<string> ActiveNetworks = new List<string>();
        private List<string> AddedRouteNames = new List<string> { "Internet", "Maschinennetz" };



        private TerminalCommand terminalCommand = new TerminalCommand();
        private RegularExpressions regularExpressions = new RegularExpressions();



        public MainWindow()
        {
            InitializeComponent();
            NetworkRefreshButton(null, null);
            GateWayButton.Click += GateWayButton_Click;//Check
            MessageBox.Show("Bitte beachten Sie, dass das Programm als Administrator ausgeführt werden muss.");
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
            NetworkGridContainer.Children.Add(textBlock);
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
                comboBox.Items.Add("Maschinennetz");
                comboBox.SelectedIndex = 0;
                comboBox.Width = 100;
                comboBox.Margin = new Thickness(5);
                comboBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                comboBox.SelectionChanged += ComboBoxSelectionChanged;

                PreviousComboboxSelections[comboBox] = comboBox.SelectedItem.ToString();
                //CurrentComboBoxSelections[i] = comboBox.SelectedItem.ToString();
            }
        }
        private void AddExistingComboBox(int runner, int rowIndex, int columnIndex)
        {
            ComboBox comboBox = PreviousComboboxSelections.Keys.FirstOrDefault(cb => cb.Name == $"ComboBox_{runner}");
            if (comboBox != null)
            {
                if (rowIndex > 1)
                {
                    Grid.SetRow(comboBox, rowIndex);
                    Grid.SetColumn(comboBox, columnIndex);
                    NetworkGridContainer.Children.Add(comboBox);
                }
            }
        }
        private void SaveComboBoxSelections()
        {
            if (CurrentComboBoxSelections.Count > 0)
            {
                PreviousComboboxSelections = CurrentComboBoxSelections.ToDictionary(kvp => NetworkGridContainer.Children.OfType<ComboBox>().First(cb => cb.Name == $"ComboBox_{kvp.Key}"), kvp => kvp.Value);
            }
            CurrentComboBoxSelections.Clear();
            foreach (var child in NetworkGridContainer.Children)
            {
                if (child is ComboBox comboBox)
                {
                    int runner = int.Parse(comboBox.Name.Split('_')[1]);
                    CurrentComboBoxSelections[runner] = comboBox.SelectedItem.ToString();
                }
            }

        }
        private void RestoreComboBoxSelections()
        {
            foreach (var child in NetworkGridContainer.Children)
            {
                if (child is ComboBox comboBox)
                {
                    int runner = int.Parse(comboBox.Name.Split('_')[1]);
                    if (CurrentComboBoxSelections.TryGetValue(runner, out string selectedValue))
                    {
                        comboBox.SelectedItem = selectedValue;
                    }
                }
            }
        }
        private void ComboBoxSelectionChanged(object sender, EventArgs e)
        {
            if (SwitchToDisableOverwritingCombobox)
            {
                selectedComboBox = sender as ComboBox;

                if (selectedComboBox == null) return;
                SaveComboBoxSelections();

                string SelectedComboboxIndex = selectedComboBox.SelectedItem?.ToString();

                string PreviousSelectedComboboxName = PreviousComboboxSelections[selectedComboBox];


                SelectedInternetRow = Grid.GetRow(selectedComboBox);


                if (SelectedComboboxIndex == "-Auswahl-")
                {
                    foreach (UIElement element in NetworkGridContainer.Children)
                    {
                        if (element is ComboBox comboBox)
                        {
                            comboBox.IsEnabled = true;
                        }
                    }
                    ShowGateWay = false;
                    CheckGateWayButtonVisibilityRequirement();
                    EnableComboBox();
                    if (PreviousComboboxSelections[selectedComboBox] != "-Auswahl-" && ActiveNetworks.Contains(CurrentlySelectedNetwork))
                    {
                        DeleteCurrentSelectedNetworks(CurrentlySelectedNetwork);
                        NetworkRefreshButton(null, null);
                    }

                    else if (PreviousComboboxSelections[selectedComboBox] == "-Auswahl-")
                    {
                        ShowGateWay = false;
                        CheckGateWayButtonVisibilityRequirement();
                        return;
                    }
                }
                else if (SelectedComboboxIndex != "-Auswahl-")
                {
                    ModifyCurrentSelectedNetworks(SelectedComboboxIndex);

                    if (PreviousSelectedComboboxName != "-Auswahl-" && ActiveNetworks.Contains(PreviousSelectedComboboxName))
                    {
                        DeleteCurrentSelectedNetworks(PreviousSelectedComboboxName);
                    }

                }
                if (AddedRouteNames.Contains(SelectedComboboxIndex))
                {
                    bool isAlreadySelected = false;

                    foreach (UIElement element in NetworkGridContainer.Children)
                    {
                        if (element is ComboBox comboBox && comboBox != selectedComboBox)
                        {
                            if (comboBox.SelectedItem?.ToString() == SelectedComboboxIndex)
                            {
                                isAlreadySelected = true;
                                break;
                            }
                            comboBox.IsEnabled = false;

                        }
                    }

                    if (isAlreadySelected)
                    {
                        MessageBox.Show($"{SelectedComboboxIndex} ist schon ausgewählt");
                        SwitchToDisableOverwritingCombobox = false;
                        selectedComboBox.SelectedItem = PreviousComboboxSelections[selectedComboBox];
                        CurrentComboBoxSelections[SelectedInternetRow] = PreviousComboboxSelections[selectedComboBox];
                        SwitchToDisableOverwritingCombobox = true;
                    }
                    else
                    {
                        if (SelectedComboboxIndex == "Internet" || SelectedComboboxIndex == "Maschinennetz")
                        {
                            MessageBox.Show("Bitte geben Sie ein Gateway ein.");
                            ShowGateWay = true;
                            CheckGateWayButtonVisibilityRequirement();
                        }
                    }
                }
                else
                {
                    //PreviousComboboxSelections[selectedComboBox] = SelectedComboboxIndex;
                    if (SelectedComboboxIndex == "Internet" || SelectedComboboxIndex == "Maschinennetz")
                    {
                        ShowGateWay = true;
                    }
                    else ShowGateWay = false;
                    CheckGateWayButtonVisibilityRequirement();
                }
                CurrentlySelectedNetwork = SelectedComboboxIndex;
            }
            else return;

        }



        private void EnableComboBox()
        {
            foreach (ComboBox comboBox in NetworkGridContainer.Children.OfType<ComboBox>())
            {
                comboBox.IsEnabled = true;
            }
        }
        private void AddedCustomRoute(object sender, RoutedEventArgs e)
        {
            GetInputFields();
            if (regularExpressions.PatternValiduation())
            {
                foreach (var child in NetworkGridContainer.Children)
                {
                    if (child is ComboBox comboBox)
                    {
                        comboBox.Items.Add(ImpIndex);
                    }
                }
                output.Text = "Route zur Liste hinzugefügt";

                ManualAddedNetworks[Counter] = Tuple.Create(ImpIp, ImpMask, ImpGateway, ImpIndex);
                AddedRouteNames.Add(ImpIndex);

                string text = $"IP Adresse: {ManualAddedNetworks[Counter].Item1}\nSubnetMaske: {ManualAddedNetworks[Counter].Item2}\nGateway: {ManualAddedNetworks[Counter].Item3}\nSchnittstellenindex: {ManualAddedNetworks[Counter].Item4}\n";

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
            SwitchToDisableOverwritingCombobox = false;
            
            foreach (UIElement element in NetworkGridContainer.Children)
            {
                if (element is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = 0;
                    comboBox.IsEnabled = true;
                }
            }

            CurrentComboBoxSelections.Clear();
            foreach (var child in NetworkGridContainer.Children)
            {
                if (child is ComboBox comboBox)
                {
                    int runner = int.Parse(comboBox.Name.Split('_')[1]);
                    comboBox.SelectedIndex = 0;
                    CurrentComboBoxSelections[runner] = comboBox.SelectedItem.ToString();
                }
            }
            PreviousComboboxSelections = CurrentComboBoxSelections.ToDictionary(kvp => NetworkGridContainer.Children.OfType<ComboBox>().First(cb => cb.Name == $"ComboBox_{kvp.Key}"), kvp => kvp.Value);
            ActiveNetworks.Clear();
            ActiveRoutes.Clear();
            ShowGateWay = false;
            CheckGateWayButtonVisibilityRequirement();
            NetworkRefreshButton(null, null);
            SwitchToDisableOverwritingCombobox = true;
        }
        private void GateWayButton_Click(object sender, EventArgs e)
        {
            CheckGateWayButtonVisibilityRequirement();
        }
        private void GateWayButtonConfirm(object sender, RoutedEventArgs e)
        {
            GetInputFields();
            if (!string.IsNullOrEmpty(ImpGateway) && Regex.IsMatch(ImpGateway, pattern))
            {
                if (CurrentlySelectedNetwork == "Internet")
                {
                    terminalCommand.CommandShell($"route add 0.0.0.0 mask 0.0.0.0 {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");

                    MessageBox.Show($"route add 0.0.0.0 mask 0.0.0.0 {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");

                    if (GetOutput())
                    {
                        Gate_Block.Clear();
                        ShowGateWay = false;
                        CheckGateWayButtonVisibilityRequirement();
                        EnableComboBox();
                        ActiveRoutes.Add(SelectedInternetRow, $"0.0.0.0 mask 0.0.0.0 {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");
                        ActiveNetworks.Add(CurrentlySelectedNetwork);
                    }
                    else selectedComboBox.SelectedItem = PreviousComboboxSelections[selectedComboBox];

                }
                else if (CurrentlySelectedNetwork == "Maschinennetz")
                {
                    string modifiedIP = ReplaceAfterThirdDotWithZero(ImportedNetworksFromPowershell[SelectedInternetRow].Item1);

                    terminalCommand.CommandShell($"route add {modifiedIP} mask {ImportedNetworksFromPowershell[SelectedInternetRow].Item2} {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");
                    MessageBox.Show($"route add {modifiedIP} mask {ImportedNetworksFromPowershell[SelectedInternetRow].Item2} {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");

                    if (GetOutput())
                    {
                        Gate_Block.Clear();
                        ShowGateWay = false;
                        CheckGateWayButtonVisibilityRequirement();
                        EnableComboBox();
                        ActiveRoutes.Add(SelectedInternetRow, $"{modifiedIP} mask {ImportedNetworksFromPowershell[SelectedInternetRow].Item2} {ImpGateway} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");
                        ActiveNetworks.Add(CurrentlySelectedNetwork);
                    }
                    else selectedComboBox.SelectedItem = PreviousComboboxSelections[selectedComboBox];
                }
            }
            else {
                MessageBox.Show("Bitte geben Sie den Gateway erneut ein.");
            }
            SwitchToDisableOverwritingCombobox = false;
            NetworkRefreshButton(null, null);

        }
        private void NetworkRefreshButton(object sender, RoutedEventArgs e)
        {

            NetworkGridContainer.Children.Clear();
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

            if (PreviousComboboxSelections.Count == 0)
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
                ImportedNetworksFromPowershell[Runner] = Tuple.Create($"{Ip}", $"{SubnetMask}", $"{Gateway}", $"{Index}");
            }

            CheckGateWayButtonVisibilityRequirement();

            RestoreComboBoxSelections();
            SwitchToDisableOverwritingCombobox = true;
        }

        private bool GetOutput()
        {
            if (!string.IsNullOrWhiteSpace(terminalCommand.Error))
            {
                output.Text = $"Error: {terminalCommand.Error}";
                return false;
            }
            else
            {
                output.Text = terminalCommand.Output;
                return true;
            }
        }
        private void ModifyCurrentSelectedNetworks(string name)
        {
            if (!ActiveNetworks.Contains(name))
            {
                ShowGateWay = true;
                GetInputFields();
                if (AddedRouteNames.Contains(CurrentlySelectedNetwork) && CurrentlySelectedNetwork != "Internet" && CurrentlySelectedNetwork != "Maschinennetz")
                {
                    string modifiedIP = ReplaceAfterThirdDotWithZero(ManualAddedNetworks[index + 1].Item1);

                    terminalCommand.CommandShell($"route add {modifiedIP} mask {ManualAddedNetworks[index + 1].Item2} {ManualAddedNetworks[index + 1].Item3} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");
                    MessageBox.Show($"route add {modifiedIP} mask {ManualAddedNetworks[index + 1].Item2} {ManualAddedNetworks[index + 1].Item3} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");

                    if (GetOutput())
                    {
                        ActiveNetworks.Add(name);

                        Gate_Block.Clear();
                        EnableComboBox();
                        ShowGateWay = false;
                        ActiveRoutes.Add(SelectedInternetRow, $"{modifiedIP} mask {ManualAddedNetworks[index + 1].Item2} {ManualAddedNetworks[index + 1].Item3} if {ImportedNetworksFromPowershell[SelectedInternetRow].Item4}");
                        NetworkRefreshButton(null,null);
                    }
                    else
                    {
                        selectedComboBox.SelectedItem = PreviousComboboxSelections[selectedComboBox];
                    }
                }
            }
        }
        private void DeleteCurrentSelectedNetworks(string name)
        {
            ActiveNetworks.Remove(name);

            MessageBox.Show($"route delete {ActiveRoutes[SelectedInternetRow]}");

            terminalCommand.CommandShell($"route delete {ActiveRoutes[SelectedInternetRow]}");

            ActiveRoutes.Remove(SelectedInternetRow);
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

            int internIndex = 0;
            string boxText = "";

            if (deleteButton.Name == "Route_1_Delete")
            {
                internIndex = 1;
                boxText = Route_1.Text;
            }
            else if (deleteButton.Name == "Route_2_Delete")
            {
                internIndex = 2;
                boxText = Route_2.Text;
            }
            else if (deleteButton.Name == "Route_3_Delete")
            {
                internIndex = 3;
                boxText = Route_3.Text;
            }
            else if (deleteButton.Name == "Route_4_Delete")
            {
                internIndex = 4;
                boxText = Route_4.Text;
            }

            DeleteManualAddedRoute(internIndex, boxText);
        }
        private void DeleteManualAddedRoute(int index, string boxnr)
        {
            if (string.IsNullOrWhiteSpace(boxnr))
            {
                output.Text = "keine route";
            }
            else
            {

                switch (index)
                {
                    case 1:
                        if (!string.IsNullOrWhiteSpace(Route_1.Text))
                        {
                            Route_1.Text = "";
                            if (GetOutput())
                            {
                                foreach (var child in NetworkGridContainer.Children)
                                {
                                    if (child is ComboBox comboBox)
                                    {
                                        comboBox.Items.Remove(ManualAddedNetworks[1].Item4);
                                    }
                                }
                            }
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrWhiteSpace(Route_2.Text))
                        {
                            Route_2.Text = "";
                            if (GetOutput())
                            {
                                foreach (var child in NetworkGridContainer.Children)
                                {
                                    if (child is ComboBox comboBox)
                                    {
                                        comboBox.Items.Remove(ManualAddedNetworks[2].Item4);
                                    }
                                }
                            }
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrWhiteSpace(Route_3.Text))
                        {
                            Route_3.Text = "";
                            if (GetOutput())
                            {
                                foreach (var child in NetworkGridContainer.Children)
                                {
                                    if (child is ComboBox comboBox)
                                    {
                                        comboBox.Items.Remove(ManualAddedNetworks[3].Item4);
                                    }
                                }
                            }
                        }
                        break;
                    case 4:
                        if (!string.IsNullOrWhiteSpace(Route_4.Text))
                        {
                            Route_4.Text = "";
                            if (GetOutput())
                            {
                                foreach (var child in NetworkGridContainer.Children)
                                {
                                    if (child is ComboBox comboBox)
                                    {
                                        comboBox.Items.Remove(ManualAddedNetworks[4].Item4);
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                ManualAddedNetworks[index] = Tuple.Create("", "", "", "");
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
            if (ShowGateWay)
            {
                GateWayButton.Visibility = Visibility.Visible;
                Eingabe_Button.Visibility = Visibility.Hidden;
                Gate_Block.Focus();
            }
            else
            {
                GateWayButton.Visibility = Visibility.Hidden;
                Eingabe_Button.Visibility = Visibility.Visible;
            }
        }
    }
}
