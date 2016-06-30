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

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InclinometerPage : Page, INotifyPropertyChanged
    {
        private Inclinometer _inclinometer;
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public InclinometerPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _inclinometer = Inclinometer.GetDefault();
            if (_inclinometer != null)
            {
                var minReportInterval = _inclinometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;
                _inclinometer.ReadingTransform = DisplayOrientations.Landscape;
                _inclinometer.ReadingChanged += ReadingChanged;
            }
        }

        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var reading = e.Reading;
                UpdateView(reading);
            });
        }

        private void UpdateView(InclinometerReading reading)
        {
            Pitch = reading.PitchDegrees;
            Roll = reading.RollDegrees;
            Yaw = reading.YawDegrees;
            OnPropertyChanged(nameof(Pitch));
            OnPropertyChanged(nameof(Roll));
            OnPropertyChanged(nameof(Yaw));

            UpdatePlane(-Roll);
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
