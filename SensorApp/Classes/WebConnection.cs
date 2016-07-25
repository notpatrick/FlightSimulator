using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SensorApp;

namespace DatabaseClient
{
    public class WebConnection
    {
        public static string ServerUrl = "http://localhost:1337";

        #region UserRoutes

        public static async Task CreateNewUser(User newUser)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(ServerUrl + "/user", UserToContent(newUser));
            }
        }

        public static async Task DeleteUser(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(ServerUrl + "/user/" + id);
            }
        }

        public static async Task<List<User>> GetAllUsers()
        {
            List<User> result;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ServerUrl + "/user");
                response.EnsureSuccessStatusCode();

                var x = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<List<User>>(x);
            }
            return result;
        }

        #endregion

        #region GameStateRoutes

        public static async Task<List<ServerGameState>> GetAllGameStates()
        {
            List<ServerGameState> result;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ServerUrl + "/state");
                response.EnsureSuccessStatusCode();

                var x = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<List<ServerGameState>>(x);
            }
            return result;
        }

        public static async Task<ServerGameState> GetGameStateOfUser(int userID, int stateID = 0)
        {
            ServerGameState result;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(ServerUrl + "/user/"+userID+"/state");
                response.EnsureSuccessStatusCode();

                var x = await response.Content.ReadAsStringAsync();
                var allStates = JsonConvert.DeserializeObject<List<ServerGameState>>(x);
                
                result = allStates[allStates.Count -1];
            }
            return result;
        }

        public static async Task CreateNewGameState(ServerGameState state)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(ServerUrl + "/state", GameStateToContent(state));
            }
        }

        public static async Task DeleteGameState(int id)
        {
            using (var client = new HttpClient())
            {
                var response = await client.DeleteAsync(ServerUrl + "/state/" + id);
            }
        }

        #endregion

        #region Helpers

        public static FormUrlEncodedContent UserToContent(User user)
        {
            var pairs = new Dictionary<string, string>
            {
                {"name", user.Name},
                {"password", user.Password}
            };

            return new FormUrlEncodedContent(pairs);
        }

        public static FormUrlEncodedContent GameStateToContent(ServerGameState state)
        {
            var pairs = new Dictionary<string, string>
            {
                {"userID", state.UserId.ToString()},
                {"score", state.Score.ToString()},
                {"isRunning", state.IsRunning ? "1" : "0"},
                {"locationName", state.LocationName},
                {"positionX", state.PositionX.ToString()},
                {"positionY", state.PositionY.ToString()},
                {"speedX", state.SpeedX.ToString()},
                {"speedY", state.SpeedY.ToString()},
            };

            return new FormUrlEncodedContent(pairs);
        }

        #endregion
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public User(int id, string name, string password)
        {
            this.Id = id;
            this.Name = name;
            this.Password = password;
        }
    }

    public class ServerGameState
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Score { get; set; }
        public bool IsRunning { get; set; }
        public string LocationName { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }

        public ServerGameState()
        {
            
        }

        public ServerGameState(GameState state)
        {
            IsRunning = state.IsRunning;
            Score = state.Score;
            PositionX = state.Position.X;
            PositionY = state.Position.Y;
            SpeedY = state.SpeedY;
            SpeedX = state.SpeedX;
            LocationName = state.Location.Name;
        }
    }
}