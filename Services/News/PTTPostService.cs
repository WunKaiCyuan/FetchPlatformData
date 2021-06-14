using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.Community;
using FetchPlatformData.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    public class PTTPostService : IFetchData<NewsConditions, Saver>
    {
        public static string platFromUrl = $"https://www.ptt.cc";

        public async Task FetchDataAsync(NewsConditions conditions, Saver saver)
        {
            var client = new HttpClient();
            //R18警告
            client.DefaultRequestHeaders.Add("cookie", "over18=1");

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            //
            //接續
            saver.CommunityCount = 0;
            var contiunefrombord = "";
            var skiptoBordName = "";
            bool skipbordto = skiptoBordName!="";

            //ptt
            //抓各版


            var HotboardsUrl = $"{platFromUrl}/bbs/index.html";
            var document = await getDocument(HotboardsUrl);
            var bordsHrefs = document.QuerySelectorAll(".b-ent a").Select(x => x.GetAttribute("href")).ToList();

            foreach (var bordHref in bordsHrefs)
            {
                //接續用
                if (!bordHref.Contains(skiptoBordName) && skipbordto)
                    continue;
                else
                    skipbordto = false;



                //下頁
                var nextPagedUrl = $"{platFromUrl}{bordHref}";

                var keepSearch = true;
                //內文連結
                var postUrls = new List<string>();

                GassDateTime gassDateTime = new GassDateTime();

                do
                {
                    //接續 看板分頁入口
                    if (contiunefrombord!="")
                    {
                        nextPagedUrl = contiunefrombord;
                        contiunefrombord = "";
                    }
                    //頁面
                    var bordpage = await getDocument($"{nextPagedUrl}");

                    nextPagedUrl = platFromUrl + bordpage.QuerySelectorAll("a.btn.wide").Where(x => x.TextContent == "‹ 上頁").First().GetAttribute("href");
                    //已刪除文章
                    postUrls = bordpage.QuerySelectorAll(".r-ent")
                        .Where(x => x.QuerySelector(".title a") != null)
                        .Where(x => x.QuerySelector(".title a").HasAttribute("href"))
                        .Where(x => gassDateTime.JustMMDD(x.QuerySelector(".meta .date").TextContent) >= DateTime.Now.Add(conditions.timeSpan))
                        .Select(x => x.QuerySelector(".title a").GetAttribute("href")).ToList();
                    var ProcrossUrl = "";
                    foreach (var url in postUrls)
                    {
                        try
                        {
                            ProcrossUrl = url;
                            #region 標題資料
                            //var pageContent = await getDocument($"https://www.ptt.cc/bbs/Gossiping/M.1623301877.A.A6A.html");
                            //特殊狀況 
                            //https://www.ptt.cc/bbs/Gossiping/M.1623141975.A.037.html
                            //https://www.ptt.cc/bbs/Gossiping/M.1623224070.A.3A7.html

                            var pageContent = await getDocument($"{platFromUrl}{url}");
                            if (pageContent.StatusCode != System.Net.HttpStatusCode.OK)
                                continue;
                            var titles = pageContent.QuerySelectorAll(".article-metaline");

                            //作者
                            var a = titles.Where(x => x.QuerySelector(".article-meta-tag").TextContent == "作者")
                                .Select(x => x.QuerySelector(".article-meta-value").TextContent).First()
                                .Split('(')[0].TrimEnd();//刪除暱稱 字尾

                            //標題
                            var t = titles.Where(x => x.QuerySelector(".article-meta-tag").TextContent == "標題")
                                .Select(x => x.QuerySelector(".article-meta-value").TextContent).First();//刪除暱稱 字尾

                            //日期
                            var d = titles.Where(x => x.QuerySelector(".article-meta-tag").TextContent == "時間")
                                .Select(x => x.QuerySelector(".article-meta-value").TextContent).First();//刪除暱稱 字尾
                            var dv = d.Replace("  ", " ").Split(' ', ':');
                            var df = DateTime.Parse(dv[1] + " " + dv[2] + " " + dv[6]);


                            #endregion

                            #region 推文
                            //格式驗證
                            var pushs = pageContent.QuerySelectorAll(".push")
                                .Where(x => x.QuerySelector(".push-userid") != null)
                                .Where(x => x.QuerySelector(".push-tag") != null)
                                .Where(x => x.QuerySelector(".push-content") != null)
                                .Where(x => x.QuerySelector(".push-ipdatetime") != null)
                                .ToList();
                            var replies = new List<Comment>();
                            foreach (var push in pushs)
                            {
                                var uid = push.QuerySelectorAll(".push-userid").First().TextContent;
                                var tag = push.QuerySelectorAll(".push-tag").First().TextContent;
                                var content = push.QuerySelectorAll(".push-content").First().TextContent.Trim(':');
                                var ipdatetime = push.QuerySelectorAll(".push-ipdatetime").First().TextContent.Trim(new char[] { ' ', '\n' }).Split(' ');
                                if (ipdatetime.Length < 2)
                                    continue;
                                var commentDatetime = gassDateTime.InPostFullTime($"{ipdatetime[ipdatetime.Length - 2]} {ipdatetime[ipdatetime.Length - 1]}");

                                var comment = new Comment()
                                {
                                    CommentText = content,
                                    Date = commentDatetime,
                                    UserID = uid
                                };

                                replies.Add(comment);
                            }
                            #endregion

                            #region 內文處理
                            var maincontent = pageContent.QuerySelector("#main-content");
                            foreach (var delnode in titles)
                            {
                                maincontent.RemoveChild(delnode);
                            };
                            foreach (var delnode in maincontent.QuerySelectorAll(".richcontent"))
                            {
                                maincontent.RemoveChild(delnode);
                            }
                            foreach (var delnode in maincontent.QuerySelectorAll(".article-metaline-right"))
                            {
                                maincontent.RemoveChild(delnode);
                            }
                            foreach (var delnode in maincontent.QuerySelectorAll(".push"))
                            {
                                maincontent.RemoveChild(delnode);
                            }
                            maincontent.RemoveChild(maincontent.LastChild);
                            maincontent.RemoveChild(maincontent.LastChild);
                            //--
                            //刪簽名檔  
                            var patt = @"--\\n.*\\n--\\n";
                            var maincontentText = Regex.Replace(maincontent.TextContent, patt, "");
                            //var maincontentText = maincontent.TextContent;
                            #endregion

                            var psot = new CommunityPostModel
                            {
                                Source = $"{platFromUrl}{url}",
                                Title = t,
                                Date = df.ToString("yyyyMMdd"),
                                Content = maincontentText,
                                Replies = replies
                            };

                            saver.Save(psot);
                        }
                        catch (Exception ex)
                        {
                            List<string> args = new List<string>();
                            args.Add($"Bordpage={bordpage}");
                            args.Add($"ProcrossUrl={ProcrossUrl}");
                            saver.ErrorLog(ex, args);
                            continue;
                        }
                    }
                    keepSearch = postUrls.Count() > 0;
                    postUrls.Clear();

                } while (keepSearch);
            }
            return;
        }
        public async Task<AngleSharp.Dom.IDocument> getDocument(string URL)
        {
            Console.WriteLine($"\n[{DateTime.Now.ToLongTimeString()}]Loaging Page :{URL}");
            var client = new HttpClient();
            //R18警告
            client.DefaultRequestHeaders.Add("cookie", "over18=1");

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var Response = await client.GetAsync(URL);
            var Result = await Response.Content.ReadAsStringAsync();
            AngleSharp.Dom.IDocument document = await context.OpenAsync(res => res.Content(Result));
            return document;
        }

    }
}
