using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SensorApp.Annotations;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SensorApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FlyPage : Page, INotifyPropertyChanged
    {
        private Inclinometer _inclinometer;
        private Image[] backgroundImages;
        private Image[] skyImages;
        private int frame;
        public double Pitch { get; set; }
        public double Roll { get; set; }
        public double Yaw { get; set; }
        public double XSpeed { get; set; }
        public double YSpeed { get; set; }
        
        public FlyPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            YSpeed = 15;
            XSpeed = 0;
            frame = 0;

            _inclinometer = Inclinometer.GetDefault();
            if (_inclinometer != null)
            {
                var minReportInterval = _inclinometer.MinimumReportInterval;
                var reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;
                _inclinometer.ReadingTransform = DisplayOrientations.Landscape;
                _inclinometer.ReadingChanged += ReadingChanged;
            }

            Loaded += delegate
            {
                InitCanvas();
                InitPlane();
                InitBackground();
                InitSky();
            };
        }

        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (frame%2 == 0)
                {
                    var reading = e.Reading;
                    UpdateView(reading);
                }
                frame++;
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
            UpdateBackground();
        }

        private void UpdatePlane(double angle)
        {
            var rotation = new RotateTransform { Angle = angle };
            Airplane.RenderTransform = rotation;
        }

        private void InitPlane()
        {
            double left = (PlaneArea.ActualWidth - Airplane.ActualWidth) / 2;
            Canvas.SetLeft(Airplane, left);

            double top = (PlaneArea.ActualHeight - Airplane.ActualHeight) / 3 * 2;
            Canvas.SetTop(Airplane, top);
        }

        private void InitCanvas()
        {
            //DrawArea.Clip = new RectangleGeometry()
            //{
            //    Rect = new Rect(0, 0, This.ActualWidth, This.ActualHeight)
            //};
            DrawArea.Height = This.ActualHeight;
            DrawArea.Width = This.ActualWidth;
            var proj = new PlaneProjection();
            proj.RotationX = -86;
            proj.GlobalOffsetY = 10;
            proj.GlobalOffsetZ = 180;
            DrawAreaBorder.Projection = proj;
        }


        private void InitBackground()
        {
            var path = "ms-appx:///../Assets/MyImages/pattern.png";
            var pathInverted = "ms-appx:///../Assets/MyImages/pattern-inverted.png";
            var width = Math.Round(DrawArea.ActualWidth, MidpointRounding.AwayFromZero);
            backgroundImages = new[]
            {
                new Image() {Source = new BitmapImage(new Uri(path)), Width = width, Height = width},
                new Image() {Source = new BitmapImage(new Uri(pathInverted)), Width = width, Height = width},
                new Image() {Source = new BitmapImage(new Uri(pathInverted)), Width = width, Height = width},
                new Image() {Source = new BitmapImage(new Uri(path)), Width = width, Height = width}
            };
            int i = 0;
            foreach (var backgroundImage in backgroundImages)
            {
                DrawArea.Children.Add(backgroundImage);
                Canvas.SetZIndex(backgroundImage, -99);
                ResetBackground(i);
                i++;
            }
        }

        private void InitSky()
        {
            var path = "ms-appx:///../Assets/MyImages/big-sky.png";
            var areaHeight = SkyDrawArea.ActualHeight;
            var height = Math.Round(areaHeight/2 );
            var width = Math.Round(height/270*1728, MidpointRounding.AwayFromZero);
            skyImages = new[]
            {
                new Image() {Source = new BitmapImage(new Uri(path)), Height = height, Width = width},
                new Image() {Source = new BitmapImage(new Uri(path)), Height = height, Width = width},
            };
            int i = 0;
            foreach (var skyImage in skyImages)
            {
                SkyDrawArea.Children.Add(skyImage);
                Canvas.SetZIndex(skyImage, -90);
                ResetSky(i);
                i++;
            }
        }

        private void UpdateBackground()
        {
            XSpeed = Math.Round(HorizontalSpeed(Roll));
            OnPropertyChanged(nameof(XSpeed));
            foreach (var backgroundImage in backgroundImages)
            {
                var top = Canvas.GetTop(backgroundImage);
                var left = Canvas.GetLeft(backgroundImage);
                var newTop = top + YSpeed;
                var newLeft = left;

                if (Roll > 0)
                {
                    newLeft = left + XSpeed;
                } else if (Roll < 0)
                {
                    newLeft = left - XSpeed;
                }

                PositionInCanvas(backgroundImage, newLeft, newTop);
                CheckBackgroundOutOfBound(backgroundImage);
            }
            foreach (var skyImage in skyImages)
            {
                var left = Canvas.GetLeft(skyImage);
                var newLeft = left;

                if (Roll > 0)
                {
                    newLeft = left + XSpeed;
                }
                else if (Roll < 0)
                {
                    newLeft = left - XSpeed;
                }

                PositionInCanvas(skyImage, newLeft, 0);
                CheckSkyOutOfBound(skyImage);
            }
        }

        private double HorizontalSpeed(double x)
        {
            var max = 15;
            var growthConst = -.025;
            x = Math.Abs(x);

            return max - max*Math.Pow(Math.E, growthConst*x);
        }

        private void CheckBackgroundOutOfBound(Image image)
        {
            var width = DrawArea.ActualWidth;
            var height = DrawArea.ActualHeight;

            var top = Canvas.GetTop(image);
            var left = Canvas.GetLeft(image);

            if (left > width - XSpeed)
            {
                Canvas.SetLeft(image, -width + XSpeed);
            } else if (left < -width + XSpeed)
            {
                Canvas.SetLeft(image, width - XSpeed);
            }

            if (top > width)
            {
                Canvas.SetTop(image, -width);
            } else if (top < -width)
            {
                Canvas.SetTop(image, width);
            }
        }

        private void CheckSkyOutOfBound(Image skyImage)
        {
            var width = skyImage.Width;
            var left = Canvas.GetLeft(skyImage);

            if (left > width - XSpeed)
            {
                Canvas.SetLeft(skyImage, - width + XSpeed);
            } else if (left < - width + XSpeed)
            {
                Canvas.SetLeft(skyImage, width - XSpeed);
            }
        }


        private void ResetBackground(int index)
        {
            double x, y;
            var width = DrawArea.ActualWidth;
            var height = DrawArea.ActualHeight;
            switch (index)
            {
                case 0:
                    x = -width/2;
                    y = -width + height/2;
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
            PositionInCanvas(backgroundImages[index], x, y);
        }

        private void ResetSky(int index)
        {
            double x, y;
            var width = skyImages[index].ActualWidth;
            switch (index)
            {
                case 0:
                    x = 0;
                    y = 0;
                    break;
                case 1:
                    x = width;
                    y = 0;
                    break;
                default:
                    x = 0;
                    y = 0;
                    break;
            }
            PositionInCanvas(skyImages[index], x, y);
        }

        private static void PositionInCanvas(Image element, double x, double y)
        {
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
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
