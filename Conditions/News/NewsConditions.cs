using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Conditions.News
{
    public class NewsConditions
    {
        //關鍵字
        public string Keyword { get; set; }

        //時間範圍
        public TimeSpan timeSpan { get; set; }
    }
}
