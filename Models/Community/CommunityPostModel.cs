using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Models.Community
{
    public class CommunityPostModel
    {
        /// <summary>
        /// 文章標題
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 本文內容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 來源網址
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 留言
        /// </summary>
        public List<Comment> Replies { get; set; } = new List<Comment>();

    }

    public class Comment
    {
        public string UserID { get; set; }
        public DateTime Date { get; set; }
        public string  CommentText { get; set; } 
    }
}
