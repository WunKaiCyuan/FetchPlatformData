using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Models.ApiReturn
{
    public class NownewsSearchResult
    {
        public string msg { get; set; }
        public int code { get; set; }
        public List<NewsPost> data { get; set; } = new List<NewsPost>();


        public class NewsPost
        {
            public string id { get; set; }// "id": "5272188",
            public string postTitle { get; set; }//"postTitle": "<b>疫情</b>嚴峻！蔡英文520未公開談話　一早視察軍隊防疫",
            public string postUrl { get; set; }// "postUrl": "/news/news-summary/politics/5272188",
            public string postOnlyUrl { get; set; }//"postOnlyUrl": "/news/5272188",
            public string newsDate { get; set; }// "newsDate": "2021-05-20",
            public int sort { get; set; }//"sort": 1,
            public string postContent { get; set; }//"postContent": "今（20）日是總統蔡英文連任就職滿週年，本土疫情升溫，據了解，蔡英文今不會針對520特別發表談話，但一早10點她將到國防部視察，掌握國軍防疫工作進度。總統府發言人張惇涵表示，由於近期國內疫情嚴峻，全國",
            public string imageUrl { get; set; }//"imageUrl": "https://media.nownews.com/nn_media/thumbnail/2021/02/1612174159818-e5410415cb6b4682a2ffdc5f6cac7f46-800x436.png?unShow=false"

            public DateTime getDate
            {
                get
                {
                    return DateTime.Parse(newsDate);
                }
            }

        }
    }
}
