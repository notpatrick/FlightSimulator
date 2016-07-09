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
        public static double MaxSpeedY = 20;
        public static double MaxSpeedX = 10;
        public static double VerticalTolerance = 15; // maximum Z angle before SpeedY is altered
        public static Angles InitialAngles = new Angles(90, 180, 90);

        [DataMember]
        public bool ShowDebugInfo { get; set; }

        [DataMember]
        public bool SoundMuted { get; set; }

        public GameSettings() {
            ShowDebugInfo = true;
            SoundMuted = false;
        }

        public GameSettings(GameSettings settings) {
            ShowDebugInfo = settings.ShowDebugInfo;
            SoundMuted = settings.SoundMuted;
        }
    }
}