using System;
using System.ComponentModel;
using System.Diagnostics;
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
using SensorApp.Classes;

namespace SensorApp {
    public sealed partial class FlyPage : INotifyPropertyChanged {
        private readonly Accelerometer _accelerometer;
        private readonly string _deviceId;
        private Image[] _groundImages;
        private Image[] _skyImages;
        private Image[] _mountainImages;

        public GameState State { get; set; }

        public FlyPage() {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            _deviceId = MyHelpers.GetHardwareId();

            // Inizialize accelerometer
            _accelerometer = Accelerometer.GetDefault();

            // Call UI initialization methods on Loaded
            Loaded += delegate {
                InitStoryboards();
                InitUpdateWindow();

                State = new GameState();
                if (_accelerometer != null) {
                    var minReportInterval = _accelerometer.MinimumReportInterval;
                    var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                    _accelerometer.ReportInterval = reportInterval;
                    _accelerometer.ReadingTransform = DisplayOrientations.Landscape;
                    StartUpdate();
                }
            };
            Unloaded += delegate { MyHelpers.Save(State, _deviceId); };
        }

        private void InitUpdateWindow() {
            InitCanvas();
            InitPlane();
            InitGround();
            InitSky();
            InitMountains();
        }

        private void InitStoryboards() {
            // When Update screen gets shown
            EventHandler<object> lambdaShowUpdate = (sender, o) => { _accelerometer.ReadingChanged += ReadingChanged; };
            ShowUpdateStoryboard.Completed += lambdaShowUpdate;
            // When Pause screen gets shown
            EventHandler<object> lambdaShowPause = (sender, o) => { };
            ShowPauseStoryboard.Completed += lambdaShowPause;
            // When update screen gets hidden
            EventHandler<object> lambdaHideUpdate = (sender, o) => { ShowPauseStoryboard.Begin(); };
            HideUpdateStoryboard.Completed += lambdaHideUpdate;
            // When pause screen gets hidden
            EventHandler<object> lambdaHidePause = (sender, o) => { ShowUpdateStoryboard.Begin(); };
            HidePauseStoryboard.Completed += lambdaHidePause;
        }

        private void StopUpdate() {
            _accelerometer.ReadingChanged -= ReadingChanged;
            HideUpdateStoryboard.Begin();
            MyHelpers.Save(State, _deviceId);
            Debug.WriteLine("Stopped Updating");
        }

        private async void StartUpdate() {
            var fileExist = await MyHelpers.CheckFile(_deviceId);
            if (fileExist) {
                State = await MyHelpers.Load(_deviceId);
                State.ResetSpeeds().ResetAngles();
            }
            HidePauseStoryboard.Begin();
            Debug.WriteLine("Started updating");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) {
            State.ResetSpeeds().ResetAngles();
            StartUpdate();
        }

