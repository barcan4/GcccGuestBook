using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.Cosmos.Table;

namespace GcccData
{
    public class DataSource
    {
        private static readonly string tableName = "entry";

        private static CloudStorageAccount storageAccount;
        private static CloudTable table;

        static DataSource()
        {
            //var storageConnStr = AppSettings.LoadAppSettings().StorageConnectionString;

            storageAccount = CreateStorage("UseDevelopmentStorage=true");
            //storageAccount = CreateStorage("DefaultEndpointsProtocol=https;AccountName=gcccazureproject;AccountKey=HPNrIXAYzGiVF+5D7JQOUB6IlqFKRByTGlEhpiAnqKIJTd0Ny3jM78K4Fqrjxn9c+Ap/Fx9o+Wro+AStysh+1w==");

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();
        }

        private static CloudStorageAccount CreateStorage(string storageConnStr)
        {
            CloudStorageAccount cloudStorageAccount;
            
            try
            {
                cloudStorageAccount = CloudStorageAccount.Parse(storageConnStr);
            } catch(FormatException)
            {
                Console.WriteLine("Invalid format exception");
                throw;
            } catch(ArgumentException)
            {
                Console.WriteLine("Invalid argument exception");
                throw;
            }

            return cloudStorageAccount;
        }
        
        public IEnumerable<Entry> GetEntries()
        {
            var entries = table.ExecuteQuery(new TableQuery<Entry>()).OrderBy(e => e.RowKey).ToList();

            return entries;
        }

        public Entry GetEntryByPhotoUrl(string photoUrl)
        {
            TableQuery<Entry> query = new TableQuery<Entry>().Where(TableQuery.GenerateFilterCondition("ImageUrl", QueryComparisons.Equal, photoUrl));
            var entries = table.ExecuteQuery(query);
            var entry = entries.ElementAt(0);
            return entry;
        }

        public void UpdateEntryThumbnailUrl(string thumbnailUrl, string photoUrl)
        {
            TableQuery<Entry> query = new TableQuery<Entry>().Where(TableQuery.GenerateFilterCondition("ImageUrl", QueryComparisons.Equal, photoUrl));
            var entries = table.ExecuteQuery(query);
            var entry = entries.ElementAt(0);
            entry.ThumbnailUrl = thumbnailUrl;
            TableOperation tableOperation = TableOperation.Merge(entry);
            table.Execute(tableOperation);
        }

        public void AddEntry(Entry newEntry)
        {
            TableOperation tableOperation = TableOperation.Insert(newEntry);
            table.Execute(tableOperation);
        }

        public void UpdateThumbnail(Entry entry)
        {
            TableOperation tableOperation = TableOperation.Merge(entry);
            table.Execute(tableOperation);
        }
    }
}
