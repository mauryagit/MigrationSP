using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DBM : IDisposable
    {
       // SqlConnection con = new SqlConnection("Data Source=10.21.52.109;Initial Catalog=RILSocialPlatformMigration;Persist Security Info=True;User ID=usagestats; Password=usage@stats2;");
       // SqlConnection con = new SqlConnection("Data Source=10.21.52.109;Initial Catalog=RILSocialPlatformMigration_05012018;Persist Security Info=True;User ID=usagestats; Password=usage@stats2;");
        SqlConnection con = new SqlConnection("Data Source=10.21.52.109;Initial Catalog=RILSocialPlatformMigration_09052018;Persist Security Info=True;User ID=usagestats; Password=usage@stats2;");
        //SqlConnection analytics = new SqlConnection("Data Source=10.21.52.109;Initial Catalog=LearnetUsage_Prod;Persist Security Info=True;User ID=usagestats; Password=usage@stats2;");
        SqlConnection analytics = new SqlConnection("Data Source=Sidcsp13dbip;Initial Catalog=LearnetUsage;Persist Security Info=True;User ID=usagestats; Password=usage@123;");

        public DataTable getListItemDocuments(string parentWeb, string web)
        {
            DataTable dt = new DataTable();

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OPERATIONONMigratedListItemEmbededDocument";
            com.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            foreach (var column in dt.Columns.Cast<DataColumn>().ToArray())
            {
                if (dt.AsEnumerable().All(dr => dr.IsNull(column)))
                    dt.Columns.Remove(column);
            }
            return dt;
        }
        public DataTable getListItemDocumentsForSpecificWeb(string parentWeb)
        {
            DataTable dt = new DataTable();

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OPERATIONONMigratedListItemEmbededDocument";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 50).Value = parentWeb;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 50).Value = parentWeb;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            foreach (var column in dt.Columns.Cast<DataColumn>().ToArray())
            {
                if (dt.AsEnumerable().All(dr => dr.IsNull(column)))
                    dt.Columns.Remove(column);
            }
            return dt;
        }
        public void updateListItemDocumentDownloadStatus(string documentrowid, string path, string parentid)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OPERATIONONMigratedListItemEmbededDocument";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "UPDATE";
            com.Parameters.Add("@PARENTID", SqlDbType.Int).Value = parentid;
            com.Parameters.Add("@DOWNLOADEDPATH", SqlDbType.NVarChar, -1).Value = path;
            com.Parameters.Add("@UPDATEID", SqlDbType.Int).Value = documentrowid;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public void insertListItemDocumentDownloadStatus(string insertStatement)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OPERATIONONMigratedListItemEmbededDocument";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "INSERT";
            com.Parameters.Add("@INSERTSTATEMENT", SqlDbType.NVarChar, -1).Value = insertStatement;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public void setCommentStatusForBlog(string sitecoll, string subsite)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "SETCOMMENTSTATUSFORBLOG";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@sitecollection", SqlDbType.NVarChar, 100).Value = sitecoll;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = subsite;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public void insertParseBlogItems(DataTable table)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationONBlogDetails]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "INSERT";
            //com.Parameters.Add("@INSERTSTATEMENT", SqlDbType.NVarChar, -1).Value = insertStatement;
            com.Parameters.Add("@BLGTBL", SqlDbType.Structured).Value = table;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = table.Rows[0][0].ToString();
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 100).Value = table.Rows[0][1].ToString();
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public void udpateParseBlogItems(int rowID, string migrationComment, string migrationStatus)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationONBlogDetails]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "UDPATE";
            //com.Parameters.Add("@INSERTSTATEMENT", SqlDbType.NVarChar, -1).Value = insertStatement;
            com.Parameters.Add("@STATUS", SqlDbType.NChar, 3).Value = migrationStatus;
            com.Parameters.Add("@UPDATEDID", SqlDbType.Int).Value = rowID;
            com.Parameters.Add("@MIGRATIONCOMMENT", SqlDbType.NVarChar, -1).Value = migrationComment;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public void createListSchema(string insertstatement, string sitecoll, string subsite, string listName)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedListDefination";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@sitecollection", SqlDbType.NVarChar, 100).Value = sitecoll;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = subsite;
            com.Parameters.Add("@listname", SqlDbType.NVarChar, 100).Value = listName;
            com.Parameters.Add("@insertQuery", SqlDbType.NVarChar, -1).Value = insertstatement;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public DataTable getMappingDetails(string siteUrl)
        {
            DataTable dt = new DataTable();
            using (SqlCommand com = new SqlCommand())
            {
                com.CommandType = CommandType.StoredProcedure;
                com.Connection = con;
                com.CommandTimeout = 0;
                com.CommandText = "GetSiteMappingDetails";
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.Add("@siteUrl", SqlDbType.NVarChar, 100).Value = siteUrl;
                SqlDataAdapter adp = new SqlDataAdapter(com);
                if (com.Connection.State != ConnectionState.Open)
                {
                    com.Connection.Open();
                }
                adp.Fill(dt);
                com.Connection.Close();
            }
            return dt;

        }



        public List<Blog> getBlogsToUpload(string sitecollection,string type)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "GETFINALBLOGTOUPLOAD";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = sitecollection;
            com.Parameters.Add("@Type", SqlDbType.NVarChar, 10).Value = type;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            //return dt;

            List<Blog> blogCollection = dt.AsEnumerable().Select(m => new Blog()
            {
                author = m.Field<string>("author"),
                body = m.Field<string>("body"),
                created = m.Field<DateTime>("created"),
                editor = m.Field<string>("editor"),
                likeCount = m.Field<Int32>("likeCount"),
                likedBy = m.Field<string>("likedBy"),
                modified = m.Field<DateTime>("modified"),
                published = m.Field<DateTime>("created"),
                siteCollection = m.Field<string>("siteCollection"),
                subSite = m.Field<string>("subSite"),
                ID = m.Field<Int32>("BlogID"),
                title = m.Field<string>("title"),
                keyArea = m.Field<string>("keyArea"),
                contentType = m.Field<string>("contentType"),
                typeOfContent = m.Field<string>("typeOfContent"),
                hasComment = m.Field<bool>("hasComment"),
                hasAttachement = m.Field<bool>("hasAttachement"),
                rowID = m.Field<int>("ID"),
                moderationStatus = m.Field<string>("moderationStatus"),
                categoryType = m.Field<string>("Categorytype"),
                thumnailPath = m.Field<string>("ThumnailPath"),
                project = m.Field<string>("project"),
                filepath = m.Field<string>("filepath"),
                channel=m.Field<string>("Channel"),
                categories=m.Field<string>("categories")

            }).ToList();

            return blogCollection;
        }

        public DataTable getBlogAttachments(string sitecollection, string subsite, int parentID)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "GETBLOGATTACHMENTS";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = sitecollection;
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 100).Value = subsite;
            com.Parameters.Add("@PARENTID", SqlDbType.Int).Value = parentID;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }
        public List<Blog> getBlogsCommentToUpload(string sitecollection, string subsite, int parentID)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "[GetBlogComments]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = sitecollection;
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 100).Value = subsite;
            com.Parameters.Add("@PARENTID", SqlDbType.Int).Value = parentID;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            List<Blog> blogCommentCollection = dt.AsEnumerable().Select(m => new Blog()
            {
                author = m.Field<string>("author"),
                body = m.Field<string>("body"),
                created = m.Field<DateTime>("created"),
                editor = m.Field<string>("editor"),
                likeCount = m.Field<Int32>("likeCount"),
                likedBy = m.Field<string>("likedBy"),
                modified = m.Field<DateTime>("modified"),
                siteCollection = m.Field<string>("siteCollection"),
                subSite = m.Field<string>("subSite")


            }).ToList();

            return blogCommentCollection;


        }
        public DataTable getListDefination(string parentWeb, string web, string listname)
        {
            DataTable dt = new DataTable();
            //string selectStatement = "Select * from [dbo].[MigratedListDefination] where sitecollection ='" + parentWeb + "' and subsite='" + web + "'";
            string selectStatement = "Select * from [dbo].[MigratedListDefination] where sitecollection =@parentWeb and subsite=@web and ListName=@listname";
            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = selectStatement;
                com.CommandType = CommandType.Text;
                com.Connection = con;
                com.CommandTimeout = 0;
                com.Parameters.Add("@parentWeb", SqlDbType.NVarChar, 100).Value = parentWeb;
                com.Parameters.Add("@web", SqlDbType.NVarChar, 100).Value = web;
                com.Parameters.Add("@listname", SqlDbType.NVarChar, 100).Value = listname;
                SqlDataAdapter adp = new SqlDataAdapter(com);
                if (com.Connection.State != ConnectionState.Open)
                {
                    com.Connection.Open();
                }
                adp.Fill(dt);
                com.Connection.Close();
                foreach (var column in dt.Columns.Cast<DataColumn>().ToArray())
                {
                    if (dt.AsEnumerable().All(dr => dr.IsNull(column)))
                        dt.Columns.Remove(column);
                }
            }
            return dt;
        }

        public void createListItem(string insertstatement, string checkstatement)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedListItem";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@wherequery", SqlDbType.NVarChar, -1).Value = checkstatement;
            com.Parameters.Add("@insertQuery", SqlDbType.NVarChar, -1).Value = insertstatement;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public void createListItemComment(string listDefinationid, string listDataItemid, string body,
            string author, string created, string comment)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedListDataComment";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@ListDefinationid", SqlDbType.Int).Value = listDefinationid;
            com.Parameters.Add("@ListDataItemid ", SqlDbType.Int).Value = listDataItemid;
            com.Parameters.Add("@body", SqlDbType.NVarChar, -1).Value = body;
            com.Parameters.Add("@author", SqlDbType.NVarChar, 150).Value = author;
            com.Parameters.Add("@created", SqlDbType.DateTime).Value = Convert.ToDateTime(created);
            com.Parameters.Add("@commentid", SqlDbType.Int).Value = comment;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public void createSiteMigratedStructure(string sitecoll, string subsite, string siteurl, string migrationstatus)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedSiteDetails";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@siteCollection", SqlDbType.NVarChar, 100).Value = sitecoll;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = subsite;
            com.Parameters.Add("@siteurl", SqlDbType.NVarChar, 100).Value = siteurl;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = "insert";
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }

        public void udpateSiteMigratedStatus(string siteurl, string migrationstatus)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedSiteDetails";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@siteCollection", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@siteurl", SqlDbType.NVarChar, 100).Value = siteurl;
            com.Parameters.Add("@migrationStatus", SqlDbType.NVarChar, 100).Value = migrationstatus;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = "update";
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public void completeMigratedStatusForSite(string sitecollection, string subsite)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "UPDATEMIGRATIONSTATUSFORSITE";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = sitecollection;
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 100).Value = subsite;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }
        public DataTable getDummysiteToMigrated()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SiteCollection");
            dt.Columns.Add("Subsite");
            dt.Columns.Add("SiteUrl");
            DataRow dr = dt.NewRow();
            dr["SiteCollection"] = "";
            dr["Subsite"] = "FC&A";
            dr["SiteUrl"] = "https://fcna.ril.com";
            dt.Rows.Add(dr);
            return dt;

        }


        public DataTable getSiteToMigrated(string status)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedSiteDetails";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@siteCollection", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@siteurl", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = "select";
            com.Parameters.Add("@migrationStatus", SqlDbType.NVarChar, 100).Value = status;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;

        }
        public DataTable getSpecficSiteToMigrated(string status, string siteColl)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedSiteDetails";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@siteCollection", SqlDbType.NVarChar, 100).Value = siteColl;
            com.Parameters.Add("@subsite", SqlDbType.NVarChar, 100).Value = siteColl;
            com.Parameters.Add("@siteurl", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = "select";
            com.Parameters.Add("@migrationStatus", SqlDbType.NVarChar, 100).Value = status;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            com.Connection.Open();
            adp.Fill(dt);
            com.Connection.Close();
            return dt;

        }

        public DataTable getDataofBlog(string siteCollection, string subSite, string listname)
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "OperationMigratedListItem";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SITECOLLECTION", SqlDbType.NVarChar, 100).Value = siteCollection;
            com.Parameters.Add("@SUBSITE", SqlDbType.NVarChar, 100).Value = subSite;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 100).Value = "SELECT";
            com.Parameters.Add("@ListName", SqlDbType.NVarChar, 100).Value = listname;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            foreach (var column in dt.Columns.Cast<DataColumn>().ToArray())
            {
                if (dt.AsEnumerable().All(dr => dr.IsNull(column)))
                    dt.Columns.Remove(column);
            }
            return dt;

        }

        #region "Anlaytics"
        public void blogAnalytics(Blog b, ListItem item, string segment, string channel)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonBlogDetails]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "INSERT";
            com.Parameters.Add("@CHANNEL", SqlDbType.NVarChar, 100).Value = channel;
            com.Parameters.Add("@SEGMENT", SqlDbType.NVarChar, 100).Value = segment;
            com.Parameters.Add("@BLOGTYPE", SqlDbType.NVarChar, 10).Value = (b.categoryType != null ? b.categoryType : "T");
            com.Parameters.Add("@BLOGID", SqlDbType.Int).Value = Convert.ToInt16(item["ID"]);
            com.Parameters.Add("@BLOGTITLE", SqlDbType.NVarChar, -1).Value = item["Title"].ToString();
            com.Parameters.Add("@ACTIVITY", SqlDbType.NVarChar, -1).Value = string.Empty;
            com.Parameters.Add("@KEYAREA", SqlDbType.NVarChar, 250).Value = b.keyArea;
            com.Parameters.Add("@CONTENTTYPEID", SqlDbType.NVarChar, 100).Value = b.contentType;
            com.Parameters.Add("@AUTHOR", SqlDbType.NVarChar, 100).Value = b.author;
            com.Parameters.Add("@CREATED", SqlDbType.DateTime).Value = b.created;
            com.Parameters.Add("@APPORVEDDATE", SqlDbType.DateTime).Value = b.created;//(b.published.ToString() == "01-01-0001 00:00:00" ? b.modified : b.published);
            com.Parameters.Add("@BLOGSTATUS", SqlDbType.NVarChar, 100).Value = "APPROVED";
            com.Parameters.Add("@ThumbNailPath", SqlDbType.NVarChar, -1).Value = b.thumnailPath;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }
        //public void blogAnalyticsComment(Blog b, string segment, string channel,int blogID,string category,
        //    string authorName,string authorEmail,DateTime created)
        public void blogAnalyticsCommentLike(string segment, string channel, int blogID, string category,
            string authorName, string authorEmail, DateTime created, string categoryType)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonBlogPromotedLikeComment]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "INSERT";
            com.Parameters.Add("@CHANNEL", SqlDbType.NVarChar, 100).Value = channel;
            com.Parameters.Add("@SEGMENT", SqlDbType.NVarChar, 100).Value = segment;
            com.Parameters.Add("@BLOGTYPE", SqlDbType.NVarChar, 10).Value = categoryType;
            com.Parameters.Add("@BLOGID", SqlDbType.Int).Value = Convert.ToInt16(blogID);
            com.Parameters.Add("@CATEGORY", SqlDbType.NVarChar, 10).Value = category;
            com.Parameters.Add("@LOGGEDEMAIL", SqlDbType.NVarChar, 100).Value = authorEmail;
            com.Parameters.Add("@LOGGEDUSER", SqlDbType.NVarChar, 100).Value = authorName;
            com.Parameters.Add("@LOGGEDTIME", SqlDbType.DateTime).Value = created;
            com.Parameters.Add("@DEVICE", SqlDbType.NVarChar, 10).Value = "D";
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public List<KeyArea_Channel> getKeyArea_Channel()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonKeyArea_Channel]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@segment", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@channel", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = "selectForMigration";
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            List<KeyArea_Channel> KeyArea_ChannelCollection = dt.AsEnumerable().Select(m => new KeyArea_Channel()
            {
                title = m.Field<string>("title"),
                channel = m.Field<string>("channel"),
                segment = m.Field<string>("segment"),
                rowID = m.Field<int>("ID")
            }).ToList();
            return KeyArea_ChannelCollection;
        }

        public void updateKeyArea(string keyarea, string segment, string channel)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "OperationonKeyArea_Channel";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@keyarea", SqlDbType.NVarChar, 100).Value = keyarea;
            com.Parameters.Add("@segment", SqlDbType.NVarChar, 100).Value = segment;
            com.Parameters.Add("@channel", SqlDbType.NVarChar, 100).Value = channel;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = "insert";
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public List<ContentType> getContentType()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonContentType]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@contentType", SqlDbType.NVarChar, 100).Value = string.Empty;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 100).Value = "select";
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            List<ContentType> contentTypeCollection = dt.AsEnumerable().Select(m => new ContentType()
            {
                title = m.Field<string>("title"),
                rowID = m.Field<int>("ID")

            }).ToList();

            return contentTypeCollection;
        }

        public void updateContentType(string contentType)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "OperationonContentType";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@contentType", SqlDbType.NVarChar, 100).Value = contentType;
            com.Parameters.Add("@type", SqlDbType.NVarChar, 10).Value = "insert";
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        public void cleanBlogID(string segment, string channel, int blogId)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "CLEANUPMIGRATEDBLOGID";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@SEGMENT", SqlDbType.NVarChar, 100).Value = segment;
            com.Parameters.Add("@CHANNEL", SqlDbType.NVarChar, 100).Value = channel;
            com.Parameters.Add("@BLOGID", SqlDbType.NVarChar, 10).Value = blogId;
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }

        #endregion


        public DataTable getRecordForHide()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = @"select sc.Segment,sc.Channel, bdd.*,bds.title,bds.moderationStatus, bds.ID 'DBID' from LearnetUsage.dbo.blogdetails bdd 
