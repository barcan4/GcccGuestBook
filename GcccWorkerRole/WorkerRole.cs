using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using GcccData;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace GcccWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly string imagesContainerName = "guestbookimagesblob";
        private readonly string thumbnailQueueName = "guestbookthumbnails";
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private QueueClient queue;
        private BlobContainerClient blobContainer;

        public override void Run()
        {
            Trace.TraceInformation("GcccWorkerRole is running");

            try
            {
                QueueMessage message = queue.ReceiveMessage();
                if (message != null)
                {
                    var imageBlobUri = message.Body.ToString();
                    string thumbnailName = System.Text.RegularExpressions.Regex.Replace(imageBlobUri, "([^\\.]+)(\\.[^\\.]+)?$", "$1-thumb$2");

                    BlobClient inBlob = blobContainer.GetBlobClient(imageBlobUri);
                    BlobClient outBlob = blobContainer.GetBlobClient(thumbnailName);
                    if (!outBlob.Exists())
                    {
                        using (Stream input = inBlob.OpenRead())
                        using (MemoryStream output = new MemoryStream())
                        {
                            this.ProcessImage(input, output);
                            output.Position = 0;

                            outBlob.Upload(output);

                            string thumbnailBlobUri = outBlob.Uri.ToString();

                            DataSource dataSource = new DataSource();
                            Entry entry = dataSource.GetEntryByPhotoUrl(inBlob.Uri.ToString());
                            entry.ThumbnailUrl = thumbnailBlobUri;
                            dataSource.UpdateThumbnail(entry);

                            queue.DeleteMessage(message.MessageId, message.PopReceipt);
                        }
                    } else
                    {
                        queue.DeleteMessage(message.MessageId, message.PopReceipt);
                    }
                } else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            } catch(Azure.RequestFailedException ex)
            {
                Trace.TraceError("Failure when processing queue '{0}'", ex.Message);
                System.Threading.Thread.Sleep(5000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            var storageConnStr = AppSettings.LoadAppSettings().StorageConnectionString;
            blobContainer = new BlobContainerClient(storageConnStr, imagesContainerName);
            queue = new QueueClient(storageConnStr, thumbnailQueueName);

            bool storageInitialized = false;
            while (!storageInitialized)
            {
                try
                {
                    blobContainer.CreateIfNotExists();
                    blobContainer.SetAccessPolicy(PublicAccessType.Blob);
                    queue.CreateIfNotExists();
                    storageInitialized = true;
                } catch (Azure.RequestFailedException ex)
                {
                    if (ex.Status.Equals(HttpStatusCode.NotFound))
                    {
                        Trace.TraceError("Failure initialising storage services\n Message: '{0}'", ex.Message);
                        System.Threading.Thread.Sleep(5000);
                    } else
                    {
                        throw;
                    }
                }
            }

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("GcccWorkerRole has been started");

            return result;
        }

        public void ProcessImage(Stream input, Stream output)
        {
            int width;
            int height;
            var image = new Bitmap(input);

            if (image.Width > image.Height)
            {
                width = 128;
                height = 128 * image.Height / image.Width;
            } else
            {
                height = 128;
                width = 128 * image.Width / image.Height;
            }

            var thumbnail = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(thumbnail))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }

            thumbnail.Save(output, ImageFormat.Jpeg);
        }

        public override void OnStop()
        {
            Trace.TraceInformation("GcccWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("GcccWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
