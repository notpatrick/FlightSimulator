using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using SensorApp.Classes;

namespace SensorApp {
    [DataContract]
    public class Location {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ImagePath { get; set; }

        public static Location GetRandomLocation() {
            var randomIndex = MyHelpers.Random.Next(LocationDictionary.Count);
            var name = LocationDictionary.Keys.ElementAt(randomIndex);
            var imagePath = LocationDictionary.Values.ElementAt(randomIndex);
            return new Location {
                Name = name,
                ImagePath = imagePath
            };
        }

        public static int Counter = 0;

        public static Location GetNextLocation() {
            var name = LocationDictionary.Keys.ElementAt(Counter);
            var imagePath = LocationDictionary.Values.ElementAt(Counter);
            Counter++;
            if (Counter >= LocationDictionary.Count)
                Counter = 0;
            return new Location {
                Name = name,
                ImagePath = imagePath
            };
        }

        public static Location GetLocationByName(string name)
        {
            if (LocationDictionary.ContainsKey(name))
            {
                var imagePath = LocationDictionary[name];
                return new Location
                {
                    Name = name,
                    ImagePath = imagePath
                };
            }
            Debug.WriteLine("Location not found");
            return GetRandomLocation();
        }

        public static Dictionary<string, string> LocationDictionary = new Dictionary<string, string> {
            {"Munich Airport", "ms-appx:///../Assets/MyImages/Locations/airport-munich.png"},
            {"Frankfurt Airport", "ms-appx:///../Assets/MyImages/Locations/airport-frankfurt.png"},
            {"Paris Airport", "ms-appx:///../Assets/MyImages/Locations/airport-paris.png"},
            {"Rome Airport", "ms-appx:///../Assets/MyImages/Locations/airport-rome.png"},
        };
    }
}