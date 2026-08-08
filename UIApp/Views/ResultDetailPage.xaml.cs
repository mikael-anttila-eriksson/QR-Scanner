namespace UIApp.Views
{
    public partial class ResultDetailPage : ContentPage
    {
        public ResultDetailPage(ViewModels.ResultDetailViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}