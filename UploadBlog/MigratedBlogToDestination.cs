using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UploadBlog
{
    class MigratedBlogToDestination
    {
        static DBM db = new DBM();
        static string siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
        static void Main(string[] args)
        {
            //tempFunc();
            //Environment.Exit(0);
            Console.Title = "Uploading Blog";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            //Get Collection
            List<KeyArea_Channel> keyAreaChannelColl = db.getKeyArea_Channel();
            List<Common.ContentType> contentTypeColl = db.getContentType();
            //args.[0] = "News";
            //if (args.Length == 0)
            //{
            //    Console.WriteLine("Site parameter missing...");
            //    Environment.Exit(100);
            //}
           // List<Blog> blogUploadCollection = db.getBlogsToUpload(args[0].ToString(),args[1].ToString());
            List<Blog> blogUploadCollection = db.getBlogsToUpload("Learnet","V");

            ClientContext ctx;
            Web oWeb;
            foreach (Blog b in blogUploadCollection)
            {
                //if (b.subSite == "Learnet")//|| b.ID !=15)
                //{
                //    continue;
                //    setSiteUrlForLearnet(b);
                //    if (siteUrl == "Not mapped to any site")
                //        continue;
                //}
                //else
                //{
                //    siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
                //}
                string p = (string.IsNullOrEmpty(b.project) ? "" : b.project).ToLower();
                //if (p == "retail" || p == "jio")
               // if (p != "retail")
                 //   continue;
                setSiteUrlForLearnet(b, "http://digitalj3.ril.com");
                if (siteUrl == "Not mapped to any site")
                    continue;
                //set Learnet site url
                //ONly video 
                // if (b.categoryType.ToString() != "V") { continue; }
                b.project = "Hydrocarbon";
                Console.WriteLine(string.Format("Uploading Blog ID {0}", b.ID));
                prepareWebContext(out ctx, out oWeb);
                // Console.WriteLine(formatBody(b.body,ctx));
                List olist = null;
                if (b.categoryType.ToLower() == "t")
                {
                    if ((b.siteCollection.ToLower() == "hr social" && b.subSite.ToLower() == "blogs"))
                    {
                        olist = oWeb.Lists.GetByTitle("MyCorner");
                        continue; // for mycorner blog

                    }
                    else
                    {
                        if (b.siteCollection.ToLower() == "" && b.subSite.ToLower() == "fc&a")
                        {
                            if(b.categories.ToLower() == "mycorner")
                            {
                            olist = oWeb.Lists.GetByTitle("MyCorner");
                            continue; // for mycorner blog
                            }
                        }
                        olist = oWeb.Lists.GetByTitle("DiscussionText");
                    }
                }
                else
                {
                    olist = oWeb.Lists.GetByTitle("Discussions List");
                }
                ListItemCreationInformation createItem = new ListItemCreationInformation();
                ListItem oItem = olist.AddItem(createItem);

                // parse keyArea and contenttype
                b.keyArea = formatKeyArea(b, ref keyAreaChannelColl, ctx);
                b.contentType = formatContentType(b, ref contentTypeColl);

                //get blogComment if exist
                List<Blog> blogCommentCollection = null;
                int blogCommentCount = 0;
                if (b.hasComment)
                {
                    blogCommentCollection = getBlogComments(b);
                    blogCommentCount = blogCommentCollection.Count;
                }
                Console.WriteLine("Uploading blog on site");
                try
                {
                    insertBlogItem(blogCommentCount, b, ctx, oItem, b.typeOfContent);
                }
                catch (Exception ex)
                {

                    rollbackTransaction(oItem, ctx, ex.Message, b.rowID);
                    continue;
                }

                if (b.hasComment)
                {
                    Console.WriteLine("Uploading blog comment on site");
                    try
                    {
                        insertBlogComments(blogCommentCollection, ctx, oItem, b);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }
                if (b.likeCount > 0)
                {
                    Console.WriteLine("Uploading blog like on site");
                    foreach (string user in b.likedBy.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        User likedUser = resolveUser(user, ctx);
                        db.blogAnalyticsCommentLike((string.IsNullOrEmpty(b.project) ? b.channel : b.project), ctx.Web.Title, Convert.ToInt16(oItem["ID"]), "LIKE",
                   likedUser.Title, likedUser.Email, (b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published), b.categoryType);
                    }

                }
                if (b.hasAttachement)
                {
                    Console.WriteLine("Uploading attachment on site");
                    ctx.Load(oItem);
                    ctx.ExecuteQuery();
                    DataRowCollection dataRows = db.getBlogAttachments(b.siteCollection, b.subSite, b.ID).Rows;
                    foreach (DataRow dr in dataRows)
                    {
                        string filePath = dr["DownloadPath"].ToString();// System.Web.HttpUtility.UrlDecode(dr["DownloadPath"].ToString());
                        if (filePath == "")
                        {
                            if (dataRows.Count == 0)
                            {
                                Console.WriteLine("Cannot access blank path");
                                rollbackTransaction(oItem, ctx, "Cannot access blank path", b.rowID);
                                return;
                            }
                            else { continue; }
                        }
                        else if (filePath.ToLower() == "na")
                        {
                            continue;
                        }
                        FileInfo temp = new FileInfo(filePath);

                        if (isImage(temp) && (dr["AttachmentType"].ToString().ToLower().Trim() == "normal" || dr["AttachmentType"].ToString().ToLower().Trim() == "thumbnail"))
                        {
                            uploadImagesForTextBlog(ctx, filePath, temp, b.categoryType);
                            continue;
                        }
                        using (System.IO.FileStream fileStream = new System.IO.FileInfo(filePath).Open(System.IO.FileMode.Open))
                        {
                            try
                            {
                                var attachment = new AttachmentCreationInformation();
                                attachment.FileName = System.Web.HttpUtility.UrlDecode(temp.Name.Substring(temp.Name.IndexOf("_") + 1));
                                attachment.ContentStream = fileStream; // My file stream
                                Attachment att = oItem.AttachmentFiles.Add(attachment);
                                ctx.Load(att);
                                ctx.ExecuteQuery();

                            }
                            catch (Exception ex)
                            {
                                //string targetFileUrl = string.Format("{0}/Lists/DiscussionsList/Attachments/{1}/{2}", ctx.Web.ServerRelativeUrl, oItem["ID"], System.Web.HttpUtility.UrlDecode(temp.Name.Substring(temp.Name.IndexOf("_") + 1)),fileStream,true);
                                //Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, targetFileUrl, fileStream, true);
                                Console.WriteLine(ex.Message);
                                if (!ex.Message.Contains("The specified name is already in use."))
                                rollbackTransaction(oItem, ctx, ex.Message, b.rowID);
                            }

                        }
                    }
                }
            }

            // UPDATE MIGRATED SITE STATUS
            Console.WriteLine("Updatint Migrated Site Status");
            var siteName = (from site in blogUploadCollection
                            select new { siteColl = site.siteCollection, subWeb = site.subSite }).Distinct().ToList();
            foreach (var obj in siteName)
            {
                db.completeMigratedStatusForSite(obj.siteColl, obj.subWeb);
            }
            Console.WriteLine(string.Format("End {0}", DateTime.Now));
        }

        private static void setSiteUrlForLearnet(Blog b,string url)
        {            
            if (string.IsNullOrEmpty(b.siteCollection))
            {
                switch (b.subSite.ToLower())
                {
                    case "fc&a":
                        siteUrl = url+"/hydrocarbon/FCNA";
                        break;
                    case "learnet":
                        siteUrl = getLearnetUrl(b,url);
                        break;
                }
            }
            else
            {
                switch (b.subSite.ToLower())
                {

                    case "academy newsletter":
                    case "bt help page":
                    case "deep dive - fc&a":
                    case "fc&a finance":
                    case "fcna accounting":
                    case "gst":
                    case "ind-as 20:20":
                    case "kudos reward system":
                    case "news":
                    case "about fc&a academy":
                    case "quaterly kudos rewards and recognition ceremony":
                    case "useful links":
                    case "xpro chats":
                    case "budget analysis & impact 2015":
                    case "deep dive key areas":
                    case "financial management system":
                    case "international taxation":
                        siteUrl = url + "/hydrocarbon/FCNA";
                        break;
                    case "mt":
                    case "theme-month":
                    case "nh-clubs":
                    case "leadership expectations":
                        siteUrl = url + "/hydrocarbon/Leadership";
                        break;
                    case "blogs":
                    case "discussionforum":
                    case "keyblogs":
                        siteUrl = url + "/hydrocarbon/HR";
                        break;
                    case "contracting":
                    case "cost benefits":
                    case "disposal management":
                    case "gst queries":
                    case "inspection & expediting":
                    case "logistics mgmt":
                    case "materials mgmt.":
                    case "p&c bt":
                    case "p&c digital transformation":
                    case "p&c sucess stories":
                    case "procurement":
                    case "social":
                    case "tips from the leaders":
                        siteUrl = url + "/hydrocarbon/PNC";
                        break;
                    case "rtg_blogs":
                        siteUrl = url + "/hydrocarbon/RNDSocial";
                        break;
                    case "tech blog":
                    case "tech forum":
                        siteUrl = url + "/hydrocarbon/InformTech";
                        break;
                    case "my corner":
                        siteUrl = url ;
                        break;
                    default:
                        siteUrl = "Not mapped to any site";
                        break;
                }
            }
        }

        private static string getLearnetUrl(Blog b,string url)
        {
            string siteUrl = "Not mapped to any site";
            if (b.subSite.ToLower() == "learnet")
            {
                switch (b.project.ToLower())
                {
                    case "fc&a":
                        siteUrl = url + "/hydrocarbon/FCNA";
                        break;
                    case "information technology":
                        siteUrl = url + "/hydrocarbon/InformTech";
                        break;
                    case "leadership":
                        siteUrl = url + "/hydrocarbon/Leadership";
                        break;
                    case "research & development":
                        siteUrl = url + "/hydrocarbon/RNDSocial";
                        break;
                    case "human resources":
                        siteUrl = url + "/hydrocarbon/HR";
                        break;
                    case "procurement & contracting":
                        siteUrl = url + "/hydrocarbon/PNC";
                        break;
                    case "exploration & production":
                        siteUrl = url + "/Hydrocarbon/ENP";
                        break;
                    case "refining & marketing":
                        siteUrl = url + "/Hydrocarbon/RNM";
                        break;
                    case "rpmg":
                        siteUrl = url + "/Hydrocarbon/RPMG";
                        break;
                    case "rsrma":
                        siteUrl = url + "/Hydrocarbon/RSRMA";
                        break;
                    case "grca":
                        siteUrl = url + "/Hydrocarbon/GRCA";
                        break;
                    case "manufacturing":
                        siteUrl = url + "/Hydrocarbon/Manufacturing";
                        break;
                    case "petrochemicals":
                        siteUrl = url + "/Hydrocarbon/Petrochemicals";
                        break;
                    case "retail":
                        siteUrl = getRetailUrl(b,url);
                        break;
                    case "jio":
                        siteUrl = "Not mapped to any site";
                        break;
                    default:
                        siteUrl = "Not mapped to any site";
                        break;
                }
               
            }
            return siteUrl;
        }

        private static string getRetailUrl(Blog b,string url1)
        {
            string url = "Not mapped to any site";
            switch (b.channel.ToLower())
            {
                case "ajio":
                    url = url1 + "/Retail/AJIO";
                    break;
                case "digital":
                    url = url1 + "/Retail/RDIGITAL";
                    break;
                case "reliance footprints":
                    url = url1 + "/Retail/RFOOTPRINT";
                    break;
                case "reliance jewels":
                    url = url1 + "/Retail/RJEWEL";
                    break;
                case "reliance market":
                    url = url1 + "/Retail/RMART";
                    break;
                case "reliance trends":
                    url = url1 + "/Retail/RTREND";
                    break;
                case "reliance value":
                    url = url1 + "/Retail/RVALUE";
                    break;
                default:
                    break;
            }
            return url;
        }

        private static void rollbackTransaction(ListItem oItem, ClientContext ctx, string errorMessage, int dbItemID)
        {
            //Clean blogItem from List
            //get Parent list
            int rowID = (int)oItem["ID"];
            List lst = oItem.ParentList;
            ctx.Load(lst);
            oItem.DeleteObject();
            ctx.ExecuteQuery();

            // update migration db
            db.udpateParseBlogItems(dbItemID, errorMessage, "No");

            // update analytics
            db.cleanBlogID(ConfigurationManager.AppSettings["segment"].ToString(), ctx.Web.Title, rowID);

        }

        private static List<Blog> getBlogComments(Blog b)
        {
            return db.getBlogsCommentToUpload(b.siteCollection, b.subSite, b.ID);
        }

        private static void insertBlogItem(int commentCount, Blog b, ClientContext ctx, ListItem oItem, string typeOfContent)
        {
            if (typeOfContent.ToLower() == "topic")
            {
                oItem["Title"] = b.title;
                oItem["ApprovedDate"] = b.created;// (b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published);
                if (b.siteCollection.ToLower() != "hr social" && b.subSite.ToLower() != "blogs")
                {
                    oItem["KeyAreas"] = b.keyArea;
                    oItem["ContenTypeId"] = b.contentType;
                }
                oItem["_ModerationStatus"] = 0;
                oItem["TotalCommentCount"] = commentCount;
                oItem["ThumbnailPath"] = b.thumnailPath;
                if (b.moderationStatus.ToLower() == "pending")
                {
                    oItem["ApprovalStatus"] = "Pending";
                    oItem["moderatoremail"] = 1;
                    oItem["approvalrejectemail"] = 0;
                }
                else
                {
                    oItem["ApprovalStatus"] = "Approved";
                    oItem["moderatoremail"] = 1;
                    oItem["approvalrejectemail"] = 1;
                    oItem["migrated"] = 1;
                }

                if (b.categoryType.ToLower() == "v")
                {
                    oItem["FileName"] = b.filepath;
                }
            }
            else
            {
                oItem["ParentItemID"] = b.parentID;
            }

            oItem["Body"] = formatBody(b.body, ctx);
            oItem["LikesCount"] = b.likeCount;
            oItem["TotalLikeCount"] = b.likeCount;
            // oItem["LikedBy"] = b.likedBy;
            oItem["Created"] = b.created;
            oItem["Modified"] = b.modified;
            oItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(oItem);
            User userinFo = resolveUser(b.author, ctx);
            b.author = userinFo.Title;
            oItem["Author"] = userinFo;
            oItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(oItem);
            oItem["Editor"] = resolveUser(b.editor, ctx);
            oItem.Update();
            ctx.ExecuteQuery();
            ctx.Load(oItem);
            ctx.ExecuteQuery();
            int newblogID = Convert.ToInt16(oItem["ID"]);
            //Check for Invalid link in Body
            string comment = checkForInValidateLinkInBody(b.body);
            //Update DB
            db.udpateParseBlogItems(b.rowID, comment, "Yes");
            //For MyCorner
            try
            {
                if (b.siteCollection.ToLower() == "hr social" && b.subSite.ToLower() == "blogs")
                {
                    db.blogAnalytics(b, oItem, "Root", ctx.Web.Title);

                }
                else { db.blogAnalytics(b, oItem, (string.IsNullOrEmpty(b.project) ? b.channel : b.project) , ctx.Web.Title); }
            }
            catch (Exception ex)
            {
                throw;

            }



        }

        private static void insertBlogComments(List<Blog> blogCollection, ClientContext ctx, ListItem oItem, Blog b)
        {
            int migratedParentItemDBID = b.rowID;
            ctx.Load(oItem);
            ctx.ExecuteQuery();
            foreach (Blog blogItem in blogCollection)
            {
                try
                {
                    blogItem.parentID = (Int32)oItem["ID"];
                    ListItem reply = Microsoft.SharePoint.Client.Utilities.Utility.CreateNewDiscussionReply(ctx, oItem);
                    reply["Body"] = formatBody(blogItem.body, ctx);
                    //reply["LikesCount"] = blogItem.likeCount;
                    //reply["TotalLikeCount"] = blogItem.likeCount;
                    //reply["LikedBy"] = blogItem.likedBy;
                    reply["Created"] = blogItem.created;
                    reply["Modified"] = blogItem.modified;
                    reply.Update();
                    ctx.ExecuteQuery();
                    ctx.Load(reply);
                    User author = resolveUser((blogItem.author != "" ? blogItem.author : (blogItem.editor != "" ? blogItem.editor : "")), ctx);
                    reply["Author"] = author;
                    reply.Update();
                    ctx.ExecuteQuery();
                    //UpdateAnalytics for comment                
                    db.blogAnalyticsCommentLike((string.IsNullOrEmpty(b.project) ? b.channel : b.project), ctx.Web.Title, Convert.ToInt16(oItem["ID"]), "COMMENT", author.Title, author.Email, blogItem.created, b.categoryType);

                }
                catch (Exception ex)
                {

                    rollbackTransaction(oItem, ctx, ex.Message, migratedParentItemDBID);
                    throw;
                }
            }
        }
        private static void uploadImagesForTextBlog(ClientContext ctx, string filePath, FileInfo fi, string TV)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    List oDocumentList = null;
                    if (TV.ToLower() == "t")
                    {
                        oDocumentList = ctx.Web.Lists.GetByTitle("TextBlogsImages");
                    }
                    else { oDocumentList = ctx.Web.Lists.GetByTitle("VideoThumbnails"); }
                    ctx.Load(oDocumentList.RootFolder);
                    ctx.ExecuteQuery();
                    var fileUrl = String.Format("{0}/{1}", oDocumentList.RootFolder.ServerRelativeUrl, System.Web.HttpUtility.UrlDecode(fi.Name.Substring(fi.Name.IndexOf("_") + 1)));
                    Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, fileUrl, fs, true);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(string.Format("{0} : {1}", e.Message, "File Overwrite"));
            }

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
                //returnUser =  GetUserObject(ConfigurationManager.AppSettings["defaultUser"].ToString());                
                returnUser = ctx.Web.EnsureUser(ConfigurationManager.AppSettings["defaultUser"].ToString());
                ctx.Load(returnUser);
                ctx.ExecuteQuery();
            }


            return returnUser;
        }

        private static User GetUserObject(string userValue)
        {
            ClientContext ctx = new ClientContext(@"http://sp13devwfe01:8623");
            Web oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();
            User user = ctx.Web.EnsureUser(userValue);
            ctx.Load(user);
            ctx.ExecuteQuery();
            return user;
        }

        private static void prepareWebContext(out ClientContext ctx, out Web oWeb)
        {
            ctx = new ClientContext(siteUrl);
            oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();

        }
        private static string formatContentType(Blog b, ref List<Common.ContentType> dbContentTypeColl)
        {
            if (b.contentType == "")
                return string.Empty;
            if(b.contentType.ToLower() == "general")
            {
                b.contentType = "Knowledge Sharing";
            }
        Start:
            var contenttypeID = (from content in dbContentTypeColl
                                 where content.title == b.contentType
                                 select new { content.rowID }).FirstOrDefault();

            if (contenttypeID == null)
            {
                //Update Analytics DB with contenttype
                db.updateContentType(b.contentType);
                dbContentTypeColl = db.getContentType();
                goto Start;
            }
            return contenttypeID.rowID.ToString();

        }

        private static string formatKeyArea(Blog b, ref List<KeyArea_Channel> dbkeyAreaColl, ClientContext ctx)
        {
            if (b.keyArea == "")
                return string.Empty;
            //string segment = (b.project.ToLower() =="retail" ? "Retail" : "Hydrocarbon");
            string segment = "hydro";
            segment = (b.project.ToLower() == "retail" ? "Retail" : "Hydrocarbon");
        Start:
            var keyareaID = (from keyArea in dbkeyAreaColl
                             where keyArea.segment == segment && keyArea.channel == ctx.Web.Title && keyArea.title == b.keyArea
                             select new { keyArea.rowID }).FirstOrDefault();
            if (keyareaID == null)
            {
                //Update Analytics DB with key area
                db.updateKeyArea(b.keyArea, segment, ctx.Web.Title);
                dbkeyAreaColl = db.getKeyArea_Channel();
                goto Start;

            }
            return keyareaID.rowID.ToString();


        }

        private static bool isImage(FileInfo temp)
        {
            foreach (string ext in ConfigurationManager.AppSettings["ImageFormat"].ToString().Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (temp.Name.Substring(temp.Name.LastIndexOf(".") + 1).ToLower().Contains(ext))
                {
                    return true;
                }
            }
            return false;
        }
        private static string checkForInValidateLinkInBody(string body)
        {
            string pattern = @"<a.*?href=""(?<url>.*?)"".*?>(?<filename>.*?)</a>";
            Regex rx = new Regex(pattern);
            MatchCollection mc = rx.Matches(body.Replace("'", "\""));
            string newBody = body.Replace("'", "\"");
            if (mc.Count > 0)
            {
                foreach (Match m in mc)
                {

                    if (m.Groups["url"].Value.ToLower().Contains(".aspx"))
                    {
                        return "Problematic link";
                    }
                    if (m.Groups["filename"].Value.ToLower().Contains("ril.com") && !m.Groups["url"].Value.ToLower().Contains("mailto"))
                    {
                        return "Problematic link text value";
                    }
                }
            }
            return "";
        }

        private static string formatBody(string body, ClientContext ctx)
        {
            //string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|<a.*?href=""(?<url>.*?)"".*?>";//*?</a>";
            string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|(<a.*?href=""(?<url>.*?)"".*?><img.*?src=""(?<url>.*?)"".*?>(?<filename>.*?)</a>)|<a.*?href=""(?<url>.*?)"".*?><img.*?src=""(?<url>.*?)"".*?>(?<filename>.*?)</a>";
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

        public static void tempFunc()
        {
            SqlConnection con = new SqlConnection(ConfigurationManager.AppSettings["Connection"]);
            ClientContext ctx = new ClientContext(siteUrl);
            Web oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();

            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = @"select pbd.BlogID , mled.DownloadPath from [LearnetUsage].[dbo].[BlogDetails] pbd inner join 
                                BLOGDETAILS bd 
                                on pbd.BlogTitle=bd.title
                                inner join MigratedListItemEmbededDocument mled on bd.BlogID=mled.DocumentParentRowID and bd.SiteCollection=mled.SiteCollection
                                where bd.sitecollection ='R&D - Social Community' and typeOfContent='topic' and migrated='yes' and hasAttachement=1 and Filepath like '%pdf'
                                and  pbd.Segment_Channel_ID=9";
            com.CommandType = CommandType.Text;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            com.Connection.Open();
            adp.Fill(dt);
            com.Connection.Close();
            foreach (DataRow dr in dt.Rows)
            {

                ListItem oItem = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(Convert.ToInt16(dr["BlogID"]));
                ctx.Load(oItem);
                ctx.ExecuteQuery();
                string filePath = dr["DownloadPath"].ToString();// System.Web.HttpUtility.UrlDecode(dr["DownloadPath"].ToString());
                FileInfo temp = new FileInfo(filePath);

                if (isImage(temp))
                {
                    uploadImagesForTextBlog(ctx, filePath, temp, "t");
                    continue;
                }
                using (System.IO.FileStream fileStream = new System.IO.FileInfo(filePath).Open(System.IO.FileMode.Open))
                {
                    try
                    {
                        var attachment = new AttachmentCreationInformation();
                        attachment.FileName = System.Web.HttpUtility.UrlDecode(temp.Name.Substring(temp.Name.IndexOf("_") + 1));
                        attachment.ContentStream = fileStream; // My file stream
                        Attachment att = oItem.AttachmentFiles.Add(attachment);
                        ctx.Load(att);
                        ctx.ExecuteQuery();
                        string targetFileUrl = string.Format("{0}/Lists/DiscussionText/Attachments/{1}/{2}", ctx.Web.ServerRelativeUrl, oItem["ID"], System.Web.HttpUtility.UrlDecode(temp.Name.Substring(temp.Name.IndexOf("_") + 1)));
                        Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, targetFileUrl, fileStream, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //rollbackTransaction(oItem, ctx, ex.Message,b.rowID);
                    }

                }
            }
        }
    }
}
