using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Configuration;
using System.IO;
namespace ScanBlog
{
    class ScanBlogCls
    {
       static string[] blogtype = new string[] { "Discussions List", "Posts", "DiscussionText" };
        //static string[] blogtype = new string[] { "DiscussionText" };
        public enum TypeOfContent
        {
            Defination,
            DataItem
        }
        static List<KeyValuePair<string, string>> resolvedUserList = new List<KeyValuePair<string, string>>();
        static void Main(string[] args)
        {
            Console.Title = "Scan Blog";
            Console.WriteLine(string.Format("Start {0}", DateTime.Now));
            DBM db = new DBM();
            DataTable siteToScan =null;
            if (args.Length == 0)
            {
                 siteToScan = db.getSiteToMigrated("Inprogress");
            }
            else { siteToScan = db.getSpecficSiteToMigrated("Inprogress",args[0].ToString()); }
            // DataTable siteToScan = db.getDummysiteToMigrated();
            //For Defination
            foreach (DataRow row in siteToScan.Rows)
            {
                ClientContext ctx;
                Web oWeb;
                string type;
                prepareWebContext(row, out ctx, out oWeb, out type);
                getBlogDetails(ctx, oWeb, type, db, TypeOfContent.Defination);
            }
            //For DataItem
            if (args.Length == 0)
            {
                siteToScan = db.getSiteToMigrated("LDEFM");
            }
            else { 
                siteToScan = db.getSpecficSiteToMigrated("LDEFM", args[0].ToString());
                //siteToScan = db.getSpecficSiteToMigrated("LDEFM", "XPrO Chats");
            }
            foreach (DataRow row in siteToScan.Rows)
            {
                ClientContext ctx;
                Web oWeb;
                string type;
                prepareWebContext(row, out ctx, out oWeb, out type);
                getBlogDetails(ctx, oWeb, type, db, TypeOfContent.DataItem);
            }

            Console.WriteLine(string.Format("End {0}", DateTime.Now));
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
                //if (w.Title == "GST")
                //{
                // Console.WriteLine(w.Title + " " + getWebDetails(context, w, "sub"));
                scanweb(context, w);
                //}
            }
        }
        static void scanroot(ClientContext ctx)
        {
            // ctx.Load(ctx.Web.ParentWeb);
            Web root = ctx.Web;
            ctx.Load(root);
            ctx.ExecuteQuery();
            //Get root
            //Console.WriteLine(root.Title + " " + getWebDetails(ctx, root, "root"));
        }
        static void scanweb(ClientContext ctx, Web w)
        {
            WebCollection webs = w.Webs;
            ctx.Load(ctx.Web.ParentWeb);
            ctx.Load(webs);
            ctx.ExecuteQuery();
            foreach (Web iw in webs)
            {
                // if (iw.Title == "GST" )
                // Console.WriteLine("====== " + iw.Title + " " + getWebDetails(ctx, iw, "sub"));

            }
        }
        private static void prepareWebContext(DataRow row, out ClientContext ctx, out Web oWeb, out string type)
        {
            ctx = new ClientContext(row["SiteUrl"].ToString());
            oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();
            type = "root";
            //check for root
            Console.WriteLine(oWeb.Title);
            if (row["SiteCollection"].ToString() != string.Empty)
            {
                type = "sub";
            }
        }

