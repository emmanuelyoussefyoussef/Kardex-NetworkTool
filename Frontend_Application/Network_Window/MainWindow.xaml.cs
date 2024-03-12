using Network_Window.Info_button;
using System.Text;
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
        public MainWindow()
        {
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
            MessageBox.Show(" your Ip " + impIP +" your mask " + impMask + " your gateway " + impGateway +" your index " + impIndex);
            IP_Adresse_Block.Clear();
            Mask_Block.Clear();
            Gate_Block.Clear();
            Index_Block.Clear();
        }
    }
}