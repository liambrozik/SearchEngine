using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using ClassLibrary1;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace WebRole1
{
    /// <summary>
    /// Summary description for Admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Admin : System.Web.Services.WebService
    {
        private string ClearPassword = "INFO344";
        private static Dictionary<string, List<string>> SmartCache = new Dictionary<string, List<string>>();
        public static Trie searcher { get; set; }
        private static int WikiCount;
        private static bool stopwordcreated = false;
        private static List<string> stopwordstitle;
        private static string stopwordstxt = "a,about,above,after,again,against,all,am,an,and,any,are,aren't,as,at,be,because,been,before,being,below,between,both,but,by,can't,cannot,could,couldn't,did,didn't,do,does,doesn't,doing,don't,down,during,each,few,for,from,further,had,hadn't,has,hasn't,have,haven't,having,he,he'd,he'll,he's,her,here,here's,hers,herself,him,himself,his,how,how's,i,i'd,i'll,i'm,i've,if,in,into,is,isn't,it,it's,its,itself,let's,me,more,most,mustn't,my,myself,no,nor,not,of,off,on,once,only,or,other,ought,our,ours,ourselves,out,over,own,same,shan't,she,she'd,she'll,she's,should,shouldn't,so,some,such,than,that,that's,the,their,theirs,them,themselves,then,there,there's,theres,these,they,they'd,they'll,they're,theyre,they've,theyve,this,those,through,to,too,under,until,up,very,was,wasn't,we,we'd,we'll,we're,we've,were,weren't,what,what's,whats,when,when's,whens,where,where's,which,while,who,who's,whom,why,why's,with,won't,would,wouldn't,you,you'd,you'll,you're,you've,your,yours,yourself,yourselves,i";
        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");

        [WebMethod]
        public string NewCrawler(string u)
        {
            CloudQueueMessage rootURL = new CloudQueueMessage(u);
            DBConnect.GetLinkQueue().AddMessage(rootURL);
            return "Crawler initiated at " + u;
        }

        [WebMethod]
        public string StartCrawler()
        {
                CloudQueueMessage startMessage = new CloudQueueMessage("start");
                DBConnect.GetStopStartQueue().AddMessage(startMessage);
                return "Start message sent to crawler";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string StopCrawler()
        {
            CloudQueueMessage stopMessage = new CloudQueueMessage("stop");
            DBConnect.GetStopStartQueue().AddMessage(stopMessage);
            return "Stop message sent to crawler";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetStats()
        {
            TableOperation retrieve = TableOperation.Retrieve<Stats>("count", "count");
            TableResult result = DBConnect.GetStatsTable().Execute(retrieve);
            List<string> stats = new List<string>();
            if (result.Result != null)
            {
                int count = ((Stats)result.Result).Count;
                string cpu = ((Stats)result.Result).CPU;
                string ram = ((Stats)result.Result).RAM;
                string status = ((Stats)result.Result).Status;
                stats.Add(count.ToString());
                stats.Add(cpu);
                stats.Add(ram);
                stats.Add(status);
            } else
            {
                stats.Add("N/A");
                stats.Add("0%");
                stats.Add("0MB");
                stats.Add("Stopped");
            }
            return stats;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetErrors()
        {
            List<string> errors = new List<string>();
            var allErrors = DBConnect.GetErrorTable().ExecuteQuery(new TableQuery<ErrorRow>()).ToList();

            foreach (ErrorRow error in allErrors)
            {
                errors.Add(error.Message);
                errors.Add(error.URL);
            }

            return errors;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetLastTenLinks()
        {
            List<string> lastTenLinks = new List<string>();
            TableQuery<LinkRow> rangeQuery = new TableQuery<LinkRow>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, " ")
                );

            var results = DBConnect.GetLinkTable().ExecuteQuery(rangeQuery);

            foreach (var row in results.Take(10))
            {
                lastTenLinks.Add(row.URL);
            }

            return lastTenLinks;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> GetTitle(string q)
        {
            List<string> titledate = new List<string>();
            try
            {
                TableQuery<LinkRow> rangeQuery = new TableQuery<LinkRow>()
                    .Where(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, LinkRow.CreateMD5(q))
                    );

                var results = DBConnect.GetLinkTable().ExecuteQuery(rangeQuery);

                foreach (var row in results.Take(1))
                {
                    titledate.Add(row.Title);
                    titledate.Add("Published: " + row.DatePublished.ToString());
                }

                return titledate;
            } 
            catch
            {
                titledate.Add("No Page Found");
                titledate.Add("Try a different URL");
                return titledate;
            }
        }
        [WebMethod]
        public int GetQueueCount()
        {
            CloudQueue cur = DBConnect.GetLinkQueue();
            cur.FetchAttributes();
            if (cur.ApproximateMessageCount == null)
            {
                return 0;
            } else
            {
                return (int)cur.ApproximateMessageCount;
            }
        }
        private static void CreateStopWords()
        {
            if (!stopwordcreated)
            {
                stopwordstitle = new List<string>();
                string[] stops = stopwordstxt.Split(',');
                foreach (string stop in stops)
                {
                        if (!stopwordstitle.Contains(stop))
                        {
                            stopwordstitle.Add(stop);
                        }
                    
                }
                stopwordcreated = true;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> Query(string query)
        {
            WikiCount = 0;
            CreateStopWords();

            query = query.ToLower();

            if (SmartCache.ContainsKey(query.ToLower()))
            {
                return SmartCache[query];
            }
            else
            {
                //Dictionary<string, string[]> results = new Dictionary<string, string[]>();
                List<LinkRow> results = new List<LinkRow>();
                List<string> qWords = new List<string>();
                string[] queryWords = query.Split(new Char[] { ' ', ',', '.', ':', '!', '?', '-', '%', '"', '(', ')', ':', '#', '_' });
                foreach (string wd in queryWords)
                {
                    if (!stopwordstitle.Contains(wd.ToLower()) && !qWords.Contains(wd.ToLower()) && !qWords.Contains(String.Concat(wd, "s").ToLower()) && wd.Length > 1)
                    {
                        if (!qWords.Contains(wd.Substring(0, wd.Length - 1)))
                        {
                            if (wd.Contains("'"))
                            {
                                string nwd = wd.Replace("'", "");
                                qWords.Add(nwd.ToLower());
                            }
                            else
                            {
                                qWords.Add(wd.ToLower());

                            }
                        }
                    }
                }
                foreach (string word in qWords)
                {
                    try
                    {
                        TableQuery<LinkRow> rangeQuery = new TableQuery<LinkRow>()
                            .Where(
                                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, word)
                            );
                        var result = DBConnect.GetLinkTable().ExecuteQuery(rangeQuery).ToList();
                        results.AddRange(result);

                        /*
                        foreach (var row in result)
                        {
                            if (results.ContainsKey(row.URL))
                            {
                                string[] update = results[row.URL];
                                int old = int.Parse(update[3]);
                                int newInt = old + 1;
                                //RANKING
                                // Titlematch = 4pt
                                // Wikipedia Article = 2pt
                                // Bodymatch = 1pt
                                if (row.Title.ToLower().Contains(word.ToLower()))
                                {
                                    newInt = old + 4;
                                }
                                else
                                {
                                    newInt = old + 1;
                                }
                                string[] newInfo = new string[] { update[0], update[1], update[2], newInt.ToString(), update[4], update[5] };
                                results[row.URL] = newInfo;
                            }
                            else
                            {
                                string[] source = row.Body.Split(new char[] { '.', '?', '!' });
                                StringBuilder sbd = new StringBuilder("");
                                for (int i = 0; i < source.Length; i++)
                                {
                                    if (source[i].Contains(word))
                                    {
                                        sbd.Append(source[i]);
                                        try
                                        {
                                            sbd.Append(". ");
                                            sbd.Append(source[i + 1]);
                                        }
                                        catch { }
                                        sbd.Append("...");
                                        break;
                                    }
                                }
                                if (sbd.ToString() == "")
                                {
                                    try
                                    {
                                        sbd.Append(source[0]);
                                        try
                                        {
                                            sbd.Append(". ");
                                            sbd.Append(source[1]);
                                        }
                                        catch { }
                                        sbd.Append("...");
                                    }
                                    catch
                                    { }
                                }
                                var snippet = sbd.ToString();
                                string suggest = "";
                                if (row.URL.Contains("wikipedia"))
                                {
                                    try
                                    {
                                        suggest = row.Suggestions;
                                    }
                                    catch { }
                                }
                                string starterPoints = "1";
                                //RANKING
                                // Titlematch = 4pt
                                // Wikipedia Article = 2pt
                                // Bodymatch = 1pt
                                if (row.Title.ToLower().Contains(word.ToLower()) && row.URL.Contains("wikipedia"))
                                {
                                    starterPoints = "6";
                                }
                                else if (row.Title.ToLower().Contains(word.ToLower()))
                                {
                                    starterPoints = "4";
                                }
                                else
                                {
                                    starterPoints = "1";
                                }
                                string[] info = new string[] { row.Title, row.DatePublished.ToString(), snippet, starterPoints, row.Img, suggest };
                                results.Add(row.URL, info);

                            }
                        } */
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("TableQuery Failed");
                    }
                }
                //var sorted = results.OrderByDescending(x => int.Parse(x.Value[3])).Take(15);
                var sorted = results.GroupBy(x => x.URL)
                    .Select(x => new Tuple<LinkRow, int>(x.ToList().First(), calculateScore(x.ToList().First(), qWords)))
                    .OrderByDescending(x => x.Item2)
                    .ThenByDescending(x => x.Item1.DatePublished)
                    .Take(10).ToList();
                // calculateScore(x.ToList().First(), qWords)
                // x.ToList().Count


                List<string> r = new List<string>();
                foreach (var pair in sorted)
                {
                    StringBuilder sb = new StringBuilder();
    
                    sb.Append(pair.Item1.URL);
                    sb.Append("~");
                    sb.Append(pair.Item1.Title);
                    sb.Append("~");
                    sb.Append(pair.Item1.DatePublished);
                    sb.Append("~");
                    sb.Append(pair.Item1.Body);
                    sb.Append("~");
                    sb.Append("null");
                    sb.Append("~");
                    sb.Append(pair.Item1.Img);
                    sb.Append(pair.Item1.Suggestions);
                    r.Add(sb.ToString());
                }
                SmartCache.Add(query.ToLower(), r);
                return r;
            }
        }

        private int calculateScore(LinkRow res, List<string> words)
        {
            int total = 0;

            foreach (string word in words)
            {
                if (res.Title.Contains(word))
                {
                    total = total + 4;
                }
                else
                {
                    total = total + 1;
                }
            }
            if (res.URL.Contains("wikipedia") && WikiCount <= 4)
            {
                WikiCount++;
                total = total + 2;
            }
            return total;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> Preview(string key, string url)
        {
            List<string> results = new List<string>();
            string rkLowerFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, LinkRow.CreateMD5(url));

            string pkUpperFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key);

            // Note CombineFilters has the effect of “([Expression1]) Operator (Expression2]), as such passing in a complex expression will result in a logical grouping. 
            string combinedFilter = TableQuery.CombineFilters(rkLowerFilter, TableOperators.And, pkUpperFilter);
            try
            {
                TableQuery<LinkRow> rangeQuery = new TableQuery<LinkRow>()
                    .Where(combinedFilter);
                var result = DBConnect.GetLinkTable().ExecuteQuery(rangeQuery).Take(1).ToList();
                LinkRow res = result[0];
                results.Add(res.Title);
                results.Add(res.URL);
                results.Add(res.Img);
                results.Add(res.Body);

            }
            catch { results.Add("No Preview Found"); }
            
            return results;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearQueue(string password)
        {
            if (password == ClearPassword)
            {
                DBConnect.GetLinkQueue().Clear();
                return "LinkQueue has been cleared";
            } else
            {
                return "Incorrect Password";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearTable(string password)
        {
            if (password == ClearPassword)
            {
                DBConnect.GetLinkTable().Delete();
                return "LinkTable has been cleared";
            }
            else
            {
                return "Incorrect Password";
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ClearErrors()
        {
            DBConnect.GetErrorTable().Delete();
            return "ErrorTable has been cleared";
        }

        [WebMethod]
        public string downloadWikiTitles()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("wikititles");
            string dir = "";
            if (container.Exists())
            {
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        dir = System.IO.Path.GetTempFileName();
                        blob.DownloadToFile(dir, FileMode.Create);
                    }
                }
            }
            return dir;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> BuildTrie(string fileName)
        {
            searcher = new Trie();
            List<string> r = new List<string>();
            List<string> titles = new List<string>();
            string title = "No titles added";
            string lastTitle = "";
            int lastIndex = 0;
            int totalIndex = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                bool spaceAvailable = true;
                bool titlesLeft = true;
                while (spaceAvailable && titlesLeft)
                {

                    for (int i = lastIndex; i < lastIndex + 10000; i++)
                    {
                        if ((title = sr.ReadLine()) == null)
                        {
                           
                            titlesLeft = false;
                            break;
                        }
                        else
                        if (title != "page title")
                        {
                            lastTitle = title;
                            totalIndex++;
                            //searcher.Insert(title);
                            searcher.Insert(title, searcher.GetRoot());
                        }
                    }
                    float memory = memProcess.NextValue();
                    if (memory <= 15)
                    {
                        Console.WriteLine("Last entry: " + title);
                        spaceAvailable = false;
                    }
                    else
                    {

                        lastIndex += 10000;
                    }
                }

            }
            r.Add((totalIndex).ToString());
            r.Add(lastTitle);
            return r;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> SearchTrie(string q)
        {
            /*
            List<string> results = new List<string>();
            results = searcher.PrefixSearch(q);
            return results;*/
            List<string> results = new List<string>();
            try
            {
                Node start = searcher.Prefix(q);
                string pre = q.Substring(0, start.Depth);
                string str = q.Substring(start.Depth);
                if (q.Length <= start.Depth)
                {
                    searcher.GetSuggestions(start, str, results);
                }
                else
                {
                    for (int i = 0; i < start.Suffix.Count; i++)
                    {
                        if (start.Suffix[i].StartsWith(str))
                        {
                            results.Add(start.Suffix[i]);
                        }
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(pre);
                    sb.Append(results[i]);
                    results[i] = sb.ToString();
                }
                for (int i = 0; i < results.Count; i++)
                {
                    if (!results[i].Contains(q))
                    {
                        results.Clear();
                        break;
                    }
                }
            } catch
            {
            }
            return results;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string AddSuggestion(string q)
        {
            Node current = searcher.Prefix(q);
            //if (current.Depth == q.Length && current.FindChild('@') != null)
            if (current.Suffix.Contains(q.Substring(q.IndexOf(current.Value) + 1))
                || current.Depth == q.Length && current.Suffix.Contains("@"))
            {
                return "!" + q + " was found!";
            }
            else
            {
                //searcher.Insert(q);
                // return q + " was added to the suggestions!";
                return q + " was not found!";
            }
        }

        private static float getAvailableRAM(System.Diagnostics.PerformanceCounter RAM)
        {
            return RAM.NextValue();
        }
    }
}
