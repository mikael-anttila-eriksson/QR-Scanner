namespace UIApp.Views
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(ViewModels.HistoryViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}