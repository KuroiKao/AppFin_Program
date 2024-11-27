using AppFin_Program.ViewModels.WindowViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppFin_Program.Views.WindowViews;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        this.DataContext = new HomeViewModel();
    }
}