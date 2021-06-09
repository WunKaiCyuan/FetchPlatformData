using FetchPlatformData.Models.Community;
using FetchPlatformData.Models.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Models
{
    public class Saver
    {
        private static Saver saver;
        public int NewsCount { get; set; } = 0;
        public int CommunityCount { get; set; } = 0;
        public string keyword = "";
        private DirectoryInfo NewsDirectoryInfo { get; set; }

        private DirectoryInfo CommunityDirectoryInfo { get; set; }
        public string FileLocation { get { return NewsDirectoryInfo.Parent.FullName; } }

        private Saver(string keyword)
        {
            this.keyword = keyword;
            this.NewsCount = 0;
            this.CommunityCount = 0;
            NewsDirectoryInfo = Directory.CreateDirectory($"{keyword}_News_{DateTime.Now.ToString("yyyyMMddhhmm")}");
            CommunityDirectoryInfo = Directory.CreateDirectory($"{keyword}_Community_{DateTime.Now.ToString("yyyyMMddhhmm")}");
        }

        public static Saver getSaver(string keyword)
        {
            if (saver == null)
                saver = new Saver(keyword);
            return saver;
        }

        public void Save(NewsDataModel post)
        {
            string fileName = $"/{ NewsCount.ToString("D8")}.json";
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]save to: {NewsDirectoryInfo.FullName}{fileName}");

            using (var writer = new StreamWriter(NewsDirectoryInfo.FullName + fileName, false, System.Text.Encoding.UTF8))
            {
                string jsonString = JsonConvert.SerializeObject(post);
                writer.WriteLine(jsonString);
                writer.Close();
            }
            NewsCount++;
        }

        public void Save(CommunityPostModel post)
        {
            string fileName = $"/{ CommunityCount.ToString("D8")}.json";
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]save to: {CommunityDirectoryInfo.FullName}{fileName}");

            using (var writer = new StreamWriter(CommunityDirectoryInfo.FullName + fileName, false, System.Text.Encoding.UTF8))
            {
                string jsonString = JsonConvert.SerializeObject(post);
                writer.WriteLine(jsonString);
                writer.Close();
            }
            CommunityCount++;
        }
    }
}