        private static void GetListDefination(ClientContext ctx, List oBlog, string typeOfWeb,
            string ParentTitle, string CurrentTitle)
        {
            //ctx.Load(oBlog.ParentWeb.ParentWeb);
            //ctx.Load(oBlog.ParentWeb);

            string InsertStatement = "Insert into [dbo].[MigratedListDefination] (SiteCollection,Subsite,ListName, ";
            string values = string.Empty;
            FieldCollection fCol = oBlog.Fields;
            ctx.Load(fCol);
            ctx.ExecuteQuery();

            int cnt = 1;
            foreach (Field f in fCol)
            {
                //if (f.InternalName.ToLower().Contains("likescount"))
                //{
                //    Console.WriteLine(f.InternalName + " : " + f.FieldTypeKind);
                //}
                //Console.WriteLine(f.InternalName + " : " + f.FieldTypeKind);
                if (f.FieldTypeKind == FieldType.Text || f.FieldTypeKind == FieldType.Note || f.FieldTypeKind == FieldType.Lookup || f.FieldTypeKind == FieldType.DateTime ||
               f.FieldTypeKind == FieldType.ModStat || f.FieldTypeKind == FieldType.User || f.FieldTypeKind == FieldType.Integer || f.FieldTypeKind == FieldType.Counter ||
                    f.FieldTypeKind == FieldType.Invalid || f.FieldTypeKind == FieldType.Choice || f.FieldTypeKind == FieldType.Attachments)
                {
                    InsertStatement += "VAl" + cnt + ",";
                    values += "'" + f.InternalName + "$" + f.FieldTypeKind + "',";
                    cnt++;
                }
            }
            values = values.Substring(0, values.Length - 1) + ")";
            InsertStatement = InsertStatement.Substring(0, InsertStatement.Length - 1);
            InsertStatement += ")Values('" + ParentTitle + "','" + CurrentTitle + "','" + oBlog.Title + "',";

            DBM db = new DBM();

            db.createListSchema(InsertStatement + values, ParentTitle, CurrentTitle, oBlog.Title);
        }
        private static void GetListDataComment(ClientContext ctx, ListItem item, string parenListDefinationID, DBM db)
        {
            List oComment = ctx.Web.Lists.GetByTitle("Comments");
            ctx.Load(oComment);
            Microsoft.SharePoint.Client.CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml =
               @"<View>  
            <Query> 
               <Where><Eq><FieldRef Name='PostTitle' /><Value Type='Lookup'>" + item["Title"] + "</Value></Eq></Where></Query></View>";
            ListItemCollection listItems = oComment.GetItems(camlQuery);
            ctx.Load(listItems);
            ctx.ExecuteQuery();
            foreach (ListItem i in listItems)
            {
                //Console.WriteLine(i["Body"]);
                string body = (i["Body"] == null ? "" : i["Body"].ToString().Replace("'", "''"));
                string author = string.Empty;
                FieldUserValue[] valColl = null;
                try
                {
                    valColl = GetUserFromAssignedToField(ctx, i["Author"]);
                }
                catch (PropertyOrFieldNotInitializedException)
                {
                }
                if (valColl != null)
                {
                    string multiValue = string.Empty;
                    foreach (FieldUserValue val in valColl)
                    {
                        User user = null;
                        try
                        {
                            user = ctx.Web.EnsureUser(val.LookupValue);
                            ctx.Load(user);
                            ctx.ExecuteQuery();
                        }
                        catch (Exception)
                        {
                            user = GetUserObject(ConfigurationManager.AppSettings["defaultUser"].ToString());
                        }

                        // Console.WriteLine(user.Email);
                        author = (user.Email == "" ? val.LookupValue : user.Email);
                    }
                }

                //FieldUserValue val = i["Author"] as FieldUserValue;
                //if (val != null)
                //{
                //    User user = ctx.Web.EnsureUser(val.LookupValue);
                //    ctx.Load(user);
                //    ctx.ExecuteQuery();
                //    author = user.Email;
                //    if (author == "")
                //    {
                //        author = val.LookupValue;
                //    }
                //}
                string created = i["Created"].ToString();
                //string insertStatement = "Insert into [dbo].[MigratedListDataComment] values (" + parenListDefinationID + "," + item["ID"] + ",'" + body + "','" + author + "','" + created + "'," + i["ID"] + ")";
                db.createListItemComment(parenListDefinationID, item["ID"].ToString(), body, author, created, i["ID"].ToString());

            }
        }

        private static User GetUserObject(string userValue)
        {
            ClientContext ctx = new ClientContext(@"http://sp13devwfe01:8623");
            Web  oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();
            User user = ctx.Web.EnsureUser(userValue);
            ctx.Load(user);
            ctx.ExecuteQuery();
            return user;
        }

