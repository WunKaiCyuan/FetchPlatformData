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
    /// <summary>
    /// 中國時報
    /// </summary>
    public class ChinatimesNewsService : IFetchData<ChinatimesNewsConditions, Saver>
    {
        public async Task FetchDataAsync(ChinatimesNewsConditions conditions, Saver saver)
        {
            //var result = new List<ChinatimesNewsModel>();

            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            //maper config
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ChinatimesNewsModel, NewsDataModel>();
            });
            var mapper = mapperConfig.CreateMapper();

            //get html content
            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;
            do
            {
                var url = $"https://www.chinatimes.com/search/{conditions.Keyword}?page={page}&chdtv";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll(".articlebox-compact .row .col")
                    .Where(x => DateTime.Parse(x.QuerySelector(".meta-info time").GetAttribute("datetime")) > DateTime.Now.Add(conditions.timeSpan))
                    .Select(x => x.QuerySelector(".title a").GetAttribute("href"));
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

                    var title = document.QuerySelector(".article-title").TextContent;
                    var contentItems = document.QuerySelectorAll(".article-body p").Select(x => x.TextContent);
                    var content = string.Join('\n', contentItems);
                    var postDate = DateTime.Parse(document.QuerySelector(".date").TextContent);
                    var model = new ChinatimesNewsModel
                    {
                        Title = title,
                        Content = content,
                        Date = postDate.ToString("yyyyMMdd"),
                        Source = href
                    };

                    //save result
                    var result = mapper.Map<NewsDataModel>(model);
                    saver.Save(result);
                }
                catch
                {
                }
            }

            return;
        }
    }
}
