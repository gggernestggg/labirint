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
    /// </summary>
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HelloWorld
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        int clicks;
        public MainWindow()
        {
            InitializeComponent();
            clicks = 2+3;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // counter.Text = "зачем ты нажал !!!!!!!!!!!!!!!!!!"; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;
            clicks = clicks + 1;
            counter.FontSize = clicks;
            counter.Text = clicks.ToString(); ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; ;

        }
    }
}





