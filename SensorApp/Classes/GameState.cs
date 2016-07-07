using System.Runtime.Serialization;
using Windows.Foundation;

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

        public GameState GetNextLocation() {
            Location = Location.GetNextLocation();
            return this;
        }
    }
}