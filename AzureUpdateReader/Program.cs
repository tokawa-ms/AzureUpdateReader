using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Text;

namespace AzureUpdateReader
{
    internal class RSSChanel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string LastBuildDate { get; set; }
    }

    internal class RSSItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string PubDate { get; set; }
        public string Description { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //読み込む対象の RSS フィードの URI
            string rssUri = "https://azurecomcdn.azureedge.net/en-us/updates/feed/";
            string exportCsvPath = @"C:\temp\out.csv";

            try
            {
                //読み込んだ情報の保存先
                RSSChanel rssChannel;
                List<RSSItem> rssItems;
                //RSSフィードを読み込む
                ReadRSSFeed(rssUri, out rssChannel, out rssItems);

                //デバッグ出力用にコンソールに出す
                Print_AllRSSItems(rssChannel, rssItems);
                
                //CSVファイルに出力
                ExportCSV(exportCsvPath, rssChannel, rssItems);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static void ExportCSV(string exportCsvPath, RSSChanel rssChannel, List<RSSItem> rssItems)
        {
            using (StreamWriter sw = new StreamWriter(exportCsvPath, false, Encoding.UTF8))
            {
                sw.WriteLine(rssChannel.Title);
                sw.WriteLine();
                sw.WriteLine("Title,Description,Link,PubDate");
                foreach (RSSItem rssItem in rssItems)
                {
                    sw.WriteLine("\"" + rssItem.Title + "\",\"" + rssItem.Description + "\",\"" +
                        rssItem.Link + "\",\"" + rssItem.PubDate + "\"");
                }
            }
        }

        private static void ReadRSSFeed(string rssUri, out RSSChanel rssChannel, out List<RSSItem> rssItems)
        {
            //RSS フィード読み込み
            XElement element = XElement.Load(rssUri);
            XElement channelElement = element.Element("channel");

            //RSS チャネル読み込み
            rssChannel = new RSSChanel();
            rssChannel.Title = channelElement.Element("title").Value;
            rssChannel.Description = channelElement.Element("description").Value;
            rssChannel.Link = channelElement.Element("link").Value;
            rssChannel.LastBuildDate = channelElement.Element("lastBuildDate").Value;

            //RSS アイテム読み込み
            IEnumerable<XElement> itemElements = channelElement.Elements("item");
            rssItems = new List<RSSItem>();
            foreach (XElement itemElement in itemElements)
            {
                RSSItem rssItem = new RSSItem();
                rssItem.Title = itemElement.Element("title").Value;
                rssItem.Link = itemElement.Element("link").Value;
                rssItem.PubDate = itemElement.Element("pubDate").Value;
                rssItem.Description = itemElement.Element("description").Value;

                rssItems.Add(rssItem);

            }
        }

        private static void Print_AllRSSItems(RSSChanel rssChannel, List<RSSItem> rssItems)
        {
            Console.WriteLine("====");
            Console.WriteLine("RSS Channel Info");
            Console.WriteLine("====");
            Console.WriteLine($"Title         : {rssChannel.Title}");
            Console.WriteLine($"Description   : {rssChannel.Description}");
            Console.WriteLine($"Link          : {rssChannel.Link}");
            Console.WriteLine($"LastBuildDate : {rssChannel.LastBuildDate}");
            Console.WriteLine();

            Console.WriteLine("====");
            Console.WriteLine("RSS Items");
            Console.WriteLine("====");

            foreach (RSSItem rssItem in rssItems)
            {
                Console.WriteLine($"Title         : {rssItem.Title}");
                Console.WriteLine($"Description   : {rssItem.Description}");
                Console.WriteLine($"PubDate       : {rssItem.PubDate}");
                Console.WriteLine($"Link          : {rssItem.Link}");
                Console.WriteLine();
            }
        }
    }
}
