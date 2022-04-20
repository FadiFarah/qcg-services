using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class Card : BaseEntity
    {
        [JsonPropertyName("imageURL")]
        public string ImageURL { get; set; }

        [JsonPropertyName("cardName")]
        public string CardName { get; set; }

        [JsonPropertyName("categoryGroup")]
        public string CategoryGroup { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
