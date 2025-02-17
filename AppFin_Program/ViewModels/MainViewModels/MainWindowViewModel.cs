using AppFin_Program.Models;
using AppFin_Program.ViewModels.RoutingViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppFin_Program.ViewModels.MainViewModels
{
    public class MainWindowViewModel : RoutingViewModel
    {
        public RoutingViewModel CurrentRouter { get; }

        public MainWindowViewModel(IDbContextFactory<FinAppDataBaseContext> dbContextFactory)
        : base(dbContextFactory)
        {
            CurrentRouter = new RoutingViewModel(dbContextFactory);
        }

    }
}
