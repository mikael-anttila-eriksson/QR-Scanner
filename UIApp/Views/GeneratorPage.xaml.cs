using UIApp.ViewModels;

namespace UIApp.Views
{
    public partial class GeneratorPage : ContentPage
    {
        private readonly GeneratorViewModel _viewModel;

        public GeneratorPage(GeneratorViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}