        /// <summary>
        /// Call UpdateView on every sensor reading update
        /// </summary>
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e) {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                var reading = e.Reading;
                UpdateView(reading);
            });
        }

        /// <summary>
        /// Update the view with new valus from Inclinometer reading
        /// </summary>
        private void UpdateView(AccelerometerReading reading) {
            State.Angles = Angles.CalculateAngles(reading);
            Update();
        }

        /// <summary>
        /// Main Update Method
        /// </summary>
        private void Update() {
            // Rotate airplane
            Airplane.RenderTransform = new RotateTransform {Angle = -State.Angles.X};
            // Calculate SpeedX and SpeedY
            CalculateSpeedY();
            CalculateSpeedX();
            // Check for Stop
            if (State.SpeedY < 0) {
                State.SpeedY = 0;
                State.SpeedX = 0;
                StopUpdate();
                return;
            }
            // Update each ground image
            foreach (var groundImage in _groundImages) {
                var top = Canvas.GetTop(groundImage);
                var left = Canvas.GetLeft(groundImage);
                var newTop = top + State.SpeedY;
                var newLeft = left;

                if (State.Angles.X > 0) {
                    newLeft = left + State.SpeedX;
                }
                else if (State.Angles.X < 0) {
                    newLeft = left - State.SpeedX;
                }

                PositionInCanvas(groundImage, newLeft, newTop);
                CheckGroundOutOfBound(groundImage);
                State.Position = new Point(State.Position.X + left - newLeft, State.Position.Y + top - newTop);
            }
            // Update each sky image
            foreach (var skyImage in _skyImages) {
                var top = Canvas.GetTop(skyImage);
                var left = Canvas.GetLeft(skyImage);
                var newTop = top - State.SpeedY * GameConstants.SkyCoeffY;
                var newLeft = left;

                if (State.Angles.X > 0) {
                    newLeft = left + State.SpeedX * GameConstants.SkyCoeffX;
                }
                else if (State.Angles.X < 0) {
                    newLeft = left - State.SpeedX * GameConstants.SkyCoeffX;
                }
                PositionInCanvas(skyImage, newLeft, newTop);
                CheckSkyOutOfBound(skyImage);
            }
            // Update each mountain image
            foreach (var mountainImage in _mountainImages) {
                var left = Canvas.GetLeft(mountainImage);
                var top = Canvas.GetTop(mountainImage);
                var newLeft = left;

                if (State.Angles.X > 0) {
                    newLeft = left + State.SpeedX * GameConstants.MountainCoeffX;
                }
                else if (State.Angles.X < 0) {
                    newLeft = left - State.SpeedX * GameConstants.MountainCoeffX;
                }
                PositionInCanvas(mountainImage, newLeft, top);
                CheckMountainOutOfBound(mountainImage);
            }
            UpdateScore();
            OnPropertyChanged(nameof(State));
        }

        /// <summary>
        /// Calculates a score from speeds
        /// </summary>
        private void UpdateScore() {
            var positionDelta = Math.Sqrt(Math.Pow(Math.Abs(State.SpeedX), 2) + Math.Pow(Math.Abs(State.SpeedY), 2));
            State.Score += positionDelta;
        }

        /// <summary>
        /// Set plane position on canvas
        /// </summary>
        private void InitPlane() {
            double left = (PlaneArea.ActualWidth - Airplane.ActualWidth) * 0.5;
            Canvas.SetLeft(Airplane, left);

            double top = (PlaneArea.ActualHeight - Airplane.ActualHeight) * 0.75;
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
                    Height = This.ActualHeight * 0.5,
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
            MountainDrawArea.Height = This.ActualHeight * 0.5;
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
        /// Initial mountain image positioning
        /// </summary>
        private void InitMountainImage(int index) {
            double x, y;
            var areaHeight = MountainDrawArea.ActualHeight;
            var areaWidth = MountainDrawArea.ActualWidth;
            var width = _mountainImages[index].Width;
            var height = _mountainImages[index].Height;
            switch (index) {
                case 0:
                    x = areaWidth * 0.5;
                    y = areaHeight - height;
                    break;
                case 1:
                    x = areaWidth * 0.5 - width + 1;
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
        /// Initial ground image positioning
        /// </summary>
        private void InitGroundImage(int index) {
            double x, y;
            var width = GroundDrawArea.ActualWidth;
            var height = GroundDrawArea.ActualHeight;
            switch (index) {
                case 0:
                    x = -width * 0.5 + 1;
                    y = -width + height * 0.5;
                    break;
                case 1:
                    x = width * 0.5;
                    y = -width + height * 0.5;
                    break;
                case 2:
                    x = -width * 0.5 + 1;
                    y = height * 0.5;
                    break;
                case 3:
                    x = width * 0.5;
                    y = height * 0.5;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(_groundImages[index], x, y);
        }

        /// <summary>
        /// Initial sky image positioning
        /// </summary>
        private void InitSkyImage(int index) {
            double x, y;
            var areaHeight = SkyDrawArea.ActualHeight;
            var areaWidth = SkyDrawArea.ActualWidth;
            var width = _skyImages[index].ActualWidth;
            var height = _skyImages[index].ActualHeight;
            switch (index) {
                case 0:
                    x = -width + areaWidth * 0.5 + 1;
                    y = -height + areaHeight * 0.5 + 1;
                    break;
                case 1:
                    x = areaWidth * 0.5;
                    y = -height + areaHeight * 0.5 + 1;
                    break;
                case 2:
                    x = -width + areaWidth * 0.5 + 1;
                    y = areaHeight * 0.5;
                    break;
                case 3:
                    x = areaWidth * 0.5;
                    y = areaHeight * 0.5;
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
        private static void CheckGroundOutOfBound(Image groundImage) {
            // Move ground image to opposite side if it leaves screen
            var width = groundImage.Width;
            var height = groundImage.Height;

            var top = Canvas.GetTop(groundImage);
            var left = Canvas.GetLeft(groundImage);

            if (left > width) {
                Canvas.SetLeft(groundImage, left - 2 * width + 2);
            }
            else if (left < -width) {
                Canvas.SetLeft(groundImage, left + 2 * width - 2);
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
        /// Linear growth function to calculate SpeedX from roll angle
        /// </summary>
        private void CalculateSpeedX() {
            var x = Math.Abs(State.Angles.X);
            var value = GameConstants.MaxSpeedX / 60 * x;
            var limitedValue = value < GameConstants.MaxSpeedX ? value : GameConstants.MaxSpeedX;
            State.SpeedX = Math.Round(limitedValue * State.SpeedY / GameConstants.MaxSpeedY, 2);
        }

        private void CalculateSpeedY() {
            const double growconst = .002;
            var x = State.Angles.Z > GameConstants.VerticalTolerance || State.Angles.Z < -GameConstants.VerticalTolerance ? State.Angles.Z : 0;
            var delta = Math.Pow(Math.E, growconst * x) - 1;
            var newSpeed = Math.Round(State.SpeedY + delta, 2);
            State.SpeedY = newSpeed > GameConstants.MaxSpeedY ? GameConstants.MaxSpeedY : newSpeed;
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