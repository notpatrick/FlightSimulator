using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
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
        private App _app;
        private Image[] _groundImages;
        private Image[] _skyImages;
        private Image[] _mountainImages;

        public GameState State { get; set; }

        public FlyPage() {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            _app = (App) Application.Current;
            _accelerometer = Accelerometer.GetDefault();
            if (_app.GameSettings.ShowDebugInfo)
                DebugInfo.Visibility = Visibility.Visible;

            Loaded += delegate {
                InitWindow();
                State = new GameState();

                if (_accelerometer != null) {
                    var minReportInterval = _accelerometer.MinimumReportInterval;
                    var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                    _accelerometer.ReportInterval = reportInterval;
                    _accelerometer.ReadingTransform = DisplayOrientations.Landscape;
                    StartFirstUpdate();
                }
            };
            Unloaded += delegate { StopUpdate(); };
        }

        public FlyPage(GameState initialState) {
            InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            _accelerometer = Accelerometer.GetDefault();
            if (_app.GameSettings.ShowDebugInfo)
                DebugInfo.Visibility = Visibility.Visible;

            Loaded += delegate {
                InitWindow();
                State = initialState;

                if (_accelerometer != null) {
                    var minReportInterval = _accelerometer.MinimumReportInterval;
                    var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                    _accelerometer.ReportInterval = reportInterval;
                    _accelerometer.ReadingTransform = DisplayOrientations.Landscape;
                    StartFirstUpdate();
                }
            };
            Unloaded += delegate { StopUpdate(); };
        }

        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e) { await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { Update(e.Reading); }); }

        private void StartButton_Click(object sender, RoutedEventArgs e) { StartUpdate(); }

        private void InitStoryboards() {
            EventHandler<object> fadeOutEventHandler = (sender, o) => {
                if (State.IsRunning) {
                    State.IsRunning = false;
                }
                else {
                    State.IsRunning = true;
                    _accelerometer.ReadingChanged += ReadingChanged;
                }
            };
            EventHandler<object> fadeInEventHandler = (sender, o) => {
                if (State.IsRunning) {
                    UpdateWindow.Visibility = Visibility.Collapsed;
                    PauseWindow.Visibility = Visibility.Visible;
                }
                else {
                    State.ResetSpeeds().ResetAngles().GetNextLocation();
                    UpdateWindow.Visibility = Visibility.Visible;
                    PauseWindow.Visibility = Visibility.Collapsed;
                }
                FadeOutInitialBlackscreenStoryboard.Begin();
            };

            FadeOutInitialBlackscreenStoryboard.Completed += fadeOutEventHandler;
            FadeInInitialBlackscreenStoryboard.Completed += fadeInEventHandler;
        }

        private void StartFirstUpdate() {
            State.ResetSpeeds().ResetAngles().GetNextLocation();
            UpdateWindow.Visibility = Visibility.Visible;
            PauseWindow.Visibility = Visibility.Collapsed;
            InitialBlackscreen.Opacity = 0;
            State.IsRunning = true;
            _accelerometer.ReadingChanged += ReadingChanged;
        }

        #region Update

        private void StartUpdate() { FadeInInitialBlackscreenStoryboard.Begin(); }

        private void StopUpdate() {
            _accelerometer.ReadingChanged -= ReadingChanged;
            FadeInInitialBlackscreenStoryboard.Begin();
        }

        // Main Update Method
        private void Update(AccelerometerReading reading) {
            State.Angles = Angles.CalculateAngles(reading);
            // Calculate Speeds
            CalculateSpeedY();
            CalculateSpeedX();
            // Check if it should stop
            if (State.SpeedY < _app.GameSettings.MinSpeedY) {
                StopUpdate();
                return;
            }
            // Rotate airplane
            Airplane.RenderTransform = new RotateTransform {Angle = -State.Angles.X};
            // Updates
            UpdateGround();
            UpdateSky();
            UpdateMountain();
            UpdateScore();
            OnPropertyChanged(nameof(State));
        }

        // Update ground
        private void UpdateGround() {
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
        }

        // Update sky
        private void UpdateSky() {
            foreach (var skyImage in _skyImages) {
                var top = Canvas.GetTop(skyImage);
                var left = Canvas.GetLeft(skyImage);
                var newTop = top - State.SpeedY * GameSettings.SkyCoeffY;
                var newLeft = left;

                if (State.Angles.X > 0) {
                    newLeft = left + State.SpeedX * GameSettings.SkyCoeffX;
                }
                else if (State.Angles.X < 0) {
                    newLeft = left - State.SpeedX * GameSettings.SkyCoeffX;
                }
                PositionInCanvas(skyImage, newLeft, newTop);
                CheckSkyOutOfBound(skyImage);
            }
        }

        // Update mountain
        private void UpdateMountain() {
            foreach (var mountainImage in _mountainImages) {
                var left = Canvas.GetLeft(mountainImage);
                var top = Canvas.GetTop(mountainImage);
                var newLeft = left;

                if (State.Angles.X > 0) {
                    newLeft = left + State.SpeedX * GameSettings.MountainCoeffX;
                }
                else if (State.Angles.X < 0) {
                    newLeft = left - State.SpeedX * GameSettings.MountainCoeffX;
                }
                PositionInCanvas(mountainImage, newLeft, top);
                CheckMountainOutOfBound(mountainImage);
            }
        }

        // Update score
        private void UpdateScore() {
            var positionDelta = Math.Sqrt(Math.Pow(Math.Abs(State.SpeedX), 2) + Math.Pow(Math.Abs(State.SpeedY), 2));
            State.Score += positionDelta;
        }

        #endregion

        #region Calculations

        // Calculate SpeedX
        private void CalculateSpeedX() {
            var x = Math.Abs(State.Angles.X);
            var value = _app.GameSettings.MaxSpeedX / 60 * x;
            var limitedValue = value < _app.GameSettings.MaxSpeedX ? value : _app.GameSettings.MaxSpeedX;
            State.SpeedX = Math.Round(limitedValue * State.SpeedY / _app.GameSettings.MaxSpeedY, 2);
        }

        // Calculate SpeedY
        private void CalculateSpeedY() {
            var x = State.Angles.Z > _app.GameSettings.VerticalTolerance || State.Angles.Z < -_app.GameSettings.VerticalTolerance ? State.Angles.Z : 0;
            var delta = Math.Pow(Math.E, .002 * x) - 1;
            var newSpeed = Math.Round(State.SpeedY + delta, 2);
            State.SpeedY = newSpeed > _app.GameSettings.MaxSpeedY ? _app.GameSettings.MaxSpeedY : newSpeed;
        }

        #endregion

        #region Initialization

        // Call init methods
        private void InitWindow() {
            PauseWindow.Visibility = Visibility.Collapsed;
            UpdateWindow.Visibility = Visibility.Collapsed;
            InitialBlackscreen.Opacity = 1;
            InitStoryboards();
            InitCanvas();
            InitPlane();
            InitGround();
            InitSky();
            InitMountains();
        }

        // Add EventHandlers to storyboards

        // Set plane position on canvas
        private void InitPlane() {
            var left = (PlaneArea.ActualWidth - Airplane.ActualWidth) * 0.5;
            Canvas.SetLeft(Airplane, left);

            var top = (PlaneArea.ActualHeight - Airplane.ActualHeight) * 0.75;
            Canvas.SetTop(Airplane, top);
        }

        // Set canvas sizes and add projections for 3D look
        private void InitCanvas() {
            var groundProjection = new PlaneProjection {
                RotationX = -86,
                GlobalOffsetY = 10,
                GlobalOffsetZ = 320
            };
            GroundDrawArea.Projection = groundProjection;
            GroundDrawArea.Height = This.ActualHeight;
            GroundDrawArea.Width = This.ActualWidth;
            SkyBox.Clip = new RectangleGeometry {
                Rect = new Rect {
                    X = 0,
                    Y = 0,
                    Height = This.ActualHeight * 0.5,
                    Width = This.ActualWidth
                }
            };
            var skyProjection = new PlaneProjection {
                RotationX = 55,
                GlobalOffsetY = -50,
                GlobalOffsetZ = 150
            };
            SkyDrawArea.Projection = skyProjection;
            SkyDrawArea.Height = This.ActualHeight;
            SkyDrawArea.Width = This.ActualWidth;
            MountainDrawArea.Height = This.ActualHeight * 0.5;
            MountainDrawArea.Width = This.ActualWidth;
        }

        // Fill _groundImages array with images and initialize them in canvas
        private void InitGround() {
            var width = This.ActualWidth;
            var image = new BitmapImage(new Uri(BaseUri, "/Assets/MyImages/ground.png"));
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
                SetInitialGroundPosition(i);
                i++;
            }
        }

        // Fill _skyImages array with images and initialize them in canvas
        private void InitSky() {
            var image = new BitmapImage(new Uri(BaseUri, "/Assets/MyImages/big-sky.png"));
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
                SetInitialSkyPosition(i);
                i++;
            }
        }

        // Fill _mountainImages array with images and initialize them in canvas
        private void InitMountains() {
            var image = new BitmapImage(new Uri(BaseUri, "/Assets/MyImages/mountains.png"));
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
                SetInitialMountainPosition(i);
                i++;
            }
        }

        // Initial mountain image positioning
        private void SetInitialMountainPosition(int index) {
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

        // Initial ground image positioning
        private void SetInitialGroundPosition(int index) {
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

        // Initial sky image positioning
        private void SetInitialSkyPosition(int index) {
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

        #endregion

        #region BoundChecks

        // Check if ground image is out of bounds and move to opposite side of canvas
        private static void CheckGroundOutOfBound(Image groundImage) {
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

        // Check if sky image is out of bounds and move to opposite side of canvas
        private static void CheckSkyOutOfBound(Image skyImage) {
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

        // Check if mountain image is out of bounds and move to opposite side of canvas
        private static void CheckMountainOutOfBound(Image mountainImage) {
            var width = mountainImage.Width;
            var left = Canvas.GetLeft(mountainImage);

            if (left > width) {
                Canvas.SetLeft(mountainImage, left - 2 * width + 2);
            }
            else if (left < -width) {
                Canvas.SetLeft(mountainImage, left + 2 * width - 2);
            }
        }

        #endregion

        #region Statics

        // Position UIElement on canvas
        private static void PositionInCanvas(UIElement element, double x, double y) {
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #endregion
    }
}