        private static void GetListItem(ClientContext ctx, List oBlog, string typeofBlog,
            string ParentTitle, string CurrentTitle)
        {
            CamlQuery varQuery = new CamlQuery();
            if (CurrentTitle.ToLower() == "learnet")
            {

                varQuery.ViewXml = getQueryForCollection((oBlog.Title.ToLower() == "discussions list" ? CurrentTitle + "video" : CurrentTitle + "text"));
                //varQuery.ViewXml =  getQueryForCollection(CurrentTitle);
            }
            else
            {
                varQuery.ViewXml = getQueryForCollection(CurrentTitle);
            }

            ListItemCollection lstitemcol = oBlog.GetItems(varQuery);
            ctx.Load(lstitemcol);
            ctx.ExecuteQuery();
            DBM db = new DBM();
            DataTable dt = null;
            dt = db.getListDefination(ParentTitle, CurrentTitle, oBlog.Title);
            foreach (ListItem item in lstitemcol)
            {
                //if (item["ID"].ToString() == "1798")
                //{
                ManageItemData(ctx, db, dt, item, "parent");

                #region "Migrating Comment"
                //Get the Comments for Discussion Blog
                if (typeofBlog.ToLower() != "posts")
                {
                    CamlQuery q = new CamlQuery();
                    //Find all replies for topic with ID=1        
                    q.ViewXml = @"<View Scope='Recursive'>
                <Query>
                    <Where>
                        <Eq>
                            <FieldRef Name=""ParentFolderId"" />
                            <Value Type=""Integer"">" + item["ID"] + "</Value></Eq></Where></Query></View>";
                    ListItemCollection replies = item.ParentList.GetItems(q);
                    ctx.Load(replies);
                    ctx.ExecuteQuery();
                    Console.WriteLine(replies.Count);
                    foreach (ListItem i in replies)
                    {
                        ManageItemData(ctx, db, dt, i, "reply");
                    }
                }
                else
                {
                    GetListDataComment(ctx, item, dt.Rows[0][0].ToString(), db);
                }
                #endregion
                //}
            }
        }

        //        private static void GetDiscussionBoardDataComment(ClientContext ctx, ListItem item)
        //        {
        //            CamlQuery q = new CamlQuery();
        //            //Find all replies for topic with ID=1        
        //            q.ViewXml = @"<View Scope='Recursive'>
        //                <Query>
        //                    <Where>
        //                        <Eq>
        //                            <FieldRef Name=""ParentFolderId"" />
        //                            <Value Type=""Integer"">" + item["ID"] + "</Value></Eq></Where></Query></View>";
        //            ListItemCollection replies = item.ParentList.GetItems(q);
        //            ctx.Load(replies);
        //            ctx.ExecuteQuery();

        //            foreach (ListItem i in replies)
        //            {

        //                Console.WriteLine(i["Body"]);
        //                string body = i["Body"].ToString().Replace("'", "''");
        //                string author = string.Empty;
        //                FieldUserValue val = item["Author"] as FieldUserValue;
        //                if (val != null)
        //                {
        //                    User user = ctx.Web.EnsureUser(val.LookupValue);
        //                    ctx.Load(user);
        //                    ctx.ExecuteQuery();
        //                    author = user.Email;
        //                    if (author == "")
        //                    {
        //                        author = val.LookupValue;
        //                    }
        //                }
        //                string created = i["Created"].ToString();
        //                string insertStatement = "Insert into [dbo].[MigratedListDataComment] values (18," + item["ID"] + ",'" + body + "','" + author + "','" + created + "'," + i["ID"] + ")";
        //                DBM db = new DBM();
        //                // db.createListItemComment(insertStatement);

