using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class Player
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("isWin")]
        public bool IsWin { get; set; }

        [JsonPropertyName("cards")]
        public List<Card> Cards { get; set; }

        [JsonPropertyName("isReady")]
        public bool IsReady { get; set; }

        [JsonPropertyName("isMaster")]
        public bool IsMaster { get; set; }

        [JsonPropertyName("isTurn")]
        public bool IsTurn { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }

        [JsonPropertyName("isDonePlaying")]
        public bool IsDonePlaying { get; set; }
    }
}
