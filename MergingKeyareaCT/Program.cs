using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingKeyareaCT
{
    class Program
    {
        static void Main(string[] args)
        {
            DBM db = new DBM();

            UpDatecOMMENTINDB(db);
            //string url = "http://sp13devwfe01:46809/Hydrocarbon/ENP";
            //ClientContext ctx = new ClientContext(url);           
            // Web oWeb = ctx.Web;
            //ctx.Load(oWeb);
            //ctx.ExecuteQuery();

           // UpDateKeyArea(db.getRecordForActivity());
            //StringBuilder strInsert = new StringBuilder();
            //foreach(DataRow dr in db.getRecordForKEYAREACT().Rows)
            //{
            //    string query = "Insert into Abc (SC,Type,ID,CommentCount) values ({0},{1},{2},{3})";
            //    string url = "http://digitalj3.ril.com/{0}/{1}";
            //    url = "http://digitalj3.ril.com";
            //    ClientContext ctx = new ClientContext(string.Format(url, dr["Segment"].ToString(), dr["Channel"].ToString()));
            //    Web oWeb = ctx.Web;
            //    ctx.Load(oWeb);
            //    ctx.ExecuteQuery();

            //    int blogid =(int) dr["BlogID"];
            //   // int keyarea = Convert.ToInt16( dr["KeyArea"].ToString());
            //    DateTime approvedDate = Convert.ToDateTime(dr["Created"].ToString());
            //    Console.WriteLine(  blogid);
            //   // int ct = Convert.ToInt16((dr["ContenTypeId"].ToString() == "" ? "0" : dr["ContenTypeId"].ToString()));
            //  //ListItem oItem=  oWeb.Lists.GetByTitle("DiscussionText").GetItemById(blogid);
            //    ListItem oItem = null;// oWeb.Lists.GetByTitle("Discussions List").GetItemById(blogid);

            //    if (dr["Type"].ToString().ToLower() == "t")
            //    {
            //       // oItem = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
            //        oItem = oWeb.Lists.GetByTitle("MyCorner").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
            //    }
            //    else
            //    {
            //        oItem = oWeb.Lists.GetByTitle("Discussions List").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
            //    }

            //  ctx.Load(oItem);
            //  ctx.ExecuteQuery();
              
            //  strInsert.AppendLine(string.Format(query, dr["Segment_Channel_ID"], dr["Type"].ToString(), dr["BlogID"].ToString(), oItem["TotalCommentCount"].ToString()));
            //  ////oItem["KeyAreas"] = keyarea;
            //  ////if (ct != 0)
            //  ////{
            //  ////    oItem["ContenTypeId"] = 6;// ct;
            //  ////}
            //  //oItem["ApprovedDate"] = approvedDate;
            //  //oItem.Update();
            //  //ctx.ExecuteQuery();
            //}
        }

        //static void UpDateKeyArea(DataTable dt )
        //{
        //    string url = "http://sp13devwfe01:8623/Leadersconnect";
        //    ClientContext ctx = new ClientContext(url);
        //    Web oWeb = ctx.Web;
        //    ctx.Load(oWeb);
        //    ctx.ExecuteQuery();
        //    List oList = oWeb.Lists.GetByTitle("KeyArea");
           
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        ListItemCreationInformation createItem = new ListItemCreationInformation();
        //        ListItem oItem = oList.AddItem(createItem);
        //        oItem["Title"] = dr["Title"].ToString();
        //        oItem["Channel"] = dr["Channel"].ToString();
        //        oItem.Update();
        //        ctx.ExecuteQuery();
        //    }
        //}

        static void UpDatecOMMENTINDB(DBM db)
        {
            string url = "https://learnet.ril.com/{0}/{1}";
            url = "https://learnet.ril.com";
            foreach(DataRow dr  in db.getRecordForKEYAREACT().Rows ){
            ClientContext ctx = new ClientContext(string.Format(url, dr["Segment"].ToString(), dr["Channel"].ToString()));
            Web oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();

            int blogid = (int)dr["ID"];
            // int keyarea = Convert.ToInt16( dr["KeyArea"].ToString());
          //  DateTime approvedDate = Convert.ToDateTime(dr["Created"].ToString());
            Console.WriteLine(blogid);
            // int ct = Convert.ToInt16((dr["ContenTypeId"].ToString() == "" ? "0" : dr["ContenTypeId"].ToString()));
            //ListItem oItem=  oWeb.Lists.GetByTitle("DiscussionText").GetItemById(blogid);
            ListItem oItem = null;// oWeb.Lists.GetByTitle("Discussions List").GetItemById(blogid);

            if (dr["Type"].ToString().ToLower() == "t")
            {
               // oItem = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(Convert.ToInt32(dr["ID"].ToString()));
                oItem = oWeb.Lists.GetByTitle("MyCorner").GetItemById(Convert.ToInt32(dr["ID"].ToString()));
            }
            else
            {
                oItem = oWeb.Lists.GetByTitle("Discussions List").GetItemById(Convert.ToInt32(dr["ID"].ToString()));
            }
            ctx.Load(oItem);
            ctx.ExecuteQuery();

            CamlQuery q = new CamlQuery();
            //Find all replies for topic with ID=1        
            q.ViewXml = @"<View Scope='Recursive'>
                <Query>
                    <Where>
                        <Eq>
                            <FieldRef Name=""ParentFolderId"" />
                            <Value Type=""Integer"">" + oItem["ID"] + "</Value></Eq></Where></Query></View>";
            ListItemCollection replies = oItem.ParentList.GetItems(q);
            ctx.Load(replies);
            ctx.ExecuteQuery();
            List<CommentUser> commentUsercoll = new List<CommentUser>();

            foreach (ListItem i in replies)
            {
                User loggeduser = null;//resolveUser(GetUserFromAssignedToField(ctx,i["Author"]), ctx);
                #region "User"
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
                                    loggeduser = ctx.Web.EnsureUser(val.LookupValue);
                                    ctx.Load(loggeduser);

                                    try
                                    {
                                        ctx.ExecuteQuery();
                                    }
                                    catch(Exception ex)
                                    {
                                        loggeduser = ctx.Web.EnsureUser(ConfigurationManager.AppSettings["defaultUser"].ToString());
                                        ctx.Load(loggeduser);
                                        ctx.ExecuteQuery();
                                    }
                                }
                            }
#endregion
                string Author = loggeduser.Title;
                string Authoremail = loggeduser.Email;
                DateTime loggedtime = TimeZoneInfo.ConvertTimeFromUtc((DateTime)i["Created"], TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                string blogidNumber = dr["BlogID"].ToString();
                //db.udpateComment(Author, Authoremail, loggedtime, blogidNumber);
                commentUsercoll.Add(new CommentUser()
                {
                    Author = Author,
                    Authoremail = Authoremail,
                    loggedtime = loggedtime,
                    blogidNumber = blogidNumber
                });
                

            }
            }
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
    }

    public class CommentUser
    {
       public string Author { get; set; }
       public string Authoremail { get; set; }
       public DateTime loggedtime { get; set; }
       public string blogidNumber { get; set; }
    }
}
