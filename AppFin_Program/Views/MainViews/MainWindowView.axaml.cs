using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using Avalonia.Diagnostics;

namespace AppFin_Program.Views.MainViews
{
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
        }
        public void Button_Click_Registration(object source, RoutedEventArgs e)
        {
            Debug.WriteLine("Click!");
        }
        public void Button_Click_Authorization(object source, RoutedEventArgs e)
        {
            Debug.WriteLine("Click!");
        }
    }
}