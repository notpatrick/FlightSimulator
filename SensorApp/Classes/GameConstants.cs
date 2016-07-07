namespace SensorApp {
    public static class GameConstants {
        public const double SkyCoeffX = .025;
        public const double SkyCoeffY = .0085;
        public const double MountainCoeffX = .055;
        public const double InitialSpeedY = 14;
        public const double InitialSpeedX = 0;
        public const double MaxSpeedY = 20;
        public const double MaxSpeedX = 10;
        public const double MinSpeedY = 3;
        public const double VerticalTolerance = 15; // maximum Z angle before SpeedY is altered
        public static readonly Angles InitialAngles = new Angles(90, 180, 90);
    }
}