        //            }
        //        }
        private static void ManageItemData(ClientContext ctx, DBM db, DataTable dt, ListItem item, string typeOfItem)
        {
            string InsertStatement = "Insert into [dbo].[MigratedListData_Learnet] ";
            string collumn = "(ListDefinationRowId,";
            string values = string.Empty;
            //Where clause statement
            string checkStatement = string.Empty;
            //if (Convert.ToInt16(item["ID"]) == 92)
            //{
            foreach (DataColumn col in dt.Columns)
            {

                if (dt.Rows[0][col].ToString() != "" && !col.ToString().ToLower().Contains("sitecollection")
                    && !col.ToString().ToLower().Contains("subsite") && !col.ToString().ToLower().Contains("listname")
                    && !col.ToString().ToLower().Contains("rowid"))
                {
                    collumn += col + ",";
                    string type, text = string.Empty;
                    string[] stringSeparators = new string[] { "$" };
                    string[] b = dt.Rows[0][col].ToString().Split(stringSeparators, StringSplitOptions.None);
                    text = b[0]; type = b[1];
                    //Console.WriteLine(item[text]);
                    if (text.ToLower() == "title") { if (item[text] == null && typeOfItem != "reply")  return; }
                    switch (type)
                    {
                        case "Text":
                            //Console.WriteLine(item[text]);
                            values += (item[text] == null ? string.Empty : item[text].ToString().Replace("'", "''")) + "','";
                            break;
                        case "Note":
                            //Console.WriteLine(item[text]);
                            values += (item[text] == null ? string.Empty : item[text].ToString().Replace("'", "''")) + "','";
                            break;
                        case "Lookup":
                            FieldLookupValue[] childIdField = null;
                            try
                            {
                                childIdField = GetCategoryFromField(ctx, item[text]);
                            }
                            catch (Exception)
                            {

                            }
                            if (childIdField != null)
                            {
                                string tempVal = string.Empty;
                                foreach (FieldLookupValue childId_Value in childIdField)
                                {
                                    tempVal += (childId_Value.LookupValue == null ? "" : childId_Value.LookupValue.Replace("'", "''") + ";");
                                }
                                //var childId_Value = childIdField.LookupValue;
                                //var childId_Id = childIdField.LookupId;
                                //Console.WriteLine(childId_Value);
                                values += (tempVal.Length != 0 ? tempVal.Substring(0, tempVal.Length - 1) : "") + "','";
                            }
                            else { values += string.Empty + "','"; }
                            break;
                        case "ModStat":
                            // Console.WriteLine(item[text]);
                            values += (item[text] == null ? string.Empty : item[text].ToString().Replace("'", "''")) + "','";
                            break;
                        case "User":
                            FieldUserValue[] valColl = null;
                            try
                            {
                                valColl = GetUserFromAssignedToField(ctx, item[text]);
                            }
                            catch (PropertyOrFieldNotInitializedException)
                            {
                            }
                            if (valColl != null)
                            {
                                string multiValue = string.Empty;
                                foreach (FieldUserValue val in valColl)
                                {
                                    if (string.IsNullOrEmpty(resolvedUserList.FirstOrDefault(ulist => ulist.Key == val.LookupValue).Value))
                                    {
                                        User user = ctx.Web.EnsureUser(val.LookupValue);
                                        ctx.Load(user);

                                        try
                                        {
                                            ctx.ExecuteQuery();
                                            // Console.WriteLine(user.Email);

                                            multiValue += (user.Email == "" ? val.LookupValue : user.Email) + ";";
                                            resolvedUserList.Add(new KeyValuePair<string, string>(val.LookupValue, user.Email));
                                        }
                                        catch (Exception)
                                        {
                                            multiValue += val.LookupValue + ";";
                                        }
                                    }
                                    else { multiValue += resolvedUserList.FirstOrDefault(ulist => ulist.Key == val.LookupValue).Value + ";"; }
                                    // multiValue += (user.Email == "" ? val.LookupValue : user.Email) + ";";
                                }
                                //values += (user.Email == "" ? val.LookupValue : user.Email) + "','";
                                values += multiValue.Substring(0, multiValue.Length - 1) + "','";
                            }
                            else { values += string.Empty + "','"; }
                            break;
                        case "Integer":
                            // Console.WriteLine(item[text]);
                            values += item[text] + "','";
                            break;
                        case "Counter":
                            Console.WriteLine(item[text]);
                            values += item[text] + "','";
                            break;
                        case "DateTime":
                            // Console.WriteLine(item[text]);
                            if (item[text] == null) { values += string.Empty + "','"; } else { values += Convert.ToDateTime(item[text].ToString()) + "','"; }
                            break;
                        case "Invalid":
                            try
                            {
                                values += item[text] + "','";
                            }
                            catch (Exception)
                            {

                                values += string.Empty + "','";
                            }
                            break;
                        case "Attachments":
                            //   Console.Write("something");
                            //values += item[text] + "','";
                            if (item[text].ToString().ToLower() == "true")
                            {
                                values += getAttachmentDetails(ctx, item) + "','";
                            }
                            else { values += string.Empty + "','"; }                           
                            break;
                        default:
                            //Console.WriteLine(item[text]);
                            values += (item[text] == null ? string.Empty : item[text].ToString().Replace("'", "''")) + "','";
                            break;
                    }

                    if (text.ToLower() == "id")
                    {
                        checkStatement += col + " =" + item[text].ToString();
                    }
                }

            }
            checkStatement += " and ListDefinationRowID =" + dt.Rows[0][0];
            values = values.Substring(0, values.Length - 2) + ")";
            collumn = collumn.Substring(0, collumn.Length - 1) + ") values (" + dt.Rows[0][0] + ",'";
            InsertStatement += collumn + values;
            db.createListItem(InsertStatement, checkStatement);
            //}
        }

