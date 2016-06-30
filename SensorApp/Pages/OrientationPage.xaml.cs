using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using SensorApp.Annotations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SensorApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrientationPage : Page, INotifyPropertyChanged
    {

        private OrientationSensor _orientationSensor;
        public SensorRotationMatrix Matrix { get; set; }
        public double Angle { get; set; }
        public OrientationPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _orientationSensor = OrientationSensor.GetDefault();
            if (_orientationSensor != null)
            {
                var minReportInterval = _orientationSensor.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _orientationSensor.ReportInterval = reportInterval;

                _orientationSensor.ReadingTransform = DisplayOrientations.Landscape;
                _orientationSensor.ReadingChanged += ReadingChanged;
            }
        }

        private async void ReadingChanged(object sender, OrientationSensorReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var reading = e.Reading;
                UpdateView(reading);
            });
        }

        private void UpdateView(OrientationSensorReading reading)
        {
            Matrix = reading.RotationMatrix;
            Angle = Math.Asin(reading.RotationMatrix.M31) * 180 / Math.PI;
            OnPropertyChanged(nameof(Matrix));
            OnPropertyChanged(nameof(Angle));

            UpdatePlane(-Angle);
        }

        private void UpdatePlane(double angle)
        {
            var rotation = new RotateTransform { Angle = angle };
            Airplane.RenderTransform = rotation;
        }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
