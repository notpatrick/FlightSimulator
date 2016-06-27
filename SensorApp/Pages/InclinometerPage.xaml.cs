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
    public sealed partial class InclinometerPage : Page
    {
        private Inclinometer _inclinometer;
        public InclinometerPage()
        {
            this.InitializeComponent();
            // Force landscape orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _inclinometer = Inclinometer.GetDefault();
            if (_inclinometer != null)
            {
                // Establish the report interval
                var minReportInterval = _inclinometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;

                _inclinometer.ReadingTransform = DisplayOrientations.Landscape;
                // Assign an event handler for the reading-changed event
                _inclinometer.ReadingChanged += new TypedEventHandler<Inclinometer, InclinometerReadingChangedEventArgs>(ReadingChanged);
            }
        }

        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var reading = e.Reading;
                txtPitch.Text = String.Format("{0,5:0.0}°", reading.PitchDegrees);
                txtRoll.Text = String.Format("{0,5:0.0}°", reading.RollDegrees);
                txtYaw.Text = String.Format("{0,5:0.0}°", reading.YawDegrees);

                UpdatePlane(-reading.RollDegrees);
            });
        }

        private void UpdatePlane(double angle)
        {
            var rotation = new RotateTransform { Angle = angle};
            airplane.RenderTransform = rotation;
        }
    }
}
