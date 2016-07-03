using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SensorApp.Annotations;

namespace SensorApp {
    public sealed partial class AccelerometerPage : Page, INotifyPropertyChanged {
        private Accelerometer _accelerometer;
        public double XAxis { get; set; }
        public double YAxis { get; set; }
        public double ZAxis { get; set; }
        public double Angle { get; set; }

        public AccelerometerPage() {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null) {
                var minReportInterval = _accelerometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                _accelerometer.ReadingChanged += ReadingChanged;
            }
        }

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var reading = e.Reading;
                UpdateView(reading);
            });
        }

        private void UpdateView(AccelerometerReading reading) {
            var angles = CalculateAngles(reading);
            XAxis = reading.AccelerationX;
            YAxis = reading.AccelerationY;
            ZAxis = reading.AccelerationZ;
            Angle = angles.Y - 90;
            OnPropertyChanged(nameof(XAxis));
            OnPropertyChanged(nameof(YAxis));
            OnPropertyChanged(nameof(ZAxis));
            OnPropertyChanged(nameof(Angle));

            UpdatePlane(Angle);
        }

        private void UpdatePlane(double angle) {
            var rotation = new RotateTransform {Angle = angle};
            Airplane.RenderTransform = rotation;
        }

        private static Angles CalculateAngles(AccelerometerReading reading) {
            var x = reading.AccelerationX;
            var y = reading.AccelerationY;
            var z = reading.AccelerationZ;

            var radius = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            var arx = Math.Acos(x / radius) * 180 / Math.PI;
            var ary = Math.Acos(y / radius) * 180 / Math.PI;
            var arz = Math.Acos(z / radius) * 180 / Math.PI;

            return new Angles(arx, ary, arz);
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #endregion
    }
}