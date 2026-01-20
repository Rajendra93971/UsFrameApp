using System;
using System.Text.Json.Serialization;

namespace UsFrameApp.Models
{
    // room data received from backend api
    public class RoomModel
    {
        // unique room id
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        // public room key used for join
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        // number of active connections
        [JsonPropertyName("totalConnections")]
        public int TotalConnections { get; set; }

        // room closed state
        [JsonPropertyName("isClosed")]
        public bool IsClosed { get; set; }
    }
}
