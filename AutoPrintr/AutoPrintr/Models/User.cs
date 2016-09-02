﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoPrintr.Models
{
    public class User
    {
        [JsonProperty("user_token")]
        public string Token { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("subdomain")]
        public string Subdomain { get; set; }

        [JsonProperty("default_location")]
        public int? DefaulLocationId { get; set; }

        [JsonProperty("locations_allowed")]
        public IEnumerable<Location> Locations { get; set; }
    }
}