inner join LearnetUsage.dbo.Segment_Channel sc 
on sc.ID= bdd.Segment_Channel_ID
inner join BLOGDETAILS bds
on bdd.BlogTitle= bds.title
where moderationStatus ='hide'
order by sc.Channel";
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }


        public DataTable getRecordForDataMovedtoMyCornerFromFCNA()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            string cmdt = @"select b.BlogTitle,b.BlogID ,sc.Segment,sc.Channel, 0 'DBID' from (
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Social%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Science and Technology%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'history%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Short Stories%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
) as c inner join LearnetUsage.dbo.BlogDetails b on c.title=b.BlogTitle
inner join LearnetUsage.dbo.Segment_Channel sc on sc.ID=b.Segment_Channel_ID
 where b.[Type] ='t' and b.Segment_Channel_ID=10";
            cmdt = @"select distinct   bd.BlogTitle,bd.BlogID ,sc.Segment,sc.Channel, 0 'DBID'
from LearnetUsage.dbo.BlogDetails bd
inner join  BLOGDETAILS bds  on bd.BlogTitle= bds.title
inner join LearnetUsage.dbo.Segment_Channel sc on sc.ID=bd.Segment_Channel_ID
 where SiteCollection='P&C Academy' and Subsite='My Corner' and typeOfContent='Topic' and [Type]='t' and bd.Segment_Channel_ID=17
 order by bd.BlogID";
            com.CommandText = cmdt;
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }

        public DataTable getRecordForDummyData()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            string cmdt = @"select b.BlogTitle,b.BlogID ,sc.Segment,sc.Channel, 0 'DBID' from (
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Social%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Science and Technology%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'history%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories not in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
and categories like'Short Stories%'
union
select * from BLOGDETAILS
where  Subsite='FC&A' and typeOfcontent='Topic' 
and categories in ('Social','history','Ms excel','Purchase','Short Stories','Science and Technology')
) as c inner join LearnetUsage.dbo.BlogDetails b on c.title=b.BlogTitle
inner join LearnetUsage.dbo.Segment_Channel sc on sc.ID=b.Segment_Channel_ID
 where b.[Type] ='t' and b.Segment_Channel_ID=10";
            cmdt = @"select distinct   bd.BlogTitle,bd.BlogID ,sc.Segment,sc.Channel, 0 'DBID'
