using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SensorApp.Annotations;

namespace SensorApp {
    public sealed partial class FlyPage : INotifyPropertyChanged {
        private readonly Inclinometer _inclinometer;
        private readonly double _skyModificatorX;
        private readonly double _skyModificatorY;
        private readonly double _mountainModificatorX;
        private Image[] _groundImages;
        private Image[] _skyImages;
        private Image[] _mountainImages;
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double XSpeed { get; set; }
        public double YSpeed { get; set; }
        public double Score { get; set; }

        public FlyPage() {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            // Set some values
            YSpeed = 17;
            XSpeed = 0;
            Score = 0;
            _skyModificatorX = .02;
            _skyModificatorY = .008;
            _mountainModificatorX = .05;
            // Inizialize inclinometer
            _inclinometer = Inclinometer.GetDefault();
            if (_inclinometer != null) {
                var minReportInterval = _inclinometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;
                _inclinometer.ReadingTransform = DisplayOrientations.Landscape;
                _inclinometer.ReadingChanged += ReadingChanged;
            }
            // Call UI initialization methods on Loaded
            Loaded += delegate {
                InitCanvas();
                InitPlane();
                InitGround();
                InitSky();
                InitMountains();
#if !DEBUG
                DebugInfo.Visibility = Visibility.Collapsed;
#endif
            };
        }

