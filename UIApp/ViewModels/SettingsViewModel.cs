using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace UIApp.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private const string SOUND_KEY = "settings_sound_on";
        private const string HAPTIC_KEY = "settings_haptic_on";

        [ObservableProperty]
        public partial bool IsSoundEnabled { get; set; }

        [ObservableProperty]
        public partial bool IsHapticEnabled { get; set; }

        public SettingsViewModel()
        {
            // Load persisted preferences
            IsSoundEnabled = Preferences.Get(SOUND_KEY, true);
            IsHapticEnabled = Preferences.Get(HAPTIC_KEY, true);

            // Watch changes and persist
            PropertyChanged += SettingsViewModel_PropertyChanged;
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSoundEnabled))
            {
                Preferences.Set(SOUND_KEY, IsSoundEnabled);
            }
            else if (e.PropertyName == nameof(IsHapticEnabled))
            {
                Preferences.Set(HAPTIC_KEY, IsHapticEnabled);
            }
        }
    }
}