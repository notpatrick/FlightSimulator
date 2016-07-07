using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SensorApp {
    public sealed partial class MainPage : Page {
        public MainPage() {
            InitializeComponent();

            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            Loaded += (sender, args) => {
                PulseButton.Completed += (o, o1) => { PulseButton.Begin(); };
                PulseButton.Begin();
            };
        }

        private void FlyButton_Click(object sender, RoutedEventArgs e) { Frame.Navigate(typeof(FlyPage)); }
    }
}