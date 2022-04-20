using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class Room : BaseEntity
    {
        [JsonPropertyName("roomMaster")]
        public User RoomMaster { get; set; }

        [JsonPropertyName("roomName")]
        public string RoomName { get; set; }

        [JsonPropertyName("roomPassword")]
        public string RoomPassword { get; set; }

        [JsonPropertyName("category")]
        public Category Category { get; set; }

        [JsonPropertyName("currentUsers")]
        public List<User> CurrentUsers { get; set; }

        [JsonPropertyName("isWaiting")]
        public bool IsWaiting { get; set; }

        [JsonPropertyName("isPublic")]
        public bool IsPublic { get; set; }
    }
}
