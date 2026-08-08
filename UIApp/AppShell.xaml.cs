using Microsoft.Maui.Controls;

namespace UIApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Route for result detail page (navigates with GoToAsync("resultdetail?id=123"))
            Routing.RegisterRoute("resultdetail", typeof(Views.ResultDetailPage));
        }
    }
}
