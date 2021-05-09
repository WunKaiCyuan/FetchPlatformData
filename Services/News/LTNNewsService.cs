using AngleSharp;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models.News;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    /// <summary>
    /// 自由時報
    /// </summary>
    public class LTNNewsService : IFetchData<LTNNewsModel, LTNNewsConditions>
    {
        public async Task<IEnumerable<LTNNewsModel>> FetchDataAsync(LTNNewsConditions conditions)
        {
            var result = new List<LTNNewsModel>();
            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;
            do
            {
                var url = $"https://search.ltn.com.tw/list?keyword={conditions.Keyword}&type=all&page={page}";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll(".Searchnews .tit").Select(x => x.GetAttribute("href"));
                paginationDataTotal = paginationHrefs.Count();
                hrefs.AddRange(paginationHrefs);

                page++;
            } while (paginationDataTotal > 0);


            foreach (var href in hrefs)
            {
                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = document.QuerySelector(".content h1").TextContent;
                var contentItems = document.QuerySelectorAll(".content .text p").Where(x=>!x.HasAttribute("style") && !x.HasAttribute("class")).Select(x => x.TextContent);
                var content = string.Join('\n', contentItems);

                var model = new LTNNewsModel
                {
                    Title = title,
                    Content = content
                };

                result.Add(model);
            }

            return result;
        }
    }
}
