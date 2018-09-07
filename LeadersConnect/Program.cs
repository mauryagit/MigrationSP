using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeadersConnect
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "LeadersConnect";
            DBM db = new DBM();
           
            ClientContext ctx = null;
            List<Blog> blogUploadCollection = db.getRecordForLeadersConnect();
            foreach (Blog b in blogUploadCollection)
            {
                bool rootSite = false;
                string url = "http://sp13devwfe01:8623/{0}/{1}";
                if (b.siteCollection.ToLower() == "root")
                {
                    url = "http://sp13devwfe01:8623";
                    rootSite = true;
                }
                Console.WriteLine(string.Format(url, b.siteCollection, b.subSite));
                Console.WriteLine(string.Format("{0} {1} {2}", b.siteCollection, b.subSite, b.ID));
                ctx = new ClientContext(string.Format(url, b.siteCollection, b.subSite));
                Web oWeb = ctx.Web;
                ctx.Load(oWeb);
                ctx.ExecuteQuery();
                ListItem oItem = null;
                if (b.typeOfContent.ToLower() == "t")
                {
                    if (rootSite)
                    {
                        oItem = oWeb.Lists.GetByTitle("MyCorner").GetItemById(Convert.ToInt32(b.ID));
                    }
                    else
                    {
                        oItem = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(Convert.ToInt32(b.ID));
                    }
                }
                else
                {
                    oItem = oWeb.Lists.GetByTitle("Discussions List").GetItemById(Convert.ToInt32(b.ID));
                }

                ctx.Load(oItem);
                ctx.ExecuteQuery();

                b.body = oItem["Body"].ToString();
                if (b.typeOfContent.ToLower() == "v")
                {
                    b.filepath = oItem["FileName"].ToString();
                }

                //Add ListItem
                AddItem(db, b, oItem);
            }
            MoveThumbNail(db);
            Console.ReadKey();
        }

        private static void AddItem(DBM db, Blog b, ListItem oItem)
        {
            ClientContext ctx = new ClientContext("http://sp13devwfe01:8623/leadersconnect/leadersconnect/");
            Web oSourceWeb = ctx.Web;
            ctx.Load(oSourceWeb);
            ctx.ExecuteQuery();
            List sourceList = null;
            if (b.typeOfContent.ToLower() == "t")
            {
                sourceList = oSourceWeb.Lists.GetByTitle("DiscussionText");
            }
            else
            {
                sourceList = oSourceWeb.Lists.GetByTitle("Discussions List");
            }
            ListItemCreationInformation createItem = new ListItemCreationInformation();
            ListItem destinationoItem = sourceList.AddItem(createItem);

            destinationoItem["Title"] = b.title;
            destinationoItem["ApprovedDate"] = b.created;// (b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published);
            destinationoItem["KeyAreas"] = b.keyArea;
            destinationoItem["ContenTypeId"] = b.contentType;
            destinationoItem["_ModerationStatus"] = 0;
            destinationoItem["ThumbnailPath"] = b.thumnailPath;
            destinationoItem["ApprovalStatus"] = "Approved";
            destinationoItem["moderatoremail"] = 1;
            destinationoItem["approvalrejectemail"] = 1;
            destinationoItem["migrated"] = 1;

            if (b.typeOfContent.ToLower() == "v")
            {
                destinationoItem["FileName"] = b.filepath;
            }

            destinationoItem["Body"] = formatBody(b.body, ctx);
            //destinationoItem["LikesCount"] = b.likeCount;
            //destinationoItem["TotalLikeCount"] = b.likeCount;
            // oItem["LikedBy"] = b.likedBy;
            destinationoItem["Created"] = b.created;
            destinationoItem["Modified"] = b.modified;
            destinationoItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(destinationoItem);
            User userinFo = resolveUser(b.author, ctx);
            b.author = userinFo.Title;
            destinationoItem["Author"] = userinFo;
            destinationoItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(destinationoItem);
            destinationoItem["Editor"] = resolveUser(b.editor, ctx);
            destinationoItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(destinationoItem);
            ctx.ExecuteQuery();
            int newblogID = Convert.ToInt16(destinationoItem["ID"]);
            db.blogAnalytics(b, destinationoItem, ctx.Web.Title, ctx.Web.Title);

        }
        private static User resolveUser(string user, ClientContext ctx)
        {
            User returnUser = ctx.Web.EnsureUser(user);
            try
            {
                ctx.Load(returnUser);
                ctx.ExecuteQuery();
            }
            catch (Exception)
            {
                //returnUser = ctx.Web.EnsureUser(ConfigurationManager.AppSettings["defaultUser"].ToString());
                //ctx.Load(returnUser);
                //ctx.ExecuteQuery();
            }


            return returnUser;
        }
        private static string formatBody(string body, ClientContext ctx)
        {
            //string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|<a.*?href=""(?<url>.*?)"".*?>";//*?</a>";
            string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|<a.*?href=""(?<url>.*?)"".*?><img.*?src=""(?<url>.*?)"".*?>(?<filename>.*?)</a>";
            Regex rx = new Regex(pattern);
            MatchCollection mc = rx.Matches(body.Replace("'", "\""));
            string newBody = body.Replace("'", "\"");
            if (mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    //Console.WriteLine(rx.Replace(body,m.Groups["filename"].Value));
                    //return rx.Replace(body.Replace("'", "\""), "Download from attachment section");
                    if (m.Groups["filename"].Value != "")
                    {
                        newBody = newBody.Replace(m.Value, m.Groups["filename"].Value);
                    }
                    else
                    {
                        string valueToReplace = ctx.Web.ServerRelativeUrl + "/TextBlogsImages" + m.Groups["url"].Value.Substring(m.Groups["url"].Value.LastIndexOf("/"));
                        newBody = newBody.Replace(m.Groups["url"].Value, valueToReplace);
                    }
                }
                return newBody.Replace(">?", ">");
                // Console.WriteLine(rx.Replace(body.Replace("'", "\""), mc[0].Groups["filename"].Value));
                //return rx.Replace(body.Replace("'", "\""), "Download from attachment section");
            }
            return body.Replace(">?", ">");
        }

        private static void MoveThumbNail(DBM db)
        {
            ClientContext ctx = new ClientContext("http://sp13devwfe01:8623/leadersconnect/leadersconnect/");
            Web oSourceWeb = ctx.Web;
            ctx.Load(oSourceWeb);
            ctx.ExecuteQuery();
            foreach (DataRow dr in db.getRecordForLeadersConnectThumbNail().Rows)
            {
                string filePath = dr["DownloadPath"].ToString();
                FileInfo temp = new FileInfo(filePath);
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    List oDocumentList = null;
                    if (dr["Type"].ToString().ToLower() == "t")
                    {
                        oDocumentList = ctx.Web.Lists.GetByTitle("TextBlogsImages");
                    }
                    else { oDocumentList = ctx.Web.Lists.GetByTitle("VideoThumbnails"); }
                    ctx.Load(oDocumentList.RootFolder);
                    ctx.ExecuteQuery();
                    var fileUrl = String.Format("{0}/{1}", oDocumentList.RootFolder.ServerRelativeUrl, System.Web.HttpUtility.UrlDecode(temp.Name.Substring(temp.Name.IndexOf("_") + 1)));
                    Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, fileUrl, fs, true);
                }
            }
        }
    }
}
