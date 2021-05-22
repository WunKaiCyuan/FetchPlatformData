using System;
using System.Text.RegularExpressions;

namespace FetchPlatformData.Models.News
{
    public class CteeNewsModel
    {
        public string mmddhhmm { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string sharelink { get; set; }




        /// <summary>
        /// 新聞標題
        /// </summary>
        public string Title { get { return title; } }
        /// <summary>
        /// 新聞內容
        /// </summary>
        public string Content { get { return Regex.Replace(content, "<.*?>", String.Empty); } }
        /// <summary>
        /// 來源
        /// </summary>
        public string Source { get { return sharelink; }  }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get { return mmddhhmm.Substring(0, 6); } }
    }
}