        /// <summary>
        /// Call UpdateView on every sensor reading update
        /// </summary>
        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var reading = e.Reading;
                UpdateView(reading);
            });
        }

        /// <summary>
        /// Update the view with new valus from Inclinometer reading
        /// </summary>
        private void UpdateView(InclinometerReading reading) {
            Pitch = reading.PitchDegrees;
            Roll = reading.RollDegrees;
            Yaw = reading.YawDegrees;
            OnPropertyChanged(nameof(Pitch));
            OnPropertyChanged(nameof(Roll));
            OnPropertyChanged(nameof(Yaw));

            Update();
        }

        /// <summary>
        /// Main Update Method
        /// </summary>
        private void Update() {
            // Rotate airplane
            Airplane.RenderTransform = new RotateTransform {Angle = -Roll};
            // Calculate XSpeed
            XSpeed = Math.Round(GrowthFunction(Roll), 1);
            OnPropertyChanged(nameof(XSpeed));
            // Update each ground image
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
            // Update each sky image
            foreach (var skyImage in _skyImages) {
                var left = Canvas.GetLeft(skyImage);
                var newLeft = left;

                if (Roll > 0) {
                    newLeft = left + XSpeed * _skyModificatorX;
                }
                else if (Roll < 0) {
                    newLeft = left - XSpeed * _skyModificatorX;
                }

                var top = Canvas.GetTop(skyImage);
                var newTop = top - YSpeed * _skyModificatorY;
                PositionInCanvas(skyImage, newLeft, newTop);
                CheckSkyOutOfBound(skyImage);
            }
            // Update each mountain image
            foreach (var mountainImage in _mountainImages) {
                var left = Canvas.GetLeft(mountainImage);
                var top = Canvas.GetTop(mountainImage);
                var newLeft = left;

                if (Roll > 0) {
                    newLeft = left + XSpeed * _mountainModificatorX;
                }
                else if (Roll < 0) {
                    newLeft = left - XSpeed * _mountainModificatorX;
                }

                PositionInCanvas(mountainImage, newLeft, top);
                CheckMountainOutOfBound(mountainImage);
            }
            CalculateScore();
        }

        /// <summary>
        /// Calculates a score from speeds
        /// </summary>
        private void CalculateScore() {
            var positionDelta = Math.Sqrt(Math.Pow(Math.Abs(XSpeed), 2) + Math.Pow(Math.Abs(YSpeed), 2));
            Score += positionDelta;
            OnPropertyChanged(nameof(Score));
        }

        /// <summary>
        /// Set plane position on canvas
        /// </summary>
        private void InitPlane() {
            double left = (PlaneArea.ActualWidth - Airplane.ActualWidth) / 2;
            Canvas.SetLeft(Airplane, left);

            double top = (PlaneArea.ActualHeight - Airplane.ActualHeight) / 4 * 3;
            Canvas.SetTop(Airplane, top);
        }

        /// <summary>
        /// Set canvas sizes and add projections for 3D look
        /// </summary>
        private void InitCanvas() {
            // Transform ground canvas so it looks 3D
            var groundProjection = new PlaneProjection {
                RotationX = -86,
                GlobalOffsetY = 10,
                GlobalOffsetZ = 320
            };
            GroundDrawArea.Projection = groundProjection;
            GroundDrawArea.Height = This.ActualHeight;
            GroundDrawArea.Width = This.ActualWidth;
            // Limit sky to upper screen half
            SkyBox.Clip = new RectangleGeometry {
                Rect = new Rect {
                    X = 0,
                    Y = 0,
                    Height = This.ActualHeight / 2,
                    Width = This.ActualWidth
                }
            };
            // Transform sky canvas so it looks 3D
            var skyProjection = new PlaneProjection {
                RotationX = 55,
                GlobalOffsetY = -50,
                GlobalOffsetZ = 150
            };
            SkyDrawArea.Projection = skyProjection;
            SkyDrawArea.Height = This.ActualHeight;
            SkyDrawArea.Width = This.ActualWidth;
            // Set mountain canvas size
            MountainDrawArea.Height = This.ActualHeight / 2;
            MountainDrawArea.Width = This.ActualWidth;
        }

        /// <summary>
        /// Fill _groundImages array with images and initialize them in canvas
        /// </summary>
        private void InitGround() {
            var width = This.ActualWidth;
            var image = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/ground.png"));
            _groundImages = new[] {
                new Image() {Width = width, Height = width, Source = image},
                new Image() {Width = width, Height = width, Source = image},
                new Image() {Width = width, Height = width, Source = image},
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

        /// <summary>
        /// Fill _skyImages array with images and initialize them in canvas
        /// </summary>
        private void InitSky() {
            var image = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/big-sky.png"));
            const int height = 270;
            const int width = 1728;
            _skyImages = new[] {
                new Image() {Height = height, Width = width, Source = image},
                new Image() {Height = height, Width = width, Source = image},
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

        /// <summary>
        /// Fill _mountainImages array with images and initialize them in canvas
        /// </summary>
        private void InitMountains() {
            var image = new BitmapImage(new Uri("ms-appx:///../Assets/MyImages/mountains.png"));
            const double height = 110 * 0.5;
            const double width = 2098 * 0.5;
            _mountainImages = new[] {
                new Image() {Height = height, Width = width, Source = image},
                new Image() {Height = height, Width = width, Source = image}
            };
            var i = 0;
            foreach (var mountainImage in _mountainImages) {
                MountainDrawArea.Children.Add(mountainImage);
                Canvas.SetZIndex(mountainImage, -80);
                InitMountainImage(i);
                i++;
            }
        }

        /// <summary>
        /// Method for initial mountain image positioning
        /// </summary>
        private void InitMountainImage(int index) {
            double x, y;
            var areaHeight = MountainDrawArea.ActualHeight;
            var areaWidth = MountainDrawArea.ActualWidth;
            var width = _mountainImages[index].Width;
            var height = _mountainImages[index].Height;
            switch (index) {
                case 0:
                    x = areaWidth / 2;
                    y = areaHeight - height;
                    break;
                case 1:
                    x = areaWidth / 2 - width + 1;
                    y = areaHeight - height;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(_mountainImages[index], x, y);
        }

        /// <summary>
        /// Method for initial ground image positioning
        /// </summary>
        private void InitGroundImage(int index) {
            double x, y;
            var width = GroundDrawArea.ActualWidth;
            var height = GroundDrawArea.ActualHeight;
            switch (index) {
                case 0:
                    x = -width / 2 +1;
                    y = -width + height / 2;
                    break;
                case 1:
                    x = width / 2;
                    y = -width + height / 2;
                    break;
                case 2:
                    x = -width / 2 +1;
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

        /// <summary>
        /// Method for initial sky image positioning
        /// </summary>
        private void InitSkyImage(int index) {
            double x, y;
            var areaHeight = SkyDrawArea.ActualHeight;
            var areaWidth = SkyDrawArea.ActualWidth;
            var width = _skyImages[index].ActualWidth;
            var height = _skyImages[index].ActualHeight;
            switch (index) {
                case 0:
                    x = -width + areaWidth / 2 +1;
                    y = -height + areaHeight / 2+1;
                    break;
                case 1:
                    x = areaWidth / 2;
                    y = -height + areaHeight / 2+1;
                    break;
                case 2:
                    x = -width + areaWidth / 2+1;
                    y = areaHeight / 2;
                    break;
                case 3:
                    x = areaWidth / 2;
                    y = areaHeight / 2;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(_skyImages[index], x, y);
        }

        /// <summary>
        /// Check if ground image is out of bounds and move to opposite side of canvas
        /// </summary>
        private static void CheckGroundImageBounds(Image groundImage) {
            // Move ground image to opposite side if it leaves screen
            var width = groundImage.Width;
            var height = groundImage.Height;

            var top = Canvas.GetTop(groundImage);
            var left = Canvas.GetLeft(groundImage);

            if (left > width) {
                Canvas.SetLeft(groundImage, left - 2 * width +2);
            }
            else if (left < -width) {
                Canvas.SetLeft(groundImage, left + 2 * width -2);
            }

            if (top > height) {
                Canvas.SetTop(groundImage, top - 2 * height + 2);
            }
            else if (top < -height) {
                Canvas.SetTop(groundImage, top + 2 * height - 2);
            }
        }

        /// <summary>
        /// Check if sky image is out of bounds and move to opposite side of canvas
        /// </summary>
        private static void CheckSkyOutOfBound(Image skyImage) {
            // Move sky image to opposite side if it leaves screen
            var width = skyImage.Width;
            var height = skyImage.Height;
            var left = Canvas.GetLeft(skyImage);
            var top = Canvas.GetTop(skyImage);

            if (left > width) {
                Canvas.SetLeft(skyImage, left - 2 * width + 2);
            }
            else if (left < -width) {
                Canvas.SetLeft(skyImage, left + 2 * width - 2);
            }

            if (top > height) {
                Canvas.SetTop(skyImage, top - 2 * height + 2);
            }
            else if (top < -height) {
                Canvas.SetTop(skyImage, top + 2 * height - 2);
            }
        }

        /// <summary>
        /// Check if mountain image is out of bounds and move to opposite side of canvas
        /// </summary>
        private static void CheckMountainOutOfBound(Image mountainImage) {
            // Move mountain image to opposite side if it leaves screen
            var width = mountainImage.Width;
            var left = Canvas.GetLeft(mountainImage);

            if (left > width) {
                Canvas.SetLeft(mountainImage, left - 2 * width + 2);
            }
            else if (left < -width) {
                Canvas.SetLeft(mountainImage, left + 2 * width - 2);
            }
        }

        /// <summary>
        /// Limited growth function to calculate XSpeed from roll angle
        /// </summary>
        private static double GrowthFunction(double x) {
            const double max = 12;
            const double growthConst = -.018;
            x = Math.Abs(x);
            return max - max * Math.Pow(Math.E, growthConst * x);
        }

        /// <summary>
        /// Position an UIElement on a canvas
        /// </summary>
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