using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SensorApp.Classes;

namespace SensorApp {
    public sealed partial class MainPage : Page {
        private MediaElement testAudio;
        private App _app;

        public MainPage() {
            InitializeComponent();
            _app = (App) Application.Current;
            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            Loaded += (sender, args) => { TestButton.Visibility = Visibility.Visible; };
        }

        private void FlyButton_Click(object sender, RoutedEventArgs e) { Frame.Navigate(typeof(FlyPage)); }

        private async void TestButton_OnClick(object sender, RoutedEventArgs e) {
            testAudio = await MyHelpers.LoadSoundFile(@"Assets\MySounds\aircraft008.wav");
            MainGrid.Children.Add(testAudio);
            testAudio.IsMuted = _app.GameSettings.SoundMuted;
            testAudio.Play();
        }

        private async void LoadButton_OnClick(object sender, RoutedEventArgs e) {
            var state = await MyHelpers.LoadGameState("GameState");
            Frame.Navigate(typeof(FlyPage), state);
        }

        private async void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var previousSettings = new GameSettings(_app.GameSettings);
            ContentDialogResult result = await SettingsContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                MyHelpers.SaveGameSettings(_app.GameSettings);
            }
            else
            {
                _app.GameSettings = previousSettings;
            }
        }

        private void SettingsContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args) {
            EnableDebugInfoCheckBox.IsChecked = _app.GameSettings.ShowDebugInfo;
            EnableSoundCheckBox.IsChecked = _app.GameSettings.SoundMuted;
        }

        private void EnableDebugInfoCheckBox_Checked(object sender, RoutedEventArgs e) { _app.GameSettings.ShowDebugInfo = true; }

        private void EnableDebugInfoCheckBox_Unchecked(object sender, RoutedEventArgs e) { _app.GameSettings.ShowDebugInfo = false; }

        private void EnableSoundCheckBox_Checked(object sender, RoutedEventArgs e) { _app.GameSettings.SoundMuted = true; }
        private void EnableSoundCheckBox_Unchecked(object sender, RoutedEventArgs e) { _app.GameSettings.SoundMuted = false; }

    }
}