from LearnetUsage.dbo.BlogDetails bd
inner join  BLOGDETAILS bds  on bd.BlogTitle= bds.title
inner join LearnetUsage.dbo.Segment_Channel sc on sc.ID=bd.Segment_Channel_ID
 where SiteCollection='P&C Academy' and Subsite='My Corner' and typeOfContent='Topic' and [Type]='t' and bd.Segment_Channel_ID=17
 order by bd.BlogID";
            cmdt = @"select * ,0 'DBID' from Segment_Channel sc inner join 
(
select * from BlogDetails where BlogTitle Like 'test%'
union
select * from BlogDetails where BlogTitle like 'Welcome to your%') bd  on sc.ID=bd.Segment_Channel_ID";
            cmdt = @"SELECT * ,0 'DBID' FROM BlogDetails b INNER JOIN Segment_Channel SC ON SC.ID=B.Segment_Channel_ID where Segment_Channel_ID in (1,24) and ContenTypeId is null";

            cmdt = @"select sc.Segment,sc.Channel , b.moderationStatus,bs.* , 0 'DBID'  from BlogDetails bs
inner join RILSocialPlatformMigration_05012018.dbo.BLOGDETAILS b on bs.BlogTitle= b.title
inner join Segment_Channel sc on bs.Segment_Channel_ID=sc.ID
where  moderationStatus='pending' and typeOfContent='topic' --and Subsite='learnet' and sc.Segment='hydro'";
            cmdt = @"SELECT * ,0 'DBID' FROM BlogDetails b INNER JOIN Segment_Channel SC ON SC.ID=B.Segment_Channel_ID where sc.Segment in ('Hydrocarbon','Retail','root')";
            cmdt = @"select *,0 'DBID' from Segment_Channel sc inner join
