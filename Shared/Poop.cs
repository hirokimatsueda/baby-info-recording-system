using Newtonsoft.Json;
using System;

namespace BlazorApp.Shared
{
    /// <summary>
    /// Cosmos DB上のうんち表現
    /// </summary>
    public class Poop
    {
        /// <summary>
        /// データのID ( = UserId + Date)
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 子供のID ( = パーティションキー)
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// うんちをした日付
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// うんちをした回数
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
