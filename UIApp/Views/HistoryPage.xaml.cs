namespace UIApp.Views
{
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(ViewModels.HistoryViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ViewModels.HistoryViewModel vm)
            {
                await vm.LoadHistory();
            }
        }
    }
}