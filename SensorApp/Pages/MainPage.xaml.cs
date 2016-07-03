using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SensorApp.Pages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
        }

        private void AccelerometerButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AccelerometerPage));
        }

        private void InclinometerButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(InclinometerPage));
        }

        private void OrientationButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OrientationPage));
        }

        private void FlyButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FlyPage));
        }
    }
}
