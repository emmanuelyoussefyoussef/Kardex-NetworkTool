using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Network_Window.Info_button
{
    /// <summary>
    /// Interaktionslogik für Info_Fenster.xaml
    /// </summary>
    public partial class Info_Fenster : Window
    {
        public Info_Fenster()
        {
            InitializeComponent();
        }

        private void btn_zurück(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Close();
        }
    }
}