(select * from BlogDetails where BlogDetailAlphaNumber in ('4T748','4T753','4T754','9T1281','9T1395','9T1398','9T1400','9T1401','9T1405',
'9T1426','9T1428','9T1432','9T1443','9T1446','9T1454','9T1457','10T10294','13T863','13T864','13T865','13T866','13T867','13T868','13T869',
'13T870','13T871','13T872','16T296','17T9','17T10','17T70','17T112','17T194','17T195','17T196','17T226','17T227','17T228','17T418','18T45',
'18T51','18T55','18T56','18T57','18T66','18T67','18T68','20T5','22T159','22T160','22T161','22T162','23T53','23T63','23T64','24T237','24T238')
union
select * from BlogDetails where BlogDetailAlphaNumber in
('10V530','10V531','10V532','10V533','10V534','17V58','20V484','20V485','20V486','20V487','20V488','20V489','20V490','20V491','20V492','20V493',
'20V496','20V497','20V498','20V499','20V500')) b on b.Segment_Channel_ID=sc.ID";
            cmdt = @"select  sc.segment,sc.channel,t.blogid, t.[type] ,0 'DBID' from temp_learnet t
inner join [LearnetUsage_Prod].dbo.Segment_Channel sc on t.channel = sc.channel";
            com.CommandText = cmdt;
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }

        public List<Blog> getRecordForMyCorner()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            string cmdt = @"select * from BLOGDETAILS where  Subsite='FC&A' and typeOfcontent='Topic' and migrated='no'
