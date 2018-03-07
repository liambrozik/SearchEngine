using ClassLibrary1;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WorkerRole1
{
    class Crawler
    {
        public static bool crawling = false;
        private static List<string> CrawledURLs = new List<string>();
        private static List<string> Whitelist = new List<string>();
        private static List<string> Blacklist = new List<string>();
        private static List<string> stopwords = new List<string>();
        private static List<string> stopwordstitle = new List<string>();
        private static bool stopwordcreated = false;
        private static bool processingXML = false;
        public static void StartCrawl()
        {
            while (crawling)
            {
                CloudQueueMessage message = DBConnect.GetLinkQueue().GetMessage(TimeSpan.FromMinutes(5));
                System.Diagnostics.Debug.WriteLine("Checking LinkQueue...");
                if (message != null)
                {
                    if (message.AsString.Contains("robots.txt"))
                    {

                        ProcessRobots(message.AsString);

                    }
                    else if (message.AsString.Contains(".xml"))
                    {
                        CloudQueueMessage newXML = new CloudQueueMessage(message.AsString);
                        DBConnect.GetXMLQueue().AddMessage(newXML);
                        processingXML = true;
                        System.Diagnostics.Debug.WriteLine("Added to XML Queue: " + message.AsString);
                    }
                    else if (message.AsString.Contains(".html") || message.AsString.Contains(".html"))
                    {
                        CreateStopWords();
                        ProcessHTML(message.AsString);
                    }
                    {
                        System.Diagnostics.Debug.WriteLine("Not a robots.txt");
                    }
                    DBConnect.GetLinkQueue().DeleteMessage(message);
                }
                if (processingXML)
                {
                    ProcessXMLQueue();
                }


                CloudQueueMessage smessage = DBConnect.GetStopStartQueue().GetMessage(TimeSpan.FromMinutes(5));
                if (smessage != null)
                {
                    if (smessage.AsString == "stop")
                    {
                        crawling = false;
                        Task.Run(() => UpdateStats("Stopped"));
                    }
                    DBConnect.GetStopStartQueue().DeleteMessage(smessage);
                }
                Thread.Sleep(100);
            }

        }

        private static void CreateStopWords()
        {
            if (!stopwordcreated)
            {
                using (StreamReader sr = new StreamReader("stopwords.txt"))
                {
                    string ln;
                    while ((ln = sr.ReadLine()) != null)
                    {
                        if (!stopwords.Contains(ln))
                        {
                            stopwords.Add(ln);
                        }
                    }
                }
                using (StreamReader sr = new StreamReader("stopwordstitle.txt"))
                {
                    string ln;
                    while ((ln = sr.ReadLine()) != null)
                    {
                        if (!stopwordstitle.Contains(ln))
                        {
                            stopwordstitle.Add(ln);
                        }
                    }
                }
                stopwordcreated = true;
            }
        }
        private static void ProcessRobots(string url)
        {
            WebClient webClient = new WebClient();
            Stream robots = webClient.OpenRead(url);
            using (StreamReader RobotsReader = new StreamReader(robots))
            {
                string line;
                while ((line = RobotsReader.ReadLine()) != null)
                {
                    if (line.ToLower().StartsWith("sitemap"))
                    {
                        string NewURL = line.Substring(8);
                        if (NewURL.StartsWith(" "))
                        {
                            NewURL = NewURL.Substring(1);
                        }
                        CloudQueueMessage newXML = new CloudQueueMessage(NewURL);
                        DBConnect.GetXMLQueue().AddMessage(newXML);
                        processingXML = true;
                        System.Diagnostics.Debug.WriteLine("Added to XML Queue: " + NewURL);
                    }
                    else if (line.ToLower().StartsWith("disallow"))
                    {
                        string NewDisallow = line.Substring(9);
                        if (NewDisallow.StartsWith(" "))
                        {
                            NewDisallow = NewDisallow.Substring(1);
                        }
                        Blacklist.Add(NewDisallow);
                        System.Diagnostics.Debug.WriteLine("Disallow:" + NewDisallow);
                    }
                    else if (line.ToLower().StartsWith("allow"))
                    {
                        string NewAllow = line.Substring(6);
                        if (NewAllow.StartsWith(" "))
                        {
                            NewAllow = NewAllow.Substring(1);
                        }
                        Whitelist.Add(NewAllow);
                        System.Diagnostics.Debug.WriteLine("Allow:" + NewAllow);
                    }
                }
            }
        }

        public static void ProcessHTML(string url)
        {
            System.Diagnostics.Debug.WriteLine("ProcessHTML");
            try
            {

                HtmlWeb hw = new HtmlWeb();
                System.Diagnostics.Debug.WriteLine("HTML Loaded");
                HtmlDocument doc = hw.Load(url);
                string imglink = "none";
                try
                {
                    var img = doc.DocumentNode.SelectNodes("//img[contains(@class, 'media__image')]").FirstOrDefault();
                    imglink = img.Attributes["src"].Value;
                }
                catch
                {
                    imglink = "none";
                }
                var title = "no title found";
                try
                {
                    title = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
                }
                catch
                {
                    try
                    {
                        title = doc.DocumentNode.Descendants("title").FirstOrDefault().InnerText;
                    }
                    catch
                    {
                        title = "no title found";
                    }
                }
                List<string> masterIndex = new List<string>();
                if (title != "no title found")
                {
                    string[] titleWords = title.Split(new Char[] { ' ', ',', '.', ':', '!', '?', '-', '%', '"' });
                    foreach (string wd in titleWords)
                    {
                        if (!stopwordstitle.Contains(wd.ToLower()) && !masterIndex.Contains(wd.ToLower()) && !masterIndex.Contains(String.Concat(wd, "s").ToLower()) && wd.Length > 1)
                        {
                            if (!masterIndex.Contains(wd.Substring(0, wd.Length - 1)))
                            {
                                if (wd.Contains("'"))
                                {
                                    string nwd = wd.Replace("'", "");
                                    masterIndex.Add(nwd.ToLower());
                                }
                                else
                                {
                                    masterIndex.Add(wd.ToLower());

                                }
                            }
                        }
                    }
                }
                var date = DateTime.Now;
                try
                {
                    date = DateTime.Parse(doc.DocumentNode.SelectSingleNode("//head/meta[@name='pubdate']").Attributes["content"].Value);
                }
                catch
                {
                    try
                    {
                        date = DateTime.Parse(doc.DocumentNode.SelectSingleNode("//head/meta[@property='og:pubdate']").Attributes["content"].Value);
                    }
                    catch
                    {
                        date = DateTime.Now;
                    }
                }

                List<string> URLs = new List<string>();
                try
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string line = link.Attributes["href"].Value;
                        if ((line.Contains(".html") || line.Contains(".htm")) && (line.Contains("cnn.com") || line.Contains("bleacherreport.com")))
                        {
                            URLs.Add(line);
                        }
                    }
                    for (int i = 0; i < URLs.Count; i++)
                    {
                        string URL = URLs[i];
                        System.Diagnostics.Debug.WriteLine("Link on page: " + URL);
                        if (!CrawledURLs.Contains(URL) && VerifyLink(URL) 
                            && (URL.Contains("cnn.com") || (URL.Contains("bleacherreport.com") && URL.Contains("nba")))
                            && URL.Contains(".htm"))
                        {
                            CloudQueueMessage newQueueLink = new CloudQueueMessage(URL);
                            DBConnect.GetLinkQueue().AddMessageAsync(newQueueLink);
                            CrawledURLs.Add(URL);
                            System.Diagnostics.Debug.WriteLine("Added to LinkQueue: " + URL);
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("No <a> tags");
                }
                StringBuilder sb = new StringBuilder();
                try
                {
                    if (url.Contains("money.cnn") || url.Contains("bleacherreport"))
                    {
                        foreach (HtmlNode text in doc.DocumentNode.SelectNodes("//p"))
                        {
                            sb.Append(text.InnerText);
                        }
                    }
                    else
                    {
                        foreach (HtmlNode text in doc.DocumentNode.SelectNodes("//*[contains(@class, 'zn-body__paragraph')]"))
                        {
                            sb.Append(text.InnerText);
                        }
                    }
                } 
                catch
                {
                    System.Diagnostics.Debug.WriteLine("No body text found: " + url);
                }
                /*
                try
                {
                    if (sb != null)
                    {
                        string[] words = sb.ToString().Split(new Char[] { ',', '\n', ' ', '.', '?', '!', ':', ';', '"', '(', ')', '#', '@', '%', '*', '-', '_' });
                        foreach (string word in words)
                        {
                            if (!stopwords.Contains(word.ToLower()) && !masterIndex.Contains(word.ToLower()) && !masterIndex.Contains(String.Concat(word, "s").ToLower()) && !word.Contains("'") && word.Length > 1)
                            {
                                if (!masterIndex.Contains(word.Substring(0, word.Length - 1)))
                                {
                                    bool valid = word.All(c => Char.IsLetter(c));
                                    if (valid)
                                    {
                                        if (Char.IsUpper(word[0]))
                                        {
                                            masterIndex.Add(word.ToLower());
                                        }
                                    }

                                }
                            }
                        }
                        masterIndex.Sort((x, y) => string.Compare(x, y));
                    } else
                    {
                        sb.Append(" ");
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Failed at reading body to masterindex");
                }*/
                foreach (string key in masterIndex)
                {
                    LinkRow lr = new LinkRow(key, url, title, date, sb.ToString(), imglink, "");
                    TableOperation insertOperation = TableOperation.InsertOrReplace(lr);
                    try
                    {

                        DBConnect.GetLinkTable().ExecuteAsync(insertOperation);
                        System.Diagnostics.Debug.WriteLine("key: " + key + " url: " + url);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Table Insert Exception:  " + e + " url: " + url);
                    }
                }
                Task.Run(() => UpdateStats("Crawling"));
            }
            catch
            {
                Error("404", url);
            }
        }

        static void UpdateStats(string status)
        {
            TableOperation retrieve = TableOperation.Retrieve<Stats>("count", "count");

            TableResult results = DBConnect.GetStatsTable().Execute(retrieve);
            int newCount = 1;
            if (results.Result != null)
            {
                int oldCount = ((Stats)results.Result).Count;
                newCount = (int)oldCount + 1;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetStatsTable error");
            }

            PerformanceCounter cpuCount = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter ramCount = new PerformanceCounter("Memory", "Available MBytes");
            string oldCPU = cpuCount.NextValue() + "%";
            Thread.Sleep(1000);
            string newCPU = cpuCount.NextValue() + "%";
            string newRAM = (1792 - ramCount.NextValue()) + "MB";

            TableOperation insertStats = TableOperation.InsertOrReplace(new Stats(newCount, newCPU, newRAM, status));
            DBConnect.GetStatsTable().Execute(insertStats);
        }
        private static async void ProcessXMLQueue()
        {
            processingXML = true;
            while (processingXML)
            {
                CloudQueueMessage msg = DBConnect.GetXMLQueue().GetMessage(TimeSpan.FromMinutes(5));
                if (msg != null)
                {
                    ProcessXML(msg.AsString);
                    DBConnect.GetXMLQueue().DeleteMessage(msg);
                    Task.Run(() => UpdateStats("Loading..."));
                }
                else
                {
                    processingXML = false;
                }
            }
        }
        private static void ProcessXML(string URL)
        {

            XElement sitemap = XElement.Load(URL);
            //Different sitemap schemas
            XName url = XName.Get("url", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName urlAlt = XName.Get("url", "http://www.google.com/schemas/sitemap/0.9");
            XName loc = XName.Get("loc", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName time = XName.Get("lastmod", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName news = XName.Get("news", "http://www.google.com/schemas/sitemap-news/0.9");
            XName locX = XName.Get("loc", "http://www.google.com/schemas/sitemap/0.9");
            XName sitemaps = XName.Get("sitemap", "http://www.sitemaps.org/schemas/sitemap/0.9");
            XName newspubdate = XName.Get("publication_date", "http://www.google.com/schemas/sitemap-news/0.9");
            XName videopuddate = XName.Get("publication_date", "http://www.google.com/schemas/sitemap-video/1.1");
            XName video = XName.Get("video", "http://www.google.com/schemas/sitemap-video/1.1");
            XName temp = url;

            //check which sitemap url is used

            if (sitemap.Elements(urlAlt).Count() != 0)
            {
                temp = urlAlt;
            }
            else if (sitemap.Elements(sitemaps).Count() != 0)
            {
                temp = sitemaps;

            }

            DateTime pubdate;

            foreach (var urlElement in sitemap.Elements(temp))
            {

                if (urlElement.Element(loc) == null)
                {
                    loc = locX;
                }
                string locElement = urlElement.Element(loc).Value;
                if (urlElement.Element(time) != null)
                {
                    pubdate = DateTime.Parse(urlElement.Element(time).Value);
                }
                else if (urlElement.Element(news) != null)
                {
                    pubdate = DateTime.Parse(urlElement.Element(news).Element(newspubdate).Value);
                }
                else if (urlElement.Element(video) != null)
                {
                    pubdate = DateTime.Parse(urlElement.Element(video).Element(videopuddate).Value);
                }
                else
                {
                    //if no pubdate is found, this is a placeholder
                    pubdate = DateTime.Today;
                }

                if (DateTime.Now.AddMonths(-2) < pubdate)
                {

                    if (!(CrawledURLs.Contains(locElement)) && (locElement.Contains("cnn.com") || locElement.Contains("bleacherreport.com")) && !locElement.Contains(".xml"))
                    {
                        if (VerifyLink(locElement))
                        {
                            CloudQueueMessage newQueueLink = new CloudQueueMessage(locElement);
                            DBConnect.GetLinkQueue().AddMessageAsync(newQueueLink);
                            CrawledURLs.Add(locElement);
                            System.Diagnostics.Debug.WriteLine("Added to Link Queue: " + locElement);
                        }
                    }
                    else if (locElement.Contains(".xml") && locElement.Contains("2018"))
                    {
                        processingXML = true;
                        CloudQueueMessage newXML = new CloudQueueMessage(locElement);
                        DBConnect.GetXMLQueue().AddMessageAsync(newXML);
                        System.Diagnostics.Debug.WriteLine("Added to XML Queue: " + locElement);
                    }
                }

            }
        }

        private static bool VerifyLink(string link)
        {
            if (IsAllowed(link))
            {
                if (HTTPStatusCheck(link))
                {
                   return true;
                }
            }
            return false;
        }

        private static bool HTTPStatusCheck(string link)
        {

            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(link);
                webRequest.AllowAutoRedirect = false;

                using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
                {
                    int responseCode = (int)response.StatusCode;

                    if (responseCode == 200 || responseCode == 301 || responseCode == 302)
                    {
                        return true;
                    }
                    else
                    {
                        Error("HTTP Error: " + responseCode + " " + response.StatusCode.ToString(), link);
                        System.Diagnostics.Debug.WriteLine("ERROR: " + response.StatusCode.ToString());
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + e.Message);
                return false;
            }
        }
        private static bool IsAllowed(string link)
        {
            for (int i = 0; i < Blacklist.Count; i++)
            {
                if (link.Contains(Blacklist[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static void Error(string message, string url)
        {
            ErrorRow error = new ErrorRow(message, url);
            TableOperation insertOperation = TableOperation.Insert(error);
            DBConnect.GetErrorTable().Execute(insertOperation);
        }
    }
}
