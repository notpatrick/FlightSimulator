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

        public static async void SaveGameState(GameState state, string fileName) {
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

        public static async Task<GameState> LoadGameState(string fileName) {
            GameState state;
            try {
                var localFolder = ApplicationData.Current.LocalFolder;
                var sampleFile = await localFolder.GetFileAsync(fileName);

                
                using (var ms = await sampleFile.OpenStreamForReadAsync()) {
                    var serializer = new DataContractSerializer(typeof(GameState));
                    state = (GameState) serializer.ReadObject(ms);
                }
                
            }
            catch (Exception e) {
                state = new GameState();
            }
            return state;
        }

        public static async void SaveGameSettings(GameSettings settings) {
            var localFolder = ApplicationData.Current.LocalFolder;
            var sampleFile = await localFolder.CreateFileAsync("GameSettings", CreationCollisionOption.ReplaceExisting);

            var serializer = new DataContractSerializer(typeof(GameSettings));

            using (var s = sampleFile.OpenStreamForWriteAsync().Result) {
                serializer.WriteObject(s, settings);
                s.Flush();
            }
        }

        public static async Task<GameSettings> LoadGameSettings() {
            var localFolder = ApplicationData.Current.LocalFolder;
            var sampleFile = await localFolder.GetFileAsync("GameSettings");

            GameSettings settings;
            using (var ms = await sampleFile.OpenStreamForReadAsync()) {
                var serializer = new DataContractSerializer(typeof(GameSettings));
                settings = (GameSettings) serializer.ReadObject(ms);
            }
            return settings;
        }

        public static async Task<MediaElement> LoadSoundFile(string path, bool infinite = false) {
            var mediaElement = new MediaElement();
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