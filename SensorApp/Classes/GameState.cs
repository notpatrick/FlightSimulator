using System.ComponentModel;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace SensorApp {
    [DataContract]
    public class GameState {
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

        [DataMember]
        public Location Location { get; set; }

        [DataMember]
        public bool IsRunning { get; set; }

        public GameState() {
            IsRunning = true;
            Score = 0;
            Position = new Point(0, 0);
            Location = Location.GetRandomLocation();
            Angles = GameSettings.InitialAngles;
            ResetSpeeds();
        }

        public GameState ResetSpeeds() {
            SpeedX = GameSettings.InitialSpeedX;
            SpeedY = GameSettings.InitialSpeedY;
            return this;
        }

        public GameState ResetAngles() {
            Angles = GameSettings.InitialAngles;
            return this;
        }

        public GameState GetNextLocation() {
            Location = Location.GetNextLocation();
            return this;
        }
    }
}