        private static string getAttachmentDetails(ClientContext ctx, ListItem item)
        {
            string returnString = string.Empty;
            List parentLst = item.ParentList;
            ctx.Load(parentLst);
            ctx.ExecuteQuery();
            string listname = string.Empty;
            if (parentLst.EntityTypeName.Contains("Community_x0020_Discussion"))
            {
                listname = "Community Discussion";
            }
            else
            {
                listname = parentLst.Title;
            }
            Folder folder = ctx.Web.GetFolderByServerRelativeUrl(ctx.Web.Url + "/Lists/" + listname + "/Attachments/" + item["ID"]);          
            ctx.Load(folder);
            FileCollection attachments = folder.Files;
            ctx.Load(attachments);
            ctx.ExecuteQuery();
            foreach (Microsoft.SharePoint.Client.File oFile in folder.Files)
            {
                FileInfo myFileinfo = new FileInfo(oFile.Name);
                returnString += oFile.ServerRelativeUrl + ";";
            }
            return returnString;
        }
        private static string getBlogDetails(ClientContext ctx, Web w, string type, DBM db, TypeOfContent content)
        {
            ListCollection lsts = w.Lists;
            //For every type of dicussion title
            foreach (string btype in blogtype)
            {
                ctx.Load(lsts, lst => lst.Include(ls => ls.Title).Where(ls => ls.Title == btype));
                ctx.ExecuteQuery();
                if (lsts.Count > 0)
                {
                    ctx.Load(w);
                    ctx.Load(w.ParentWeb);
                    List l = w.Lists.GetByTitle(btype);
                    ctx.Load(l);
                    ctx.ExecuteQuery();
                    string ParentTitle = string.Empty;
                    string CurrentTitle = string.Empty;
                    if (type.ToLower() != "root")
                    {
                        ParentTitle = w.ParentWeb.Title.Replace("'", "''");

                    }
                    CurrentTitle = w.Title.Replace("'", "''");
                    if (content == TypeOfContent.Defination)
                    {
                        GetListDefination(ctx, l, type, ParentTitle, CurrentTitle);
                        if (CurrentTitle.ToLower() == "learnet")
                        {
                            if (Array.FindIndex(blogtype, bt => bt == btype) == blogtype.Length - 1)
                            {
                                db.udpateSiteMigratedStatus(ctx.Web.Url, "LDEFM");
                            }
                        }
                        else { db.udpateSiteMigratedStatus(ctx.Web.Url, "LDEFM"); }
                    }
                    else if (content == TypeOfContent.DataItem)
                    {
                        GetListItem(ctx, l, btype, ParentTitle, CurrentTitle);
                        if (CurrentTitle.ToLower() == "learnet")
                        {
                            if (Array.FindIndex(blogtype, bt => bt == btype) == blogtype.Length - 1)
                            {
                                db.udpateSiteMigratedStatus(ctx.Web.Url, "LDATAM");
                            }
                        }
                        else { db.udpateSiteMigratedStatus(ctx.Web.Url, "LDATAM"); }
                    }
                    // return l.ItemCount.ToString();

                }
            }
            return "0";
        }
        public static FieldUserValue[] GetUserFromAssignedToField(ClientContext ctx, object obj)
        {

            FieldUserValue[] childIdFieldColl = null;
            FieldUserValue childIdField = null;
            try
            {
                childIdFieldColl = (FieldUserValue[])obj;
            }
            catch (Exception)
            {
                childIdField = (FieldUserValue)obj;
                childIdFieldColl = new FieldUserValue[1];
                childIdFieldColl[0] = childIdField;
            }
            return childIdFieldColl;
        }
        public static FieldLookupValue[] GetCategoryFromField(ClientContext ctx, object obj)
        {

            FieldLookupValue[] childIdFieldColl = null;
            FieldLookupValue childIdField = null;
            try
            {
                childIdFieldColl = (FieldLookupValue[])obj;
            }
            catch (Exception)
            {
                childIdField = (FieldLookupValue)obj;
                childIdFieldColl = new FieldLookupValue[1];
                childIdFieldColl[0] = childIdField;
            }
            return childIdFieldColl;
        }

