using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class Room : BaseEntity
    {
        [JsonPropertyName("roomName")]
        public string RoomName { get; set; }

        [JsonPropertyName("roomPassword")]
        public string RoomPassword { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        [JsonPropertyName("remainingCards")]
        public List<Card> RemainingCards { get; set; }

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; }

        [JsonPropertyName("isWaiting")]
        public bool IsWaiting { get; set; }

        [JsonPropertyName("isPublic")]
        public bool IsPublic { get; set; }

        [JsonPropertyName("totalPoints")]
        public int TotalPoints { get; set; }
    }
}
