using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Configuration;
using Microsoft.SharePoint.Client;
using System.Data;
namespace ScanMigratedSiteStructure
{
    class ScanMigratedSiteStructureCls
    {
        static string MainWeb = string.Empty;
        static string SubWeb = string.Empty;
        static string Url = string.Empty;
      
        static void Main(string[] args)
        {
            Console.Title = "Scan Site Structure";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            string siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
            foreach (string url in siteUrl.Split(new string[]{";"},StringSplitOptions.RemoveEmptyEntries))
            {
                ClientContext context = new ClientContext(url);
                getStastics(context);
                Console.WriteLine(string.Format("End {0}", DateTime.Now));
            }
        }

        static void LogSiteStructure(string sitecoll, string subsite, string siteurl)
        {
            DBM db = new DBM();
            db.createSiteMigratedStructure(sitecoll, subsite, siteurl,"Inprogress");
        }
        
        static void getStastics(ClientContext context)
        {
            // Get the SharePoint web  
            scanroot(context);
            WebCollection webs = context.Web.Webs;
            context.Load(webs);
            context.ExecuteQuery();
            foreach (Web w in webs)
            {
                context.Load(w.ParentWeb);
                context.ExecuteQuery();
                
                Console.WriteLine(w.Title + " : " + w.Url);
                LogSiteStructure(w.ParentWeb.Title, w.Title, w.Url);
                //Scan subweb
                scanweb(context, w);               
            }
        }
        static void scanroot(ClientContext ctx)
        {
            // ctx.Load(ctx.Web.ParentWeb);
            Web root = ctx.Web;
            ctx.Load(root);
            ctx.Load(ctx.Web.ParentWeb);
            ctx.ExecuteQuery();
            //Get root
            Console.WriteLine(root.Title + " : " + root.Url );
            LogSiteStructure(string.Empty, root.Title, root.Url);
        }
        static void scanweb(ClientContext ctx, Web w)
        {
            WebCollection webs = w.Webs;           
            ctx.Load(webs);
            ctx.ExecuteQuery();
            foreach (Web iw in webs)
            {
                ctx.Load(iw.ParentWeb);
                ctx.ExecuteQuery();
                // if (iw.Title == "GST" )
                Console.WriteLine("====== " + iw.Title + " : " + iw.Url);
                LogSiteStructure(iw.ParentWeb.Title, iw.Title, iw.Url);
                

            }
        }
    }
}
