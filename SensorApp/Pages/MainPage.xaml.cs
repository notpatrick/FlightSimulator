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
            // create dummy save
            var state = new GameState {
                Angles = GameSettings.InitialAngles,
                IsRunning = true,
                Location = Location.GetRandomLocation(),
                Position = new Point(33333, 55555),
                Score = 133337,
                SpeedY = 12,
                SpeedX = 0
            };
            MyHelpers.SaveGameState(state, "GameState");
            //_app.GameSettings.ShowDebugInfo = !_app.GameSettings.ShowDebugInfo;

            //testAudio = await MyHelpers.LoadSoundFile(@"Assets\MySounds\aircraft008.wav");
            //MainGrid.Children.Add(testAudio);
            //testAudio.Play();
        }

        private async void LoadButton_OnClick(object sender, RoutedEventArgs e) {
            var state = await MyHelpers.LoadGameState("GameState");
            Frame.Navigate(typeof(FlyPage), state);
        }
        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) { throw new NotImplementedException(); }
        private void ExitButton_OnClick(object sender, RoutedEventArgs e) { throw new NotImplementedException(); }
    }
}