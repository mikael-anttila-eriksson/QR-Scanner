using Microsoft.Maui.Controls;

namespace UIApp
{
    public partial class AppShell : Shell
    {
        public const string ResultDetailRoute = nameof(ResultDetailRoute);

        public AppShell()
        {
            InitializeComponent();
            // Route for result detail page (navigates with GoToAsync($"{ResultDetailRoute}?id=123"))
            Routing.RegisterRoute(ResultDetailRoute, typeof(Views.ResultDetailPage));
        }
    }
}
