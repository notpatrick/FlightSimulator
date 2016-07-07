using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using SensorApp.Classes;

namespace SensorApp
{
    [DataContract]
    public class GameState
    {
        [DataMember]
        public Point Position { get; set; }
        [DataMember]
        public double SpeedX { get; set; }
        [DataMember]
        public double SpeedY { get; set; }
        [DataMember]
        public double Score { get; set; }
        [DataMember]
        public Angles Angles { get; set; }

        public GameState() { ResetSpeeds(); }

        public GameState ResetSpeeds() {
            SpeedX = GameConstants.InitialSpeedX;
            SpeedY = GameConstants.InitialSpeedY;
            return this;
        }

        public GameState ResetAngles() {
            Angles = GameConstants.InitialAngles;
            return this;
        }
    }

    public static class GameConstants
    {
        public const double SkyCoeffX = .02;
        public const double SkyCoeffY = .008;
        public const double MountainCoeffX = .05;
        public const double InitialSpeedY = 10;
        public const double InitialSpeedX = 0;
        public const double MaxSpeedY = 17;
        public const double MaxSpeedX = 15;
        public const double VerticalTolerance = 15; // maximum Z angle before SpeedY is altered
        public static readonly Angles InitialAngles = new Angles(90,180,90);
    }
}