and categories in ('History','Ms Excel','Purchase', 'Science and Technology','Science and Technology;Social',
'Short Stories','Short Stories;Social','Social','Social;Accounts','Social;Finance;Accounts','Social;Science and Technology',
'Social;Short Stories') union
select * from BLOGDETAILS where typeOfContent='topic' and migrated='no' and Subsite in ('Blogs','My Corner') order by id";
//            cmdt=@"select distinct bds.* 
//from LearnetUsage.dbo.BlogDetails bd
//inner join  BLOGDETAILS bds  on bd.BlogTitle= bds.title
// where SiteCollection='P&C Academy' and Subsite='My Corner' and typeOfContent='Topic'
// order by bds.BlogID";
            com.CommandText = cmdt;

            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            List<Blog> blogCollection = dt.AsEnumerable().Select(m => new Blog()
            {
                author = m.Field<string>("author"),
                body = m.Field<string>("body"),
                created = m.Field<DateTime>("created"),
                editor = m.Field<string>("editor"),
                likeCount = m.Field<Int32>("likeCount"),
                likedBy = m.Field<string>("likedBy"),
                modified = m.Field<DateTime>("created"),
                published = m.Field<DateTime>("created"),
                siteCollection = m.Field<string>("siteCollection"),
                subSite = m.Field<string>("subSite"),
                ID = m.Field<Int32>("BlogID"),
                title = m.Field<string>("title"),
                keyArea = m.Field<string>("keyArea"),
                contentType = m.Field<string>("contentType"),
                typeOfContent = m.Field<string>("typeOfContent"),
                hasComment = m.Field<bool>("hasComment"),
                hasAttachement = m.Field<bool>("hasAttachement"),
                rowID = m.Field<int>("ID"),
                moderationStatus = m.Field<string>("moderationStatus"),
                categoryType = m.Field<string>("Categorytype"),
                thumnailPath = m.Field<string>("ThumnailPath"),
                project = m.Field<string>("project"),
                filepath = m.Field<string>("filepath")

            }).ToList();

            return blogCollection;

        }

        public DataTable getRecordForKEYAREACT()
        {
            string comm = @"select BlogID , KeyArea,ContenTypeId from  LearnetUsage.dbo.BlogDetails lbd where lbd.Segment_Channel_ID=9 and [Type]='t'";
//            comm = @"select  BlogID , KeyArea,ContenTypeId  from LearnetUsage.dbo.BlogDetails where BlogID in 
//(46,56,410,629,633,634,655,669,670,671,676,677,697,700,723,725,726,731,735,750,750,756,756,791,808,841,850,862,866,881,888,928,928,934,934,960,987,
//989,1030,1035,1047,1066,1094,1109,1113,1114,1117,1120,1152,1192,1204,1209,1230,1243,1277,1443,1459,1480,1489,1495,1500,1542,1570,1601,1624,1649,1675,
//1688,1722,1730,1791,1794,1802,1805,1842,1845,1885,1887,1901,1912,1994,2002,2008,2033,2041,2092,2116,2174,2178,2195,2213,2246,2247,2418,2447,2481,2493,
//2501,2528,2530,2539,2544,2551,2556,2579,2591,2602,2635,2640,2656,2667,2675,2678,2682,2683,2694,2720,2821,2835,2853,2854,2859,2870,2875,2876,2881,2882,
//2884,2886,2890,2891,2892,2894,2898,2918,2938,2950,2953,2954,2955,2956,2957,2958,2961,11451) and Segment_Channel_ID=10 and [Type]='t'";
//            comm = @"select * from BlogDetails where Segment_Channel_ID=13 and Type='t'";
//            comm = @"select b.BlogTitle,d.title,d.categories ,b.BlogID,b.Activity,b.KeyArea,b.ContenTypeId from LearnetUsage.dbo.BlogDetails b
//inner join  RILSocialPlatformMigration.dbo.BLOGDETAILS d  on b.BlogTitle=d.title
// where d.SiteCollection ='FC&A' and d.Subsite='XPrO Chats' and typeOfContent='topic' and  segment_channel_id=10";
//            comm = @"select * from  LearnetUsage.dbo.BlogDetails d  where Segment_Channel_ID=16 and type='v'";
//            comm = @"select * from BlogDetails bd inner join Segment_Channel sc on bd.Segment_Channel_ID=sc.ID where sc.Segment not in ('Root', 'MFG')";
//            comm = @"SELECT * FROM BlogDetails  BD 
//INNER JOIN Segment_Channel SC ON BD.Segment_Channel_ID=SC.ID
//WHERE BlogDetailAlphaNumber IN ('4T745','4T746','4T747','4T749','4T752','4V3','4V5','4V6','4V8','9T1282','9T1394','9T1447','9T1452','9V5',
//'10T3716','10T4659','10T4677','10T4705','10T4728','10T9920','10T10281','10T10282','10T10283','10T10285','10T10288','10T10295','10T10301','10V74','10V109',
//'10V131','10V146','10V148','10V157','10V161','10V163','10V198','10V199','10V201','10V204','10V213','10V227','10V267','10V287','10V319','10V399','10V410','10V411',
//'10V428','10V453','13T756','13T861','13V455','13V517','13V521','13V526','13V531','13V536','13V537','13V538','13V541','13V551','13V552','13V553','13V554','13V561',
//'13V568','13V571','13V574','13V575','13V584','13V591','13V602','13V603','13V604','13V605','13V607','13V608','13V610','13V616','15T7','15T9','15T15','15T17','15T22',
//'15V1','15V10','15V117','15V160','15V183','15V215','15V225','15V227','15V228','15V229','15V235','15V241','15V242','16T4','16T6','16T9','16T11','16T18','16T19','16T20',
//'16T21','16T37','16T43','16T44','16T46','16T47','16T49','16T50','16T64','16T65','16T67','16T72','16T85','16T89','16T101','16T103','16T106','16T110','16T112','16T190',
//'16T198','16T199','16V2','16V3','16V4','16V7','16V8','16V17','16V29','16V33','16V52','16V57','16V59','16V60','16V61','16V62','16V66','16V73','16V74','16V76','16V82',
//'16V83','16V85','16V93','16V98','16V99','16V101','16V103','16V104','16V105','16V107','16V111','16V143','16V162','16V175','16V184','16V186','16V193','17T3','17T11',
//'17T13','17T41','17T68','17T71','17T113','17T146','17T151','17T153','17T156','17T163','17T171','17T172','17T184','17T197','17T229','17T245','17T251','17T308','17T327',
//'17T334','17T376','17T379','17T383','17T392','17T397','17T411','17T414','17T419','17T430','17T465','17T471','17T473','17T502','17T505','17T508','17T510','17T512','17T514',
//'17T520','17T522','17T854','17T856','17T885','17T3661','17V3','17V17','17V20','17V21','17V24','17V27','17V39','17V40','17V44','17V45','17V49','17V50','18T31','18T38','18T39',
//'18T40','18T41','18T42','18T43','18T44','18T46','18T47','18T49','18T52','18T59','18V1','18V18','18V20','18V21','18V23','18V30','19T3','19T4','19T5','19T6','19T7','19T8','19T9',
//'19T11','19T12','19T13','19T14','19T15','19T19','19V1','19V14','19V28','19V40','19V46','19V48','19V60','19V61','19V62','19V73','19V75','19V76','19V109','19V122','19V123','19V126',
//'19V169','19V172','19V173','19V189','19V192','19V200','19V247','19V252','19V256','19V258','19V261','19V263','19V264','19V266','19V267','19V270','19V285','19V288','19V505','19V585',
//'19V592','19V625','19V685','19V687','19V918','19V928','19V1006','19V1012','19V1014','19V1048','19V1080','19V1137','19V1152','19V1193','19V1198','19V1484','19V1488','19V1492',
//'19V1516','19V1522','19V1756','19V1781','19V1782','19V1948','19V2158','19V2179','19V2186','19V2252','19V2303','19V2304','19V2320','19V2371','19V2377','19V2383','19V2539','19V2540',
//'19V2565','19V2585','19V2593','19V2783','19V2788','19V2848','19V2850','19V2874','19V2889','19V2908','19V2923','19V3238','19V3256','19V3257','19V3258','19V3287','20V1','20V3',
//'20V136','20V149','20V252','20V287','20V292','20V323','20V335','20V350','20V354','20V370','20V380','20V411','20V437','20V459','20V460','20V481','21T3','21T26','21T32','21T34',
//'21T35','21T36','21T37','21T38','21T41','21T42','21T44','21T46','21T48','21T151','21T154','21T157','21T159','21T163','21T394','21T396','21T398','21T403','21T408','21T411',
//'21T414','21V1','21V4','21V11','21V12','22T3','22T5','22T7','22T8','22T21','22T22','22T30','22T39','22T43','22T105','22T144','22T147','22T155','22V1','22V37','22V50','22V59',
//'22V64','22V67','22V84','22V89','22V99','22V119','22V120','22V124','22V125','22V131','22V135','22V164','22V167','22V174','22V176','22V212','22V236','22V274','22V275','22V276',
//'22V277','22V278','22V280','22V281','22V301','22V310','22V330','22V357','22V358','22V369','23T5','23T8','23T10','23T12','23T14','23T20','23T22','23T38','23T40',
//'23T42','23T47','23T48','23T49','23T54','23T56','23T57','23T58','23T59','23V1','23V2','23V3','23V4','23V7','23V8','23V9')";
//            comm = @"select * from  BlogDetails b inner join Segment_Channel sc on b.Segment_Channel_ID=sc.ID  where BlogDetailAlphaNumber in (
//'29T1','29T4','29T8','29T9','29T10','29T13','29T14',
//'29T16','29T17','29T18','29V1','29V2','29V5','29V6','29V11','29V12')";
//            comm = @"select * from  BlogDetails b inner join Segment_Channel sc on b.Segment_Channel_ID=sc.ID  where BlogDetailAlphaNumber in
//('34V3','35V529','35V533','35V534','35V538','35V539','37V198','37V199','37V200','37V201','37V202','37V203','37V204','37V205','37V206','37V207','37V208','37V209','37V210','37V211','37V212','37V213','37V214','37V215','37V216','37V237','37V315','37V316','37V317','37V318','37V408','37V433','37V434','37V458','37V459','38V259','38V261','38V298','38V345','39V56','40V1','40V2','40V4','41V1','41V14','41V28','41V40','41V46','41V48','41V60','41V61','41V62','41V73','41V75','41V76','41V109','41V122','41V123','41V126','41V169','41V172','41V173','41V189','41V192','41V200','41V247','41V252','41V256','41V258','41V261','41V263','41V264','41V266','41V267','41V270','41V285','41V288','41V505','41V585','41V592','41V625','41V662','41V664','41V895','41V905','41V983','41V989','41V991','41V1025','41V1057','41V1114','41V1129','41V1170','41V1175','41V1463','41V1467','41V1471','41V1495','41V1501','41V1734','41V1768','41V1793','41V1794','41V1960','41V2170','41V2191','41V2198','41V2264','41V2316','41V2317','41V2333','41V2384','41V2390','41V2396','41V2552','41V2553','41V2578','41V2598','41V2606','41V2796','41V2801','41V2861','41V2863','41V2887','41V2902','41V2921','41V2936','41V3251','41V3269','41V3270','41V3271','41V3300','41V3314','42V1','42V3','42V136','42V149','42V253','42V288','42V293','42V324','42V336','42V351','42V355','42V361','42V371','42V376','42V377','42V381','42V398','42V412','42V438','42V460','42V461','42V482','42V485','42V486','42V487','43V33','43V50','44V239','44V390')";
//            comm = @"select * 
//from BlogDetails  b 
//inner join Segment_Channel sc on b.Segment_Channel_ID = sc.ID
//where sc.Segment='hydrocarbon' and sc.Channel in ('RSRMA','RPMG')";
//            comm = @"select * 
//from BlogDetails  b 
//inner join Segment_Channel sc on b.Segment_Channel_ID = sc.ID
//where sc.Segment='hydrocarbon' and sc.Channel in ('RNDSocial')";
//            comm = @"select *  from blogdetails b inner join Segment_Channel sc on b.Segment_Channel_ID = sc.ID where Segment_Channel_ID=8 and ContenTypeId=8";
//            comm = @"select datediff (day,created,approvedDate), * from blogdetails b
//inner join Segment_Channel sc on b.Segment_Channel_ID = sc.ID
// where convert(varchar(10), created, 120)  != convert(varchar(10), approvedDate, 120) 
//and datediff (day,created,approvedDate) >60";
//            comm = @"select * from  BlogDetails b
//inner join Segment_Channel sc on b.Segment_Channel_ID = sc.ID
// where b.Segment_Channel_ID in (15)";
            comm = @"select sc.Segment,sc.Channel,a.*, CommentCount -[count] from 
(
select  bp.BlogID , b.BlogTitle,b.BlogID 'ID',abc.[type],b.Segment_Channel_ID,Category, count(*) 'count' , CommentCount
 from Blog_Promoted_Like_Comment bp
inner join BlogDetails b on b.BlogDetailAlphaNumber=bp.BlogID
inner join Abc on bp.BlogID= cast(sc as nvarchar(2))+abc.[type]+cast(id as nvarchar(10))
where Category='COMMENT'
group by bp.BlogID, b.BlogTitle,b.BlogID,b.Segment_Channel_ID,abc.[type],Category,CommentCount) a
inner join Segment_Channel sc on sc.ID= a.Segment_Channel_ID
where [count] != CommentCount and CommentCount -[count] >0 
and Segment_Channel_ID =15
order by a.BlogID";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText =comm ;
            com.CommandType = CommandType.Text;
            SqlConnection analytics1 = new SqlConnection("Data Source=Sidcsp13dbip;Initial Catalog=LearnetUsage;Persist Security Info=True;User ID=usagestats; Password=usage@123;");
            com.Connection = analytics1;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }



        public DataTable getRecordForLeadersConnectThumbNail()
        {
            string comm = @"select s.Title 'sitecollection',sc.Channel,bd.ThumbNailPath, bd.[Type], DownloadPath from BlogDetails bd 
inner join GLBlogs gl on bd.BlogID=gl.BlogID and bd.BlogTitle = gl.BlogTitle
inner join Segment_Channel sc on bd.Segment_Channel_ID=sc.ID
inner join Segment s on s.ID=sc.SegmentID
inner join RILSocialPlatformMigration_16042018.[dbo].[MigratedListItemEmbededDocument] md on ThumbNailPath= Filepath
where gl.NewKeyArea!='Not to be replicated' and ThumbNailPath !='' and Segment_Channel_ID !=46 and downloaded=1
order by s.Title,sc.Channel , bd.[Type]";
            comm = @"select s.Title 'sitecollection',sc.Channel,bd.ThumbNailPath, bd.[Type], DownloadPath from BlogDetails bd 
inner join [learnetusage].[dbo].[GLblogs] gl on bd.BlogID=gl.BlogID and bd.BlogTitle = gl.BlogTitle
inner join Segment_Channel sc on bd.Segment_Channel_ID=sc.ID
inner join Segment s on s.ID=sc.SegmentID
inner join RILSocialPlatformMigration_09052018.[dbo].[MigratedListItemEmbededDocument] md on ThumbNailPath= Filepath
where gl.NewKeyArea!='Not to be replicated' and ThumbNailPath !='' and Segment_Channel_ID !=24 and downloaded=1
order by s.Title,sc.Channel , bd.[Type]";
            comm = @"select b.*, d.downloadpath from blogdetails b inner join RILSocialPlatformMigration_09052018.[dbo].[MigratedListItemEmbededDocument] d
on b.thumbnailpath = d.filepath where d.downloaded=1  and segment_channel_id=24";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = comm;
            com.CommandType = CommandType.Text;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }

        public List<Blog> getRecordForLeadersConnect()
        {
            string comm = @"select s.Title 'sitecollection',sc.Channel,bd.*,kc.Title,cast (kc.ID as varchar(10)) 'NewKeyarea',ct.Title,cast (ct.ID as varchar(10)) 'NewContenttype' from BlogDetails bd 
inner join GLBlogs gl on bd.BlogID=gl.BlogID and bd.BlogTitle = gl.BlogTitle
inner join Segment_Channel sc on bd.Segment_Channel_ID=sc.ID
inner join Segment s on s.ID=sc.SegmentID
inner join KeyArea_Channel kc on gl.NewKeyArea=kc.Title and kc.Segment_Channel_Id=46
inner join ContentType ct on gl.ContentType= ct.Title
where gl.NewKeyArea!='Not to be replicated' order by s.Title,sc.Channel";
            comm = @"select s.Title 'sitecollection',sc.Channel,bd.*,kc.Title,cast (kc.ID as varchar(10)) 'NewKeyarea',ct.Title,cast (ct.ID as varchar(10)) 'NewContenttype' 
from blogdetails bd inner join 
[learnetusage].[dbo].[GLblogs] gl  on bd.blogtitle=gl.blogtitle
inner join [learnetusage_Prod].[dbo].Segment_Channel sc on bd.Segment_Channel_ID=sc.ID
inner join [LearnetUsage_Prod].[dbo].Segment s on s.ID=sc.SegmentID
inner join [LearnetUsage_Prod].[dbo].KeyArea_Channel kc on gl.NewKeyArea=kc.Title and kc.Segment_Channel_Id=24
inner join [learnetusage_Prod].[dbo].ContentType ct on gl.ContentType= ct.Title
where gl.NewKeyArea !='Not to be replicated' order by s.Title,sc.Channel";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = comm;
            com.CommandType = CommandType.Text;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            List<Blog> blogCollection = dt.AsEnumerable().Select(m => new Blog()
            {
                author = m.Field<string>("author"),               
                created = m.Field<DateTime>("Created"),
                editor = m.Field<string>("author"),
                modified = m.Field<DateTime>("Created"),
                published = m.Field<DateTime>("ApprovedDate"),
                siteCollection = m.Field<string>("sitecollection"),
                subSite = m.Field<string>("channel"),
                ID = m.Field<Int32>("BlogID"),
                title = m.Field<string>("BlogTitle"),
                keyArea = m.Field<string>("NewKeyarea"),
                contentType = m.Field<string>("NewContenttype"),
                typeOfContent = m.Field<string>("Type"),
                thumnailPath = m.Field<string>("ThumbNailPath"),
                categoryType = m.Field<string>("Type")

            }).ToList();

            return blogCollection;
        }
        public DataTable getSegmentChannel()
        {
            string comm = @"select * from Segment_Channel where Segment in ('Hydrocarbon','Retail','root')";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = comm;
            com.CommandType = CommandType.Text;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;

        }
        public DataTable getKeyarea()
        {
            string comm = @"select BlogID , KeyArea,ContenTypeId from  LearnetUsage.dbo.BlogDetails lbd where lbd.Segment_Channel_ID=9 and [Type]='t'";
            comm = @"select  BlogID , KeyArea,ContenTypeId  from LearnetUsage.dbo.BlogDetails where BlogID in 
(46,56,410,629,633,634,655,669,670,671,676,677,697,700,723,725,726,731,735,750,750,756,756,791,808,841,850,862,866,881,888,928,928,934,934,960,987,
989,1030,1035,1047,1066,1094,1109,1113,1114,1117,1120,1152,1192,1204,1209,1230,1243,1277,1443,1459,1480,1489,1495,1500,1542,1570,1601,1624,1649,1675,
1688,1722,1730,1791,1794,1802,1805,1842,1845,1885,1887,1901,1912,1994,2002,2008,2033,2041,2092,2116,2174,2178,2195,2213,2246,2247,2418,2447,2481,2493,
2501,2528,2530,2539,2544,2551,2556,2579,2591,2602,2635,2640,2656,2667,2675,2678,2682,2683,2694,2720,2821,2835,2853,2854,2859,2870,2875,2876,2881,2882,
2884,2886,2890,2891,2892,2894,2898,2918,2938,2950,2953,2954,2955,2956,2957,2958,2961,11451) and Segment_Channel_ID=10 and [Type]='t'";
          
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = comm;
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }
        public DataTable getRecordForActivity()
        {
            string comm = @" select Title,Channel  from  LearnetUsage.dbo.KeyArea_Channel where segment='Leadersconnect'  ";
            //comm = @"select bd.blogid , d.categories 'Activity' from [LearnetUsage_Prod].dbo.blogdetails bd inner join BLOGDETAILS d on bd.BlogTitle=d.title where sitecollection='FC&A' and subsite='gst' and typeOfContent='topic'";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = comm;
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;
            //com.CommandText = "OperationMigratedListItem";
            //com.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }

        public DataTable getRecordForUserProfile()
        {
            //            string query = @"SELECT D.[User] FROM (
            //                                    SELECT DISTINCT Author 'User' FROM BlogDetails ) D
            //                                    LEFT JOIN [dbo].[UserProfile] SP ON D.[User] = SP.Name
            //                                    WHERE SP.Department IS NULL AND D.[USER] NOT IN ('Social Learner (Generic ID)','System Account')";
            //           // query = @"SELECT DISTINCT Author 'User' FROM BlogDetails WHERE Author <> 'System Account'";
            //           // query = @"SELECT DISTINCT (LoggedUser) 'User' FROM SiteUsage WHERE LoggedUser <> 'System Account'";
            //            query = @"select distinct LoggedUser 'User' from Blog_Promoted_Like_Comment where LoggedUser NOT IN ('Social Learner (Generic ID)','System Account')";
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonUserProfile]";
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.Parameters.Add("@EMAIL", SqlDbType.NVarChar, -1).Value = string.Empty;
            com.Parameters.Add("@NAME", SqlDbType.NVarChar, -1).Value = string.Empty;
            com.Parameters.Add("@ECODE", SqlDbType.NVarChar, -1).Value = string.Empty;
            com.Parameters.Add("@MANAGER", SqlDbType.NVarChar, -1).Value = string.Empty;
            com.Parameters.Add("@DEPARTMENT", SqlDbType.NVarChar, -1).Value = string.Empty;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }
        public void insertUserProfile(List<KeyValuePair<string, string>> info)
        {
            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = con;
            com.CommandTimeout = 0;
            com.CommandText = "[OperationonUserProfile]";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@EMAIL", SqlDbType.NVarChar, -1).Value = info.First(ee => ee.Key == "emailAddress").Value.ToString();
            com.Parameters.Add("@NAME", SqlDbType.NVarChar, -1).Value = info.First(ee => ee.Key == "name").Value.ToString();
            com.Parameters.Add("@ECODE", SqlDbType.NVarChar, -1).Value = info.First(ee => ee.Key == "employeecode").Value.ToString();
            com.Parameters.Add("@MANAGER", SqlDbType.NVarChar, -1).Value = info.First(ee => ee.Key == "manager").Value.ToString();
            com.Parameters.Add("@DEPARTMENT", SqlDbType.NVarChar, -1).Value = info.First(ee => ee.Key == "department").Value.ToString();
            com.Parameters.Add("@TYPE", SqlDbType.NVarChar, 10).Value = "INSERT";

            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();

        }
        public DataTable getRecordForMergingStatics()
        {
            DataTable dt = new DataTable();
            SqlCommand com = new SqlCommand();
            com.CommandText = @"SELECT T.BlogID,T.Segment,T.Channel,T.BlogID,t.[Type],ISNULL(T.[LIKE],0) 'LIKE',ISNULL(T.COMMENT,0) 'COMMENT',ISNULL(T.PROMOTE,0) 'PROMOTE' FROM (
SELECT BD.BlogID, SC.Segment,SC.Channel , BD.[Type], BPLC.Category , COUNT(*) 'COUNT' FROM BlogDetails BD
INNER JOIN Segment_Channel SC ON BD.Segment_Channel_ID = SC.ID
INNER JOIN Blog_Promoted_Like_Comment BPLC ON BPLC.BlogID=BD.BlogDetailAlphaNumber
WHERE SC.Segment IN ('Root') --AND BD.[Type]='T'
GROUP BY  BD.BlogID, SC.Segment,SC.Channel,bd.[Type] , BPLC.Category 
) AS S
PIVOT
(
	SUM([COUNT]) FOR Category IN ([LIKE],[COMMENT],[PROMOTE])
) AS T
ORDER BY T.Segment,T.Channel";
            com.CommandType = CommandType.Text;
            com.Connection = con;
            com.CommandTimeout = 0;

            SqlDataAdapter adp = new SqlDataAdapter(com);
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            adp.Fill(dt);
            com.Connection.Close();
            return dt;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (con != null) { con.Dispose(); con = null; }
                if (analytics != null) { analytics.Dispose(); analytics = null; }
            }
            // free native resources
        }


        public void udpateComment(string username,string useremail, DateTime created , string blogid)
        {

            SqlCommand com = new SqlCommand();
            com.CommandType = CommandType.StoredProcedure;
            com.Connection = analytics;
            com.CommandTimeout = 0;
            com.CommandText = "Temp_Comment";
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = username;
            com.Parameters.Add("@useremail", SqlDbType.NVarChar, 100).Value = useremail;
            com.Parameters.Add("@created", SqlDbType.DateTime).Value = created;
            com.Parameters.Add("@blogid", SqlDbType.NVarChar, 100).Value = blogid;          
            if (com.Connection.State != ConnectionState.Open)
            {
                com.Connection.Open();
            }
            com.ExecuteNonQuery();
            com.Connection.Close();
        }
    }
}
