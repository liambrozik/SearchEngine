using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using ClassLibrary1;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;
using System.Xml.Linq;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage.Table;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");
            while (true)
            {
                CloudQueueMessage smessage = DBConnect.GetStopStartQueue().GetMessage(TimeSpan.FromMinutes(5));
                if (smessage != null)
                {

                    if (smessage.AsString == "start" || Crawler.crawling)
                    {
                        Crawler.crawling = true;
                        DBConnect.GetStopStartQueue().DeleteMessage(smessage);
                        Crawler.StartCrawl();
                    } else
                    {
                        DBConnect.GetStopStartQueue().DeleteMessage(smessage);
                    }
                }
                if (Crawler.crawling)
                {
                    Crawler.StartCrawl();
                }
                Thread.Sleep(3000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("WorkerRole1 has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("WorkerRole1 is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("WorkerRole1 has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                

                await Task.Delay(10000);
            }
        }
    }
}