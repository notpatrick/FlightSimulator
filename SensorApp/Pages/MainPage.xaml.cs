using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
