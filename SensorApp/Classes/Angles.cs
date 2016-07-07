using System;
using System.Runtime.Serialization;
using Windows.Devices.Sensors;

namespace SensorApp {
    [DataContract]
    public class Angles {
        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }

        [DataMember]
        public double Z { get; set; }

        public Angles(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        public static Angles CalculateAngles(AccelerometerReading reading) {
            var x = reading.AccelerationX;
            var y = reading.AccelerationY;
            var z = reading.AccelerationZ;

            var radius = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            var arx = Math.Acos(x / radius) * 180 / Math.PI - 90; // -90 so 0 is middle
            var ary = Math.Acos(y / radius) * 180 / Math.PI;
            var arz = Math.Acos(z / radius) * 180 / Math.PI - 90; // -90 so 0 is middle

            return new Angles(arx, ary, arz);
        }
    }
}