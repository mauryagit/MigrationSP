using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Data;
using System.IO;
using System.Configuration;
using Common;
namespace MigratedDocument
{
    class MigratedDocumentCls
    {
        static void Main(string[] args)
        {
            Console.Title = "Download Blog Document";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            DBM db = new DBM();
            DataTable dt = null;

            if (args.Length == 0)
            {
                dt = db.getListItemDocuments("", "");
            }
            else
            {
                dt = db.getListItemDocumentsForSpecificWeb(args[0].ToString());
            }



            foreach (DataRow dr in dt.Rows)
            {
                string tempfilepath = dr["FilePath"].ToString();
                string hostName = GetHostUrl(dr["SITECOLLECTION"].ToString(), dr["SUBSITE"].ToString());
                // Uri filename = new Uri(dr["SiteUrl"].ToString() +"/"+ tempfilepath);
                Uri filename = new Uri(hostName + tempfilepath);
                string server = filename.AbsoluteUri.Replace(filename.AbsolutePath, "");
                string serverrelative = filename.AbsolutePath;

                Console.WriteLine(filename.AbsolutePath);
                string downloadlocationfilepath = ConfigurationManager.AppSettings["DownloadLocation"].ToString() + dr["RowID"].ToString() + "_" + serverrelative.Substring(serverrelative.LastIndexOf("/") + 1);
                try
                {
                    Microsoft.SharePoint.Client.ClientContext clientContext =
                        new Microsoft.SharePoint.Client.ClientContext(server);
                    Microsoft.SharePoint.Client.FileInformation f =
                    Microsoft.SharePoint.Client.File.OpenBinaryDirect(clientContext, serverrelative);

                    clientContext.ExecuteQuery();

                    using (var fileStream = new FileStream(downloadlocationfilepath, FileMode.Create))
                        f.Stream.CopyTo(fileStream);
                    db.updateListItemDocumentDownloadStatus(dr["RowID"].ToString(), downloadlocationfilepath, dr["ListDefinationRowID"].ToString());
                }
                catch (Exception)
                {
                    db.updateListItemDocumentDownloadStatus(dr["RowID"].ToString(), null, dr["ListDefinationRowID"].ToString());

                }
            }
            Console.WriteLine(string.Format("End {0}", DateTime.Now));
            // Console.Read();
        }

        private static string GetHostUrl(string siteColl, string subsite)
        {
            string hostName = ConfigurationManager.AppSettings["SiteUrl"].ToString();
            if (string.IsNullOrEmpty(siteColl))
            {
                switch (subsite.ToLower())
                {
                    case "fc&a":
                        hostName = "https://fcna.ril.com";
                        break;
                    case "learnet":
                        hostName = "http://learnet.ril.com";
                        break;
                }
            }
            else
            {
                switch (subsite.ToLower())
                {
                    case "budget analysis & impact 2015":
                    case "financial management system":
                    case "international taxation":
                    case "about fc&a academy":
                    case "academy newsletter":
                    case "fc&a finance":
                    case "fcna accounting":
                    case "gst":
                    case "ind-as 20:20":
                    case "news":
                    case "xpro chats":
                        hostName = "https://fcna.ril.com";
                        break;
                    case "mt":
                    case "theme-month":
                        hostName = "https://discoversocial.ril.com";
                        break;
                    case "blogs":
                    case "discussionforum":
                    case "keyblogs":
                        hostName = "https://hrsocial.ril.com";
                        break;
                    case "contracting":
                    case "cost benefits":
                    case "disposal management":
                    case "gst queries":
                    case "inspection & expediting":
                    case "logistics mgmt":
                    case "materials mgmt.":
                    case "my corner":
                    case "p&c bt":
                    case "p&c digital transformation":
                    case "p&c sucess stories":
                    case "procurement":
                    case "social":
                    case "tips from the leaders":
                        hostName = "https://pncsocial.ril.com";
                        break;
                    case "rtg_blogs":
                        hostName = "https://rndsocial.ril.com";
                        break;
                    case "tech blog":
                        hostName = "https://techtalk.ril.com";
                        break;
                    case "learnet":
                        hostName = "https://learnet.ril.com";
                        break;
                }
            }
            return hostName;
        }
    }
    public static class StreamExtensions
    {
        public static void CopyTo(this System.IO.Stream src, System.IO.Stream dest)
        {
            if (src == null)
                throw new System.ArgumentNullException("src");
            if (dest == null)
                throw new System.ArgumentNullException("dest");

            System.Diagnostics.Debug.Assert(src.CanRead, "src.CanRead");
            System.Diagnostics.Debug.Assert(dest.CanWrite, "dest.CanWrite");

            int readCount;
            var buffer = new byte[8192];
            while ((readCount = src.Read(buffer, 0, buffer.Length)) != 0)
                dest.Write(buffer, 0, readCount);
        }
    }
}
