using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SensorApp.Annotations;

namespace SensorApp {
    public sealed partial class FlyPage : Page, INotifyPropertyChanged {
        private readonly Inclinometer _inclinometer;
        private readonly double _skyModificator;
        private Image[] _groundImages;
        private Image[] _skyImages;
        private int _currentFrame;

        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double XSpeed { get; set; }
        public double YSpeed { get; set; }

        public FlyPage() {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            YSpeed = 15;
            XSpeed = 0;
            _currentFrame = 0;
            _skyModificator = .25;
            _inclinometer = Inclinometer.GetDefault();

            if (_inclinometer != null) {
                var minReportInterval = _inclinometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;
                _inclinometer.ReadingTransform = DisplayOrientations.Landscape;
                _inclinometer.ReadingChanged += ReadingChanged;
            }
            // Initialize everything on Loaded
            Loaded += delegate {
                InitCanvas();
                InitPlane();
                InitGround();
                InitSky();
            };
        }

        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                // Render evere second frame
                if (_currentFrame % 2 == 0) {
                    var reading = e.Reading;
                    UpdateView(reading);
                    _currentFrame = 0;
                }
                _currentFrame++;
            });
        }

        private void UpdateView(InclinometerReading reading) {
            Pitch = reading.PitchDegrees;
            Roll = reading.RollDegrees;
            Yaw = reading.YawDegrees;
            OnPropertyChanged(nameof(Pitch));
            OnPropertyChanged(nameof(Roll));
            OnPropertyChanged(nameof(Yaw));

            UpdatePlane(-Roll);
            UpdateBackground();
        }

        private void InitPlane() {
            double left = (PlaneArea.ActualWidth - Airplane.ActualWidth) / 2;
            Canvas.SetLeft(Airplane, left);

            double top = (PlaneArea.ActualHeight - Airplane.ActualHeight) / 3 * 2;
            Canvas.SetTop(Airplane, top);
        }

        private void InitCanvas() {
            // Transform ground canvas so it looks 3D
            var projection = new PlaneProjection {
                RotationX = -86,
                GlobalOffsetY = 10,
                GlobalOffsetZ = 320
            };
            GroundDrawArea.Projection = projection;
            GroundDrawArea.Height = This.ActualHeight;
            GroundDrawArea.Width = This.ActualWidth;
        }

        private void InitGround() {
            // Create array of ground images
            var width = GroundDrawArea.ActualWidth;
            var image = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/pattern.png"));
            var imageInverted = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/pattern-inverted.png"));

            _groundImages = new[] {
                new Image() {Width = width, Height = width, Source = image},
                new Image() {Width = width, Height = width, Source = imageInverted},
                new Image() {Width = width, Height = width, Source = imageInverted},
                new Image() {Width = width, Height = width, Source = image},
            };
            var i = 0;
            foreach (var backgroundImage in _groundImages) {
                GroundDrawArea.Children.Add(backgroundImage);
                Canvas.SetZIndex(backgroundImage, -99);
                InitGroundImage(i);
                i++;
            }
        }

        private void InitSky() {
            // Create array of sky images
            var image = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/big-sky.png"));
            var areaHeight = SkyDrawArea.ActualHeight;
            var height = areaHeight / 2;
            var width = height / 270 * 1728;
            _skyImages = new[] {
                new Image() {Height = height, Width = width, Source = image},
                new Image() {Height = height, Width = width, Source = image},
            };
            var i = 0;
            foreach (var skyImage in _skyImages) {
                SkyDrawArea.Children.Add(skyImage);
                Canvas.SetZIndex(skyImage, -90);
                InitSkyImage(i);
                i++;
            }
        }

        private void UpdateBackground() {
            // Calculate XSpeed
            XSpeed = Math.Round(GrowthFunction(Roll), 1);
            OnPropertyChanged(nameof(XSpeed));
            // Update each ground image based on XSpeed
            foreach (var groundImage in _groundImages) {
                var top = Canvas.GetTop(groundImage);
                var left = Canvas.GetLeft(groundImage);
                var newTop = top + YSpeed;
                var newLeft = left;

                if (Roll > 0) {
                    newLeft = left + XSpeed;
                }
                else if (Roll < 0) {
                    newLeft = left - XSpeed;
                }

                PositionInCanvas(groundImage, newLeft, newTop);
                CheckGroundImageBounds(groundImage);
            }
            // Update each sky image based on XSpeed
            foreach (var skyImage in _skyImages) {
                var left = Canvas.GetLeft(skyImage);
                var newLeft = left;

                if (Roll > 0) {
                    newLeft = left + XSpeed * _skyModificator;
                }
                else if (Roll < 0) {
                    newLeft = left - XSpeed * _skyModificator;
                }

                PositionInCanvas(skyImage, newLeft, 0);
                CheckSkyOutOfBound(skyImage);
            }
        }

        private void UpdatePlane(double angle) { Airplane.RenderTransform = new RotateTransform { Angle = angle }; }

        private void CheckGroundImageBounds(Image image) {
            // Move ground image to opposite side if it leaves screen
            var width = GroundDrawArea.ActualWidth;
            var imgWidth = image.ActualWidth;
            var height = imgWidth;

            var top = Canvas.GetTop(image);
            var left = Canvas.GetLeft(image);

            if (left > width) {
                Canvas.SetLeft(image, left - 2 * imgWidth);
            }
            else if (left < -width) {
                Canvas.SetLeft(image, left + 2 * imgWidth);
            }

            if (top > width) {
                Canvas.SetTop(image, top - 2 * height);
            }
            else if (top < -width) {
                Canvas.SetTop(image, top + 2 * height);
            }
        }

        private void CheckSkyOutOfBound(Image skyImage) {
            // Move sky image to opposite side if it leaves screen
            var width = skyImage.Width;
            var left = Canvas.GetLeft(skyImage);

            if (left > width) {
                Canvas.SetLeft(skyImage, left - 2 * width);
            }
            else if (left < -width) {
                Canvas.SetLeft(skyImage, left + 2 * width);
            }
        }

        private void InitGroundImage(int index) {
            double x, y;
            var width = GroundDrawArea.ActualWidth;
            var height = GroundDrawArea.ActualHeight;
            switch (index) {
                case 0:
                    x = -width / 2;
                    y = -width + height / 2;
                    break;
                case 1:
                    x = width / 2;
                    y = -width + height / 2;
                    break;
                case 2:
                    x = -width / 2;
                    y = height / 2;
                    break;
                case 3:
                    x = width / 2;
                    y = height / 2;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(_groundImages[index], x, y);
        }

        private void InitSkyImage(int index) {
            double x, y;
            switch (index) {
                case 0:
                    x = 0;
                    y = 0;
                    break;
                case 1:
                    x = _skyImages[0].ActualWidth;
                    y = 0;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(_skyImages[index], x, y);
        }

        private static double GrowthFunction(double x) {
            // Limited growth function to calculate XSpeed from roll
            const int max = 15;
            const double growthConst = -.025;
            x = Math.Abs(x);

            return max - max * Math.Pow(Math.E, growthConst * x);
        }

        private static void PositionInCanvas(UIElement element, double x, double y) {
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #endregion
    }
}