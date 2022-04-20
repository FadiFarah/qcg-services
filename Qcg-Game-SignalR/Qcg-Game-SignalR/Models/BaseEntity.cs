using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class BaseEntity
    {
        [JsonPropertyName("_id")]
        public string _id { get; set; }
    }
}
