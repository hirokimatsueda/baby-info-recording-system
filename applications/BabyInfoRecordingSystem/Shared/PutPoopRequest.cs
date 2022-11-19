using System;

namespace BlazorApp.Shared
{
    public class PutPoopRequest
    {
        /// <summary>
        /// 子供のID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// うんちをした日付
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// うんちをした回数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// IDの生成
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            return $"{UserId}-{Date:yyyyMMdd}";
        }
    }
}
