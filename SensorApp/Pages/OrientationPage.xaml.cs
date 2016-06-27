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

namespace SensorApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OrientationPage : Page
    {

        private OrientationSensor _orientationSensor;

        public OrientationPage()
        {
            this.InitializeComponent();

            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _orientationSensor = OrientationSensor.GetDefault();
            if (_orientationSensor != null)
            {
                // Establish the report interval
                var minReportInterval = _orientationSensor.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _orientationSensor.ReportInterval = reportInterval;

                _orientationSensor.ReadingTransform = DisplayOrientations.Landscape;
                // Assign an event handler for the reading-changed event
                _orientationSensor.ReadingChanged += new TypedEventHandler<OrientationSensor, OrientationSensorReadingChangedEventArgs>(ReadingChanged);
            }
        }

        private async void ReadingChanged(object sender, OrientationSensorReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var reading = e.Reading;

                txt11.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M11);
                txt12.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M12);
                txt13.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M13);

                txt21.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M21);
                txt22.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M22);
                txt23.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M23);

                txt31.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M31);
                txt32.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M32);
                txt33.Text = String.Format("{0,5:0.00}", reading.RotationMatrix.M33);

                var angle = Math.Asin(reading.RotationMatrix.M31)*180/Math.PI;
                txtAngle.Text = String.Format("{0,5:0.0}°", -angle);
                UpdatePlane(-angle);
            });
        }

        private void UpdatePlane(double angle)
        {
            var rotation = new RotateTransform { Angle = angle };
            airplane.RenderTransform = rotation;
        }
    }
}
