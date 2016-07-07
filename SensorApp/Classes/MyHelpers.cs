using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace SensorApp.Classes
{
    public static class MyHelpers
    {
        public static async void Save(GameState state, string fileName) {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            var serializer = new DataContractSerializer(typeof(GameState));

            byte[] byteArr;

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, state);
                byteArr = ms.ToArray();
            }

            await FileIO.WriteBytesAsync(sampleFile, byteArr);
            Debug.WriteLine("Save to file done");
        }

        public static async Task<GameState> Load(string fileName) {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.GetFileAsync(fileName);

            GameState state;
            using (var ms = await sampleFile.OpenStreamForReadAsync()) {
                var serializer = new DataContractSerializer(typeof(GameState));
                state = (GameState)serializer.ReadObject(ms);
            }
            Debug.WriteLine("Load from file done");
            return state;
        }

        public static async Task<bool> CheckFile(string fileName) {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            return await localFolder.TryGetItemAsync(fileName) != null;
        }

        public static string GetHardwareId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = DataReader.FromBuffer(hardwareId);

            byte[] bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);

            return BitConverter.ToString(bytes);
        }
    }

    [DataContract]
    public class Angles
    {
        [DataMember]
        public double X { get; set; }
        [DataMember]
        public double Y { get; set; }
        [DataMember]
        public double Z { get; set; }

        public Angles(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Angles CalculateAngles(AccelerometerReading reading)
        {
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
