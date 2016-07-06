using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
        }

        public static async Task<GameState> Load(string fileName) {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await localFolder.GetFileAsync(fileName);

            GameState state;
            using (var ms = await sampleFile.OpenStreamForReadAsync()) {
                var serializer = new DataContractSerializer(typeof(GameState));
                state = (GameState)serializer.ReadObject(ms);
            }
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
}
