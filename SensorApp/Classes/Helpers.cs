using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using SensorApp.Annotations;

namespace SensorApp {
    public class Angles {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Angles(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}