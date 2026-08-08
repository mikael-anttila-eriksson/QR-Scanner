namespace UIApp.Views
{
    public partial class ScannerPage : ContentPage
    {
        public ScannerPage(ViewModels.ScannerViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}