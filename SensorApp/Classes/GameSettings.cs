using System.Runtime.Serialization;

namespace SensorApp {
    [DataContract]
    public class GameSettings {
        public static double SkyCoeffX = .025;
        public static double SkyCoeffY = .0085;
        public static double MountainCoeffX = .055;
        public static double InitialSpeedY = 14;
        public static double InitialSpeedX = 0;
        public static double MinSpeedY = 3;
        public static Angles InitialAngles = new Angles(90, 180, 90);

        [DataMember]
        public double MaxSpeedY { get; set; }

        [DataMember]
        public double MaxSpeedX { get; set; }

        [DataMember]
        public double VerticalTolerance { get; set; }

        [DataMember]
        public bool ShowDebugInfo { get; set; }

        public GameSettings() {
            MaxSpeedY = 20;
            MaxSpeedX = 10;
            VerticalTolerance = 15; // maximum Z angle before SpeedY is altered
            ShowDebugInfo = true;
        }
    }
}