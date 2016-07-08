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

        public MainPage() {
            InitializeComponent();

            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            Loaded += (sender, args) => {
                PulseButton.Completed += (o, o1) => { PulseButton.Begin(); };
                //PulseButton.Begin();

                TestButton.Visibility = Visibility.Visible;
            };
        }

        private void FlyButton_Click(object sender, RoutedEventArgs e) { Frame.Navigate(typeof(FlyPage)); }

        private async void TestButton_OnClick(object sender, RoutedEventArgs e) {
            testAudio = await MyHelpers.LoadSoundFile(@"Assets\MySounds\aircraft008.wav");
            MainGrid.Children.Add(testAudio);
            testAudio.Play();
        }
    }
}