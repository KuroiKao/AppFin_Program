using AppFin_Program.ViewModels.MainViewModels;
using AppFin_Program.Views.MainViews;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AppFin_Program
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                var dbContextFactory = new Factory.DbContextFactory(connectionString!);

                desktop.MainWindow = new MainWindowView
                {
                    DataContext = new MainWindowViewModel(dbContextFactory),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}