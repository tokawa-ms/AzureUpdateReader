using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Transactions;

namespace AzureUpdateReader
{
    internal class RSSChannel
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

    /// <summary>
    /// The C# classes that represents the JSON returned by the Translator Text API.
    /// </summary>
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }

    class Program
    {
        private const string region_var = "TRANSLATOR_SERVICE_REGION";
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);

        private const string key_var = "TRANSLATOR_TEXT_RESOURCE_KEY";
        private static readonly string resourceKey = Environment.GetEnvironmentVariable(key_var);

        private const string endpoint_var = "TRANSLATOR_TEXT_ENDPOINT";
        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);

        private static string route = "/translate?api-version=3.0&to=ja";

        static async Task Main(string[] args)
        {
            //読み込む対象の RSS フィードの URI
            string rssUri = "https://azurecomcdn.azureedge.net/en-us/updates/feed/?category=devops%2Cweb%2Cmedia%2Cdeveloper-tools";
            string exportCsvPath = @"C:\temp\out.csv";

            try
            {
                //読み込んだ情報の保存先
                RSSChannel rssChannel;
                List<RSSItem> rssItems;
                //RSSフィードを読み込む
                ReadRSSFeed(rssUri, out rssChannel, out rssItems);

                //デバッグ出力用にコンソールに出す
                Print_AllRSSItems(rssChannel, rssItems);
                
                //CSVファイルに出力
                await ExportCSV(exportCsvPath, rssChannel, rssItems);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private static string ParsePubDate(string pubDate)
        {
            DateTime datetime;
            if (DateTime.TryParse(pubDate, out datetime) == true)
            {
                return datetime.ToString("yyyy/MM/dd");
            }
            else
            {
                return "";
            }
        }

        private static async Task ExportCSV(string exportCsvPath, RSSChannel rssChannel, List<RSSItem> rssItems)
        {
            using (StreamWriter sw = new StreamWriter(exportCsvPath, false, Encoding.UTF8))
            {
                sw.WriteLine(rssChannel.Title);
                sw.WriteLine();
                if (region != null && resourceKey != null && endpoint != null && route != null)
                {
                    sw.WriteLine("Title,Description,Original_Title,Original_Description,Link,PubDate");
                }
                else
                {
                    sw.WriteLine("Title,Description,Link,PubDate");
                }

                foreach (RSSItem rssItem in rssItems)
                {
                    if (region != null && resourceKey != null && endpoint != null && route != null)
                    {
                        var jptitle = await TranslateTextRequest(resourceKey, endpoint, route, rssItem.Title);
                        var jpdesc = await TranslateTextRequest(resourceKey, endpoint, route, rssItem.Description);
                        sw.WriteLine("\"" + jptitle + "\",\"" + jpdesc + "\",\"" +
                            rssItem.Title + "\",\"" + rssItem.Description + "\",\"" +
                            rssItem.Link + "\",\"" + ParsePubDate(rssItem.PubDate) + "\"");
                    }
                    else
                    {
                        sw.WriteLine("\"" + rssItem.Title + "\",\"" + rssItem.Description + "\",\"" +
                            rssItem.Link + "\",\"" + ParsePubDate(rssItem.PubDate) + "\"");
                    }
                }
            }
        }

        private static void ReadRSSFeed(string rssUri, out RSSChannel rssChannel, out List<RSSItem> rssItems)
        {
            //RSS フィード読み込み
            XElement element = XElement.Load(rssUri);
            XElement channelElement = element.Element("channel");

            //RSS チャネル読み込み
            rssChannel = new RSSChannel();
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

        private static void Print_AllRSSItems(RSSChannel rssChannel, List<RSSItem> rssItems)
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

        // Async call to the Translator Text API
        static public async Task<string> TranslateTextRequest(string resourceKey, string endpoint, string route, string inputText)
        {
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);
            string output = "";

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", resourceKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                // Iterate over the deserialized results.
                foreach (TranslationResult o in deserializedOutput)
                {
                    // Print the detected input language and confidence score.
                    Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
                    // Iterate over the results and print each translation.
                    foreach (Translation t in o.Translations)
                    {
                        Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                        output = t.Text;
                    }
                }
            }
            return output;
        }
    }
}
