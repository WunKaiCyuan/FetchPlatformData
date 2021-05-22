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
        public int Count { get; set; } = 0;
        public string keyword = "";
        private DirectoryInfo DataDirectoryInfo { get; set; }
        public string FileLocation { get { return DataDirectoryInfo.FullName; } }

        private Saver(string keyword)
        {
            this.keyword = keyword;
            this.Count = 0;
            var dirName = $"{keyword}_{DateTime.Now.ToString("yyyyMMddhhmm")}";
            DataDirectoryInfo = Directory.CreateDirectory(dirName);
        }

        public static Saver getSaver(string keyword)
        {
            if (saver == null)
                saver = new Saver(keyword);
            return saver;
        }

        public static Saver getSaver()
        {
            return saver;
        }

        public void Save(NewsDataModel post)
        {
            string fileName = $"/{ Count.ToString("D8")}.json";
            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]save to: {DataDirectoryInfo.FullName}{fileName}");

            using (var writer = new StreamWriter(DataDirectoryInfo.FullName + fileName, false, System.Text.Encoding.UTF8))
            {
                string jsonString = JsonConvert.SerializeObject(post);
                writer.WriteLine(jsonString);
                writer.Close();
            }
            Count++;
        }
    }
}




