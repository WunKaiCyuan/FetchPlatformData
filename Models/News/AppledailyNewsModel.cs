using System;

namespace FetchPlatformData.Models.News
{
    public class AppledailyNewsModel
    {
        /// <summary>
        /// 新聞標題
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 新聞內容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 來源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }
    }
}
