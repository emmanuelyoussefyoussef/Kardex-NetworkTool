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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Network_Window.Components
{
    /// <summary>
    /// Interaktionslogik für DeleteButton.xaml
    /// </summary>
    public partial class DeleteButton : UserControl
    {
        public DeleteButton()
        {
            InitializeComponent();
        }
        public static readonly RoutedEvent JoinClickEvent =
            EventManager.RegisterRoutedEvent(nameof(JoinClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DeleteButton));

        public event RoutedEventHandler JoinClick
        {
            add 
            { 
                AddHandler(JoinClickEvent, value); 
            }
            remove
            {
                RemoveHandler(JoinClickEvent, value);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(JoinClickEvent));
        }
    }
}
