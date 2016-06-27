using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccelerometerPage : Page
    {
        private Accelerometer _accelerometer;

        public AccelerometerPage()
        {
            this.InitializeComponent();
            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                // Establish the report interval
                var minReportInterval = _accelerometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var reading = e.Reading;
                var angles = CalculateAngles(reading);
                txtXAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                txtYAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                txtZAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);
                txtAngle.Text = String.Format("{0,5:0.0}°", angles.Y - 90);

                UpdatePlane(angles.Y - 90);
            });
        }

        private Angles CalculateAngles(AccelerometerReading reading)
        {
            var x = reading.AccelerationX;
            var y = reading.AccelerationY;
            var z = reading.AccelerationZ;

            var radius = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            var arx = Math.Acos(x / radius) * 180 / Math.PI;
            var ary = Math.Acos(y / radius) * 180 / Math.PI;
            var arz = Math.Acos(z / radius) * 180 / Math.PI;

            return new Angles(arx, ary, arz);
        }

        private void UpdatePlane(double angle)
        {
            var rotation = new RotateTransform {Angle = angle};
            airplane.RenderTransform = rotation;
        }
    }
}
