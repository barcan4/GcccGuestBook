using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using GcccData;
using System;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GcccWebRole
{
    public partial class _Default : Page
    {
        private readonly string containerName = "guestbookimagesblob";

        private static bool isInitialised = false;
        private static object isLocked = new object();

        private static BlobContainerClient blobContainerClient;
        private static QueueClient queueClient;
        private static DataSource dataSource = new DataSource();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Timer1.Enabled = true;
            }
        }

        protected void SubmitBtn_Click(object sender, EventArgs e)
        {
            if(ImagineFU.HasFile)
            {
                InitStorage();

                string blobNameUnique = string.Format("imagine_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(ImagineFU.FileName));
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobNameUnique);

                using (var filestream = ImagineFU.PostedFile.InputStream)
                {
                    blobClient.Upload(filestream);
                }

                Entry entry = new Entry()
                {
                    GuestName = NameTB.Text,
                    Message = MesajTB.Text,
                    ImageUrl = blobClient.Uri.ToString(),
                    ThumbnailUrl = blobClient.Uri.ToString()
                };
                dataSource.AddEntry(entry);

                if (queueClient.Exists())
                {
                    var msg = blobNameUnique;
                    queueClient.SendMessage(msg);
                }
            }

            NameTB.Text = string.Empty;
            MesajTB.Text = string.Empty;

            ComentariiDataList.DataBind();
        }

        private void InitStorage()
        {
            if (isInitialised)
            {
                return;
            }

            lock (isLocked)
            {
                if (isInitialised)
                {
                    return;
                }

                try
                {
                    //var storageConnStr = AppSettings.LoadAppSettings().StorageConnectionString;

                    //blobContainerClient = new BlobContainerClient("DefaultEndpointsProtocol=https;AccountName=gcccazureproject;AccountKey=HPNrIXAYzGiVF+5D7JQOUB6IlqFKRByTGlEhpiAnqKIJTd0Ny3jM78K4Fqrjxn9c+Ap/Fx9o+Wro+AStysh+1w==", containerName);
                    blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", containerName);
                    blobContainerClient.CreateIfNotExists();

                    blobContainerClient.SetAccessPolicy(PublicAccessType.Blob);

                    string queueName = "guestbookthumbnails";
                    //queueClient = new QueueClient("DefaultEndpointsProtocol=https;AccountName=gcccazureproject;AccountKey=HPNrIXAYzGiVF+5D7JQOUB6IlqFKRByTGlEhpiAnqKIJTd0Ny3jM78K4Fqrjxn9c+Ap/Fx9o+Wro+AStysh+1w==", queueName);
                    queueClient = new QueueClient("UseDevelopmentStorage=true", queueName);
                    queueClient.CreateIfNotExists();
                }
                catch (WebException)
                {
                    throw new WebException("Initializare esuata.");
                }

                isInitialised = true;
            }
        }

        protected void Imagine_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton imageButton = sender as ImageButton;
            ImagineFull.ImageUrl = imageButton.Attributes["ImageFull"].ToString();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "imageMod", "$('#imageMod').modal('show');", true);
            ImagineUPMod.Update();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            ComentariiDataList.DataBind();
        }
    }
}