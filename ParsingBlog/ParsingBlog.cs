using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParsingBlog
{
    class ParsingBlog
    {
        static string libraryName = ConfigurationManager.AppSettings["documentLibrary"];
        static void Main(string[] args)
        {
            //
            Console.Title = "Parsing Blog";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            DBM db = new DBM();
            DataTable dtSite = null;
            if (args.Length == 0)
            {
                dtSite = db.getSiteToMigrated("LDATAM");
               // dtSite = db.getSpecficSiteToMigrated("LDATAM", "Discover Reliance");
            }
            else
            {
                dtSite = db.getSpecficSiteToMigrated("LDATAM", args[0].ToString());
                
            }

            foreach (DataRow dr in dtSite.Rows)
            {
               // if (dr["SubSite"].ToString() != "Learnet") { continue; }
                DataTable dtBlogCollumn = db.getListDefination(dr["SiteCollection"].ToString(), dr["SubSite"].ToString(), dr["ListName"].ToString());
                DataTable dtBlogData = db.getDataofBlog(dr["SiteCollection"].ToString(), dr["SubSite"].ToString(), dr["ListName"].ToString());
                DataTable dtMappingDetails = db.getMappingDetails(dr["SiteUrl"].ToString());
                string insertStatementforBlog = string.Empty;
                string insertStatementforEmbedbedDocument = string.Empty;
                List<Blog> blogCollectionlst = new List<Blog>();
                foreach (DataRow blogRow in dtBlogData.Rows)
                {
                    string embeddocumentStatement = @"INSERT INTO [MigratedListItemEmbededDocument] (SiteCollection,SubSite,ListDefinationRowID,DocumentParentRowID
                                                      ,Filepath,Downloadable,AttachmentType) values(";
                    int collumnCounter = 4;
                    Blog newBlog = new Blog();
                    newBlog.channel = "Hydrocarbon";
                    for (int i = 2; i <= dtBlogData.Columns.Count - 1; i++)
                    {
                        newBlog.siteCollection = dtBlogCollumn.Rows[0][1].ToString();
                        newBlog.subSite = dtBlogCollumn.Rows[0][2].ToString();
                        #region "Switch case"
                        if (newBlog.subSite.ToLower() == "rtg_blogs")
                        {
                            NormalSwitchForRNDSocial(dtBlogCollumn, blogRow, collumnCounter, ref newBlog, i);
                        }
                        else if (newBlog.subSite.ToLower() == "learnet")
                        {
                            NormalSwitchForLearnet(dtBlogCollumn, blogRow, collumnCounter, ref newBlog, i);
                        }
                        else
                        {
                            NormalSwitch(dtBlogCollumn, blogRow, collumnCounter, ref newBlog, i);
                        }

                        #endregion
                        collumnCounter++;
                    }
                    //if (newBlog.ID != 163) { continue; }
                    if (newBlog.subSite.ToLower() != "learnet")
                    {
                        setKeyAreaAndContentType(newBlog, dtMappingDetails, dtBlogCollumn.Rows[0][3].ToString());
                        newBlog.categoryType = "T";
                    }
                    //Set typeOfCategory video or text
                    if (newBlog.subSite.ToLower() == "learnet")
                    {
                         if (dr["ListName"].ToString().ToLower() == "discussions list")
                             newBlog.categoryType="V";
                        else
                             newBlog.categoryType = "T";

                    }
                    insertStatementforEmbedbedDocument += extractEmbedDocuments(newBlog, embeddocumentStatement, dtBlogCollumn.Rows[0][0].ToString());
                    blogCollectionlst.Add(newBlog);
                }

                db.insertParseBlogItems(Extension.ToDataTable(blogCollectionlst));
                db.insertListItemDocumentDownloadStatus(insertStatementforEmbedbedDocument);
                //SET COMMENT STATUS
                db.setCommentStatusForBlog(dr["SiteCollection"].ToString(), dr["SubSite"].ToString());
            }

            Console.WriteLine(string.Format("End {0}", DateTime.Now));

        }

        private static void NormalSwitchForLearnet(DataTable dtBlogCollumn, DataRow blogRow, int collumnCounter, ref Blog newBlog, int i)
        {
            switch (dtBlogCollumn.Rows[0][collumnCounter].ToString().Split(new string[] { "$" }, StringSplitOptions.None)[0].ToLower())
            {
                case "id":
                    newBlog.ID = Convert.ToInt16(blogRow[i].ToString());
                    break;
                case "title":
                    newBlog.title = blogRow[i].ToString();
                    newBlog.typeOfContent = (blogRow[i] != null && blogRow[i].ToString() != "" ? "Topic" : "Reply");
                    break;
                case "body":
                    newBlog.body = blogRow[i].ToString();
                    break;
                case "numcomments":
                    // newBlog.hasComment = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "postcategory":
                case "categorieslookup":
                    newBlog.categories = (blogRow[i] == null ? "" : blogRow[i].ToString());
                    newBlog.contentType = (blogRow[i] == null ? "" : blogRow[i].ToString());
                    break;
                case "publisheddate":
                    newBlog.published = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "likescount":
                    newBlog.likeCount = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "likedby":
                    newBlog.likedBy = blogRow[i].ToString();
                    break;
                case "modified":
                       newBlog.modified =Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "created":
                    newBlog.created = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                
                case "memberlookup":
                    newBlog.author = blogRow[i].ToString();
                    break;
               
                case "myeditor":
                    newBlog.editor = blogRow[i].ToString();
                    break;
                case "approvalstatus":
                case "status":
                    newBlog.moderationStatus = blogRow[i].ToString();
                    break;
                case "parentfolderid":
                    // newBlog.typeOfContent = (blogRow[i] == null  ? "Topic" : "Reply");
                    newBlog.parentID = (blogRow[i] == null || blogRow[i] == "" ? 0 : Convert.ToInt16(blogRow[i].ToString()));
                    break;
                case "attachments":
                    newBlog.attachementFiles = blogRow[i].ToString();
                    break;
                case "key_x0020_area_x003a_title":
                case "key_x0020_area":
                    newBlog.keyArea = blogRow[i].ToString();
                    break;
                case "thumbnailpath":
                    newBlog.thumnailPath = blogRow[i].ToString();
                    break;
                case "project":
                case "project_x003a_title":
                    newBlog.project = blogRow[i].ToString();                   
                    break;   
                case "filename":
                    newBlog.filepath = blogRow[i].ToString();                   
                    break;
            }
        }

        private static void NormalSwitch(DataTable dtBlogCollumn, DataRow blogRow, int collumnCounter, ref Blog newBlog, int i)
        {
            switch (dtBlogCollumn.Rows[0][collumnCounter].ToString().Split(new string[] { "$" }, StringSplitOptions.None)[0].ToLower())
            {
                case "id":
                    newBlog.ID = Convert.ToInt16(blogRow[i].ToString());
                    break;
                case "title":
                    newBlog.title = blogRow[i].ToString();
                    newBlog.typeOfContent = (blogRow[i] != null && blogRow[i].ToString() != "" ? "Topic" : "Reply");
                    break;
                case "body":
                    newBlog.body = blogRow[i].ToString();
                    break;
                case "numcomments":
                    // newBlog.hasComment = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "postcategory":
                case "categorieslookup":
                    newBlog.categories = (blogRow[i] == null ? "" : blogRow[i].ToString());
                    break;
                case "publisheddate":
                    newBlog.published = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "likescount":
                    newBlog.likeCount = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "likedby":
                    newBlog.likedBy = blogRow[i].ToString();
                    break;
                case "modified":
                    newBlog.modified = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "created":
                    newBlog.created = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "author":
                    newBlog.author = blogRow[i].ToString();
                    break;
                case "editor":
                    newBlog.editor = blogRow[i].ToString();
                    break;
                case "_moderationstatus":
                    newBlog.moderationStatus = (blogRow[i].ToString() == "0" ? "Approved" : "Rejected");
                    break;
                case "parentfolderid":
                    // newBlog.typeOfContent = (blogRow[i] == null  ? "Topic" : "Reply");
                    newBlog.parentID = (blogRow[i] == null || blogRow[i] == "" ? 0 : Convert.ToInt16(blogRow[i].ToString()));
                    break;
                case "attachments":
                    newBlog.attachementFiles = blogRow[i].ToString();
                    break;
            }
        }
        private static void NormalSwitchForRNDSocial(DataTable dtBlogCollumn, DataRow blogRow, int collumnCounter, ref Blog newBlog, int i)
        {
            switch (dtBlogCollumn.Rows[0][collumnCounter].ToString().Split(new string[] { "$" }, StringSplitOptions.None)[0].ToLower())
            {
                case "id":
                    newBlog.ID = Convert.ToInt16(blogRow[i].ToString());
                    break;
                case "title":
                    newBlog.title = blogRow[i].ToString();
                    newBlog.typeOfContent = (blogRow[i] != null && blogRow[i].ToString() != "" ? "Topic" : "Reply");
                    break;
                case "body":
                    newBlog.body = blogRow[i].ToString();
                    break;
                case "numcomments":
                    // newBlog.hasComment = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "postcategory":
                case "categorieslookup":
                    newBlog.categories = (blogRow[i] == null ? "" : blogRow[i].ToString());
                    break;
                case "publisheddate":
                    newBlog.published = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "likescount":
                    newBlog.likeCount = Convert.ToInt16((blogRow[i].ToString() == "" ? "0" : blogRow[i].ToString()));
                    break;
                case "likedby":
                    newBlog.likedBy = blogRow[i].ToString();
                    break;
                case "modified":
                    newBlog.modified = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "created":
                    newBlog.created = Convert.ToDateTime(blogRow[i].ToString());
                    break;
                case "author":
                    newBlog.author = blogRow[i].ToString();
                    break;
                case "editor":
                    newBlog.editor = blogRow[i].ToString();
                    break;
                case "approval":
                    newBlog.moderationStatus = blogRow[i].ToString();
                    break;
                case "parentfolderid":
                    // newBlog.typeOfContent = (blogRow[i] == null  ? "Topic" : "Reply");
                    newBlog.parentID = (blogRow[i] == null || blogRow[i] == "" ? 0 : Convert.ToInt16(blogRow[i].ToString()));
                    break;
                case "attachments":
                    newBlog.attachementFiles = blogRow[i].ToString();
                    break;
            }
        }
        static void setKeyAreaAndContentType(Blog blog, DataTable mappingDetail, string typeOfList)
        {
            if (typeOfList.ToLower() != "posts" && (blog.typeOfContent == null || blog.typeOfContent.ToLower() == "reply"))
                return;

            if (blog.siteCollection.ToLower() == "p&c academy" || blog.siteCollection.ToLower() == "discover reliance" || (blog.siteCollection.ToLower() == "hr social" && blog.subSite.ToLower() == "blogs"))
            {
                blog.categories = blog.subSite;            
            }
            if (blog.siteCollection.ToLower() == "fc&a" && blog.subSite.ToLower() == "fcna accounting")
            {
                blog.categories = "Accounting";
            }
            if (blog.categories == "")
            {
                //DataRow[] rows = mappingDetail.Select("[Old_Category] is null");
                //blog.keyArea = rows[0]["KeyArea"].ToString().Trim();
                //blog.contentType = rows[0]["ContentType"].ToString().Trim();
            }

            //else
            //{
            //    string[] categories = blog.categories.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            //    //if (blog.siteCollection.ToLower() == "p&c academy")
            //    //{
            //    //    categories[0] = blog.subSite;
            //    //}
            //    int currentCounter = 0;
            //    foreach (string category in categories)
            //    {
            //        currentCounter += 1;
            //        DataRow[] rows = mappingDetail.Select("TRIM([Old_Category]) = '" + category + "'");
            //        if (rows.Length > 0)
            //        {
            //            blog.keyArea = rows[0]["KeyArea"].ToString().Trim();
            //            blog.contentType = rows[0]["ContentType"].ToString().Trim();
            //            return;
            //        }
            //        else
            //        {
            //            if (currentCounter == categories.Length)
            //            {
            //                DataRow[] nullRows = mappingDetail.Select("[Old_Category] is null OR [Old_Category] = ''");
            //                blog.keyArea = nullRows[0]["KeyArea"].ToString().Trim();
            //                blog.contentType = nullRows[0]["ContentType"].ToString().Trim();
            //            }
            //        }
            //    }
            //}
        }

        static string extractEmbedDocuments(Blog blog, string appendStatement, string listDefinationrowid)
        {

            string placeHolder = "'{0}','{1}',{2},{3},'{4}',{5},'{6}') ";
            string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|<a.*?href=""(?<url>.*?)"".*?>";
            // string pattern = @"<img.*?src=""(?<url>.*?)"".*?>|<a.*?href=""(?<url>.*?)"".*?><img.*?src=""(?<url>.*?)"".*?>(?<filename>.*?)</a>";
            string query = string.Empty;
            Regex rx = new Regex(pattern);
            foreach (Match m in rx.Matches(blog.body))
            {
                string replacementPath = string.Format("/{0}/{1}/", blog.subSite, libraryName);
                string internalPattern = @"^(http|https)://|^(/_layouts)|^(mailto)|^(http|https).*;//";
                Regex childrx = new Regex(internalPattern);
                MatchCollection mc = childrx.Matches(m.Groups["url"].Value);
                //if (!m.Groups["url"].Value.ToLower().Contains("_layouts"))
                if (mc.Count == 0)
                {
                    blog.hasAttachement = true;
                    if (m.Groups["url"].Value != "/" && !m.Groups["url"].Value.ToLower().Contains("#"))
                    {
                        if (m.Groups["url"].Value.ToLower().Contains("_layouts") || m.Groups["url"].Value.ToLower().Contains("?rootfolder=") || m.Groups["url"].Value.ToLower().Contains("other-user-profile.aspx"))
                        {
                            query += appendStatement + String.Format(placeHolder, blog.siteCollection, blog.subSite, listDefinationrowid, blog.ID, m.Groups["url"].Value, 0, "Normal");
                        }
                        else
                        {
                            query += appendStatement + String.Format(placeHolder, blog.siteCollection, blog.subSite, listDefinationrowid, blog.ID, m.Groups["url"].Value, 1, "Normal");
                        }
                    }
                }

            }
            //Blog Attachment
            if (!string.IsNullOrEmpty(blog.attachementFiles))
            {
                foreach (string attachs in blog.attachementFiles.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    blog.hasAttachement = true;
                    query += appendStatement + String.Format(placeHolder, blog.siteCollection, blog.subSite, listDefinationrowid, blog.ID, attachs, 1, "Attach");
                }
            }
            //Thumnail
            if (!string.IsNullOrEmpty(blog.thumnailPath))
            {
                blog.hasAttachement = true;
                query += appendStatement + String.Format(placeHolder, blog.siteCollection, blog.subSite, listDefinationrowid, blog.ID, blog.thumnailPath, 1, "Thumbnail");
            }
            return query;
        }

    }



}
