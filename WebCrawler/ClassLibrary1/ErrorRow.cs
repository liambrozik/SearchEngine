using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    public class ErrorRow : TableEntity
    {
        public ErrorRow(string message, string url)
        {
            this.PartitionKey = "error";
            this.RowKey = Guid.NewGuid().ToString();
            this.Message = message;
            this.URL = url;
        }

        public ErrorRow() { }

        public string Message { get; set; }
        public string URL { get; set; }
    }
}
