using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.ApiReturn;
using FetchPlatformData.Models.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace FetchPlatformData.Services.News
{
    public class CteeNewsService : IFetchData<CteeNewsConditions, Saver>
    {
        public async Task FetchDataAsync(CteeNewsConditions conditions, Saver saver)
        {

            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            //maper config
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CteeNewsModel, NewsDataModel>();
            });

            var mapper = mapperConfig.CreateMapper();

            var NewsDataModelList = new List<NewsDataModel>();

            var ApiUrl = $"https://search.ctee.com.tw/multiindexsearch/{conditions.Keyword}";
            var responseMessage = await client.GetAsync(ApiUrl);
            var responseResult = await responseMessage.Content.ReadAsStringAsync();

            var newsRespond = JsonConvert.DeserializeObject<List<CteeNewsModel>>(responseResult);

            newsRespond.ForEach(e =>
            {
                var result = mapper.Map<NewsDataModel>(e);
                saver.Save(result);
            });
        }
    }
}
