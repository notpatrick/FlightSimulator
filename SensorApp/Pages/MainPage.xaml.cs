using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            _app.GameSettings.ShowDebugInfo = !_app.GameSettings.ShowDebugInfo;

            //testAudio = await MyHelpers.LoadSoundFile(@"Assets\MySounds\aircraft008.wav");
            //MainGrid.Children.Add(testAudio);
            //testAudio.Play();
        }
    }
}