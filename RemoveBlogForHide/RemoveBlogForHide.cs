using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveBlogForHide
{
    class RemoveBlogForHide
    {
       
        static void Main(string[] args)
        {
            DBM db = new DBM();
            Console.Title="Remove Blog";
            //DataTable dt = db.getRecordForHide();
           // DataTable dt = db.getRecordForDataMovedtoMyCornerFromFCNA();
            DataTable dt = db.getRecordForDummyData();
            string blogID = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                string url = "http://sp13devwfe01:46809/{0}/{1}";

                if (dr["Segment"].ToString().ToLower() == "root")
                {
                    url = "http://sp13devwfe01:46809";
                }

                Console.WriteLine(Convert.ToInt32(dr["BlogID"].ToString()));
                ClientContext ctx = new ClientContext(string.Format(url, dr["Segment"].ToString(),dr["Channel"].ToString()));
                Web oWeb = ctx.Web;
                ctx.Load(oWeb);
                ctx.ExecuteQuery();
                ListItem lst = null;
                if (dr["Type"].ToString().Trim().ToLower() == "t"){
                    if (dr["Segment"].ToString().ToLower() != "root")
                    {
                        lst = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                    }
                    else
                    {
                        lst = oWeb.Lists.GetByTitle("MyCorner").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                    }
                }else{
                    lst = oWeb.Lists.GetByTitle("Discussions List").GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                }
                ctx.Load(lst);
                try
                {
                    ctx.ExecuteQuery();
                    rollbackTransaction(lst, ctx, "Hide Need to Removed", Convert.ToInt32(dr["DBID"].ToString()), ref db, dr);
                }
                catch (Exception)
                {
                   // blogID += dr["BlogDetailAlphaNumber"].ToString() + "','";
                  
                }
               
               

            }
        }

        private static void rollbackTransaction(ListItem oItem, ClientContext ctx, string errorMessage, int dbItemID, ref DBM db, DataRow dr)
        {
            //Clean blogItem from List
            //get Parent list
            int rowID = (int)oItem["ID"];
            List lst = oItem.ParentList;
            ctx.Load(lst);
            oItem.DeleteObject();
            ctx.ExecuteQuery();

            //// update migration db
            //if (dbItemID != 0)
            //{
            //    db.udpateParseBlogItems(dbItemID, errorMessage, "No");
            //}

            //// update analytics
           // db.cleanBlogID(dr["Segment"].ToString(), dr["Channel"].ToString(), rowID);

        }
    }
}
