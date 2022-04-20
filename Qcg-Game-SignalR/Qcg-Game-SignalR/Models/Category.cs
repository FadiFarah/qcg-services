using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class Category : BaseEntity
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        [JsonPropertyName("deck")]
        public List<Card> Deck { get; set; }
    }
}
