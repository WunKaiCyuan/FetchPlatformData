using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    public class TTVNewsService : IFetchData<TTVNewsConditions, Saver>
    {

        public static string PlatfromUrl = $"https://news.ttv.com.tw/";
        public async Task FetchDataAsync(TTVNewsConditions conditions, Saver saver)
        {


            //var result = new List<ChinatimesNewsModel>();

            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            //get html content
            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;
            do
            {
                var url = $"https://news.ttv.com.tw/search/{conditions.Keyword}/{page}";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll("li .clearfix")
                    .Where(x => DateTime.Parse(x.QuerySelector(".time").TextContent) > DateTime.Now.Add(conditions.timeSpan))
                    .Select(x => PlatfromUrl + x.GetAttribute("href"));
                paginationDataTotal = paginationHrefs.Count();

                hrefs.AddRange(paginationHrefs);

                page++;

            } while (paginationDataTotal > 0);

            foreach (var href in hrefs)
            {
                try
                {
                    var responseMessage = await client.GetAsync(href);
                    var responseResult = await responseMessage.Content.ReadAsStringAsync();
                    var document = await context.OpenAsync(res => res.Content(responseResult));

                    var title = document.QuerySelector("h1").TextContent.Replace("\n", string.Empty).Replace(" ", string.Empty);
                    var content = document.QuerySelector("#newscontent").TextContent
                        .Replace("\n", string.Empty).
                        Replace(" ", string.Empty);
                    var postDate = DateTime.Parse(document.QuerySelector(".date.time").TextContent.Replace("\n", string.Empty).Replace(" ", string.Empty));

                    var model = new NewsDataModel
                    {
                        Title = title,
                        Content = content,
                        Date = postDate.ToString("yyyyMMdd"),
                        Source = href
                    };

                    //save result
                    saver.Save(model);
                }
                catch
                {
                }
            }
            return;
        }
    }
}