using AppFin_Program.ViewModels.StartViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AppFin_Program.Views.StartViews;

public partial class RegistrationView : UserControl
{
    public RegistrationView()
    {
        InitializeComponent();
        this.DataContext = new RegistrationViewModel();
    }
}