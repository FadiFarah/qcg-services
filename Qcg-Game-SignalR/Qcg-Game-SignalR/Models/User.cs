using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Qcg_Game_SignalR.Models
{
    class User : BaseEntity
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        [JsonPropertyName("sub")]
        public string Sub { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
