using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{

    public class DBConnect
    {
        public static CloudTable LinkTable { get; set; }
        public static CloudQueue LinkQueue { get; set; }
        public static CloudTable StatsTable { get; set; }
        public static CloudQueue XMLQueue { get; set; }
        public static CloudQueue StopStartQueue { get; set; }
        public static CloudTable ErrorTable { get; set; }
        public static int TableIndex { get; set; }
        public static int TotalURLsCrawled { get; set; }
        public DBConnect() { }

        public static CloudTable GetLinkTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            LinkTable = tableClient.GetTableReference("linktable");
            LinkTable.CreateIfNotExists();

            return LinkTable;
        }

        public static CloudTable GetStatsTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            StatsTable = tableClient.GetTableReference("statstable");
            StatsTable.CreateIfNotExists();

            return StatsTable;
        }

        public static CloudQueue GetLinkQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            LinkQueue = queueClient.GetQueueReference("linkqueue");
            LinkQueue.CreateIfNotExists();
            LinkQueue.FetchAttributes();

            return LinkQueue;
        }

        public static CloudQueue GetXMLQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            XMLQueue = queueClient.GetQueueReference("xmlqueue");
            XMLQueue.CreateIfNotExists();

            return XMLQueue;
        }

        public static CloudQueue GetStopStartQueue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            StopStartQueue = queueClient.GetQueueReference("stopstart");
            StopStartQueue.CreateIfNotExists();

            return StopStartQueue;
        }

        public static CloudTable GetErrorTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            ErrorTable = tableClient.GetTableReference("errortable");
            ErrorTable.CreateIfNotExists();

            return ErrorTable;
        }

        public void SetTotalURLsCrawled(int Count, bool MultiThreaded)
        {
            if (MultiThreaded)
            {
                TotalURLsCrawled += Count;

            } else
            {
                TotalURLsCrawled = Count;
            }
        }
    }
}
