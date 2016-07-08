using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Xaml.Controls;

namespace SensorApp.Classes {
    public static class MyHelpers {
        public static Random Random = new Random();

        public static async void Save(GameState state, string fileName) {
            var localFolder = ApplicationData.Current.LocalFolder;
            var sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            var serializer = new DataContractSerializer(typeof(GameState));

            byte[] byteArr;

            using (var ms = new MemoryStream()) {
                serializer.WriteObject(ms, state);
                byteArr = ms.ToArray();
            }

            await FileIO.WriteBytesAsync(sampleFile, byteArr);
        }

        public static async Task<GameState> Load(string fileName) {
            var localFolder = ApplicationData.Current.LocalFolder;
            var sampleFile = await localFolder.GetFileAsync(fileName);

            GameState state;
            using (var ms = await sampleFile.OpenStreamForReadAsync()) {
                var serializer = new DataContractSerializer(typeof(GameState));
                state = (GameState) serializer.ReadObject(ms);
            }
            return state;
        }

        public static async Task<bool> CheckFile(string fileName) {
            var localFolder = ApplicationData.Current.LocalFolder;
            return await localFolder.TryGetItemAsync(fileName) != null;
        }


        public static async Task<MediaElement> LoadSoundFile(string path, bool infinite = false)
        {
            var mediaElement = new MediaElement();
            if (infinite)
                mediaElement.MediaEnded += (sender, args) => { mediaElement.Play(); };
            mediaElement.MediaFailed += (sender, e) => { Debug.WriteLine($"Media_MediaFailed({e.ErrorMessage})"); };
            var audioFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(path);
            var audio = await audioFile.OpenAsync(FileAccessMode.Read);
            mediaElement.SetSource(audio, audioFile.FileType);
            return mediaElement;
        }

        public static string GetHardwareId() {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = DataReader.FromBuffer(hardwareId);

            var bytes = new byte[hardwareId.Length];
            dataReader.ReadBytes(bytes);

            return BitConverter.ToString(bytes);
        }
    }
}