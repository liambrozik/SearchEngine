using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class Stats : TableEntity 
    {
        public Stats(int count, string cpu, string ram, string status)
        {
            this.PartitionKey = "count";
            this.RowKey = "count";
            this.Count = count;
            this.CPU = cpu;
            this.RAM = ram;
            this.Status = status;
        }

        public Stats() { }

        public int Count { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string Status { get; set; }
    }
}
