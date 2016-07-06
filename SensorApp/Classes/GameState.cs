using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace SensorApp
{
    [DataContract]
    public class GameState
    {
        [DataMember]
        public Point Position { get; set; }
        [DataMember]
        public double XSpeed { get; set; }
        [DataMember]
        public double YSpeed { get; set; }
        [DataMember]
        public bool IsRunning { get; set; }
        [DataMember]
        public double Score { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Roll { get; set; }
        [DataMember]
        public double Yaw { get; set; }

        public GameState()
        {
            XSpeed = GameConstants.InitialSpeedX;
            YSpeed = GameConstants.InitialSpeedY;
            IsRunning = true;
        }
    }

    public static class GameConstants
    {
        public const double SkyCoeffX = .02;
        public const double SkyCoeffY = .008;
        public const double MountainCoeffX = .05;
        public const double InitialSpeedY = 17;
        public const double InitialSpeedX = 0;
    }
}
