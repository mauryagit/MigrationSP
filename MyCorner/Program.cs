using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace MyCorner
{
    class Program
    {
        static DBM db = new DBM();
        static string siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
        static void Main(string[] args)
        {
            Console.Title = "MyCorner Blog";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            //Get Collection
            List<KeyArea_Channel> keyAreaChannelColl = db.getKeyArea_Channel();
            List<Common.ContentType> contentTypeColl = db.getContentType();
            List<Blog> blogUploadCollection = db.getRecordForMyCorner();

            ClientContext ctx;
            Web oWeb;
            foreach (Blog b in blogUploadCollection)
            {

                siteUrl = ConfigurationManager.AppSettings["SiteUrl"];


                Console.WriteLine(string.Format("Uploading Blog ID {0}", b.ID));
                prepareWebContext(out ctx, out oWeb);
                // Console.WriteLine(formatBody(b.body,ctx));
                List olist = null;
                if (b.categoryType.ToLower() == "t")
                {
                    olist = oWeb.Lists.GetByTitle("MyCorner");
                }
                else
                {
                    olist = oWeb.Lists.GetByTitle("Discussions List");
                }
                ListItemCreationInformation createItem = new ListItemCreationInformation();
                ListItem oItem = olist.AddItem(createItem);

                // parse keyArea and contenttype
                b.keyArea = "462";
                b.contentType = "5";

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
                        db.blogAnalyticsCommentLike("Root", ctx.Web.Title, Convert.ToInt16(oItem["ID"]), "LIKE",
                   likedUser.Title, likedUser.Email, (b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published), b.categoryType);
                    }

                }
                if (b.hasAttachement)
                {
                    Console.WriteLine("Uploading attachment on site");
                    ctx.Load(oItem);
                    ctx.ExecuteQuery();
                    foreach (DataRow dr in db.getBlogAttachments(b.siteCollection, b.subSite, b.ID).Rows)
                    {
                        string filePath = dr["DownloadPath"].ToString();// System.Web.HttpUtility.UrlDecode(dr["DownloadPath"].ToString());
                        if (filePath == "")
                        {
                            Console.WriteLine("Cannot access blank path");
                            rollbackTransaction(oItem, ctx, "Cannot access blank path", b.rowID);
                            return;
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
                                rollbackTransaction(oItem, ctx, ex.Message, b.rowID);
                            }

                        }
                    }
                }
               // db.udpateParseBlogItems(b.rowID, comment, "Yes");
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

        private static void setSiteUrlForLearnet(Blog b)
        {
            if (b.subSite.ToLower() == "learnet")
            {
                switch (b.project.ToLower())
                {
                    case "fc&a":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/FCNA";
                        break;
                    case "information technology":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/InformTech";
                        break;
                    case "leadership":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/Leadership";
                        break;
                    case "research & development":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/RNDSocial";
                        break;
                    case "human resources":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/HR";
                        break;
                    case "procurement & contracting":
                        siteUrl = "http://sp13devwfe01:46809/sites/hydrocarbon/PNC";
                        break;
                    case "exploration & production":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/ENP";
                        break;
                    case "refining & marketing":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/RNM";
                        break;
                    case "rpmg":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/RPMG";
                        break;
                    case "rsrma":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/RSRMA";
                        break;
                    case "grca":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/GRCA";
                        break;
                    case "manufacturing":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/Manufacturing";
                        break;
                    case "petrochemicals":
                        siteUrl = "http://sp13devwfe01:46809/sites/Hydrocarbon/Petrochemicals";
                        break;
                    default:
                        siteUrl = "Not mapped to any site";
                        break;
                }
            }
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
                oItem["ApprovedDate"] = (b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published);
                if (b.siteCollection.ToLower() != "" && b.subSite.ToLower() != "fc&a")
                {
                    //oItem["KeyAreas"] = b.keyArea;
                    //oItem["ContenTypeId"] = b.contentType;
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
                //if (b.siteCollection.ToLower() == "" && b.subSite.ToLower() == "fc&a")
                //{
                db.blogAnalytics(b, oItem, "Root", ctx.Web.Title);

                //}
                //else { db.blogAnalytics(b, oItem, ConfigurationManager.AppSettings["segment"].ToString(), ctx.Web.Title); }
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
                    db.blogAnalyticsCommentLike("root", ctx.Web.Title, Convert.ToInt16(oItem["ID"]), "COMMENT", author.Title, author.Email, blogItem.created, b.categoryType);

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
                returnUser = ctx.Web.EnsureUser(ConfigurationManager.AppSettings["defaultUser"].ToString());
                ctx.Load(returnUser);
                ctx.ExecuteQuery();
            }


            return returnUser;
        }

        private static void prepareWebContext(out ClientContext ctx, out Web oWeb)
        {
            ctx = new ClientContext(siteUrl);
            oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();

        }
        private static string formatContentType(string contenttype, ref List<Common.ContentType> dbContentTypeColl)
        {
            if (contenttype == "")
                return string.Empty;
        Start:
            var contenttypeID = (from content in dbContentTypeColl
                                 where content.title == contenttype
                                 select new { content.rowID }).FirstOrDefault();

            if (contenttypeID == null)
            {
                //Update Analytics DB with contenttype
                db.updateContentType(contenttype);
                dbContentTypeColl = db.getContentType();
                goto Start;
            }
            return contenttypeID.rowID.ToString();

        }

        private static string formatKeyArea(string keyAreaName, ref List<KeyArea_Channel> dbkeyAreaColl, ClientContext ctx)
        {
            if (keyAreaName == "")
                return string.Empty;
        Start:
            var keyareaID = (from keyArea in dbkeyAreaColl
                             where keyArea.segment == ConfigurationManager.AppSettings["segment"].ToString() && keyArea.channel == ctx.Web.Title && keyArea.title == keyAreaName
                             select new { keyArea.rowID }).FirstOrDefault();
            if (keyareaID == null)
            {
                //Update Analytics DB with key area
                db.updateKeyArea(keyAreaName, ConfigurationManager.AppSettings["segment"].ToString(), ctx.Web.Title);
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
    }
}
