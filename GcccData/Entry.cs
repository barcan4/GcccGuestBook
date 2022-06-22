using System;
using Microsoft.Azure.Cosmos.Table;

namespace GcccData
{
    public class Entry : TableEntity
    {
        public Entry()
        {
            PartitionKey = DateTime.UtcNow.ToString("dMMyyyy");

            RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string Message { get; set; }

        public string GuestName { get; set; }
        
        public string ImageUrl { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}
