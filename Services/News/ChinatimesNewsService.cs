using AngleSharp;
using FetchPlatformData.Conditions.News;
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
    public class ChinatimesNewsService : IFetchData<ChinatimesNewsModel, ChinatimesNewsConditions>
    {
        public async Task<IEnumerable<ChinatimesNewsModel>> FetchDataAsync(ChinatimesNewsConditions conditions)
        {
            var result = new List<ChinatimesNewsModel>();
            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;
            do
            {
                var url = $"https://www.chinatimes.com/search/{conditions.Keyword}?page={page}&chdtv";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll(".search-result .title a").Select(x => x.GetAttribute("href"));
                paginationDataTotal = paginationHrefs.Count();
                hrefs.AddRange(paginationHrefs);

                page++;
            } while (paginationDataTotal > 0);


            foreach (var href in hrefs)
            {
                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = document.QuerySelector(".article-title").TextContent;
                var contentItems = document.QuerySelectorAll(".article-body p").Select(x => x.TextContent);
                var content = string.Join('\n', contentItems);

                var model = new ChinatimesNewsModel {
                    Title = title,
                    Content = content
                };

                result.Add(model);
            }

            return result;
        }
    }
}
