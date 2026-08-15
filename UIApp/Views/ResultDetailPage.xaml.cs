using UIApp.ViewModels;

namespace UIApp.Views
{
    public partial class ResultDetailPage : ContentPage
    {
        private readonly ResultDetailViewModel _viewModel;

        public ResultDetailPage(ViewModels.ResultDetailViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Enable Open button only if result type is URL
            if (_viewModel.CurrentResult != null)
            {
                OpenButton.IsEnabled = _viewModel.CurrentResult.Type == "URL";
            }
        }
    }
}