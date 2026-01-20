using System;
using System.Net.Http;
using System.Net.Http.Json;
using UsFrameApp.Constants;
using UsFrameApp.Models;

namespace UsFrameApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService()
        {
            // shared http client with base api url
            _http = new HttpClient
            {
                BaseAddress = new Uri(ApiEndpoints.BaseUrl)
            };
        }

        // create a new room on server
        public async Task<RoomModel?> CreateRoomAsync()
        {
            var response = await _http.PostAsync(ApiEndpoints.CreateRoom, null);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<RoomModel>();
        }

        // mark user joined in room
        public async Task<bool> JoinRoomAsync(Guid roomId)
        {
            var url = string.Format(ApiEndpoints.JoinRoom, roomId);
            var response = await _http.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }

        // close room on server
        public async Task CloseRoomAsync(Guid roomId)
        {
            var url = string.Format(ApiEndpoints.CloseRoom, roomId);
            await _http.PostAsync(url, null);
        }

        // fetch room details using room key
        public async Task<RoomModel?> GetRoomByKeyAsync(string roomKey)
        {
            var response = await _http.GetAsync($"usframe/api/rooms/{roomKey}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<RoomModel>();
        }
    }
}