        public static string getQueryForCollection(string siteName)
        {
            string returnQuery = string.Empty;
            switch (siteName.ToLower())
            {
                case "tech blog":
                    returnQuery = @"<View>  
                                        <Query> 
                                           <Where><Eq><FieldRef Name='BlogApprovalStatus' /><Value Type='Choice'>Approved</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
                                        </Query>             
                                  </View>";
                    break;
                case "tech forum":
                    returnQuery = @"<View>  
                                        <Query> 
                                           <Where><In><FieldRef Name='ID' /><Values><Value Type='Counter'>5</Value><Value Type='Counter'>6</Value><Value Type='Counter'>9</Value><Value Type='Counter'>12</Value><Value Type='Counter'>13</Value><Value Type='Counter'>35</Value></Values></In></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
                                        </Query>             
                                  </View>";
                    break;
                case "rtg_blogs":
//                    returnQuery = @"<View>  
//                                        <Query> 
//                                           <Where><Or><Eq><FieldRef Name='Approval' /><Value Type='Choice'>Approved</Value></Eq><Eq><FieldRef Name='Approval' /><Value Type='Choice'>Pending</Value></Eq></Or></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
//                                        </Query>             
//                                  </View>";
                    returnQuery = @"<View>  
                                        <Query> 
                                           <Where><Eq><FieldRef Name='Approval' /><Value Type='Choice'>Approved</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
                                        </Query>             
                                  </View>";
                    break;
                case "learnettext":
//                    returnQuery = @"<View>  
//                                        <Query> 
//                                           <Where><Or><Eq><FieldRef Name='ApprovalStatus' /><Value Type='Choice'>Approved</Value></Eq><Eq><FieldRef Name='ApprovalStatus' /><Value Type='Choice'>Pending</Value></Eq></Or></Where><OrderBy><FieldRef Name='ID' /></OrderBy>  
//                                        </Query>             
//                                  </View>";
                    returnQuery = @"<View>  
                                        <Query> 
                                           <Where><Eq><FieldRef Name='ApprovalStatus' /><Value Type='Choice'>Approved</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy>  
                                        </Query>             
                                  </View>";
//                    returnQuery = @"<View>  
//                                        <Query> 
//                                           <Where><In><FieldRef Name='ID' /><Values><Value Type='Counter'>2226</Value><Value Type='Counter'>2227</Value><Value Type='Counter'>2228</Value><Value Type='Counter'>2280</Value><Value Type='Counter'>2285</Value><Value Type='Counter'>2286</Value><Value Type='Counter'>2287</Value></Values></In></Where> 
//                                        </Query>                                         
//                                  </View>";  
                    break;
                case "learnetvideo":
//                    returnQuery = @"<View>  
//                                        <Query>
//                                            <Where><Or><Eq><FieldRef Name='Status' /><Value Type='Choice'>Approved</Value></Eq><Eq><FieldRef Name='Status' /><Value Type='Choice'>Pending</Value></Eq></Or></Where><OrderBy><FieldRef Name='ID' /></OrderBy>
//                                        </Query>             
//                                  </View>";
                    returnQuery = @"<View>  
                                        <Query>
                                            <Where><Eq><FieldRef Name='Status' /><Value Type='Choice'>Approved</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy>
                                        </Query>             
                                  </View>";
//                    returnQuery = @"<View>  
//                                        <Query> 
//                                           <Where><In><FieldRef Name='ID' /><Values><Value Type='Counter'>6701</Value><Value Type='Counter'>6700</Value></Values></In></Where> 
//                                        </Query>                                         
//                                  </View>"; 
                    break;
                default:
//                    returnQuery = @"<View>  
//                                        <Query> 
//                                           <Where><Or><Eq><FieldRef Name='_ModerationStatus' /><Value Type='ModStat'>0</Value></Eq><Eq><FieldRef Name='_ModerationStatus' /><Value Type='ModStat'>2</Value></Eq></Or></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
//                                        </Query>             
//                                  </View>";
                    returnQuery = @"<View>  
                                        <Query> 
                                           <Where><Eq><FieldRef Name='_ModerationStatus' /><Value Type='ModStat'>0</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy> 
                                        </Query>             
                                  </View>";
                    break;
            }
            return returnQuery;
        }

    }
}
