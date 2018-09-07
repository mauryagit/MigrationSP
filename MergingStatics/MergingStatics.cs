using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingStatics
{
    class MergingStatics
    {
        static void Main(string[] args)
        {
            DBM db = new DBM();
            foreach (DataRow dr in db.getRecordForMergingStatics().Rows)
            {
                string url = "http://sp13devwfe01:46809/sites/{0}/{1}";

                if (dr["Segment"].ToString().ToLower() == "root") {
                    url = "http://sp13devwfe01:46809";
                }

                ClientContext ctx = new ClientContext(string.Format(url, dr["Segment"].ToString(), dr["Channel"].ToString()));
                Web oWeb = ctx.Web;
                ctx.Load(oWeb);
                ctx.ExecuteQuery();
                string name = string.Empty;
                if (dr["Type"].ToString().ToLower() == "t")
                {
                    if (dr["Segment"].ToString().ToLower() == "root") {
                        name = "MyCorner";
                    }
                    else
                    {
                        name = "DiscussionText";
                    }
                }
                else { name = "Discussions List"; }

                List spList = ctx.Web.Lists.GetByTitle(name);
                ctx.Load(spList);
                ctx.ExecuteQuery(); 
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml =
                   @"<View> <Query>  <Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>"+dr["BlogID"].ToString()+"</Value></Eq></Where> </Query></View>";

                ListItemCollection listItems = spList.GetItems(camlQuery);
                ctx.Load(listItems);
                ctx.ExecuteQuery(); 

                //ListItem lst = oWeb.Lists.GetByTitle(name).GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                //ctx.Load(lst);
                //ctx.ExecuteQuery();
                foreach (ListItem lst in listItems)
                {
                    string like, comment, promote, dblike, dbcomment, dbpromote = string.Empty;
                    like = (lst["TotalLikeCount"] == null ? "0" : lst["TotalLikeCount"].ToString());
                    comment = (lst["TotalCommentCount"] == null ? "0" : lst["TotalCommentCount"].ToString());
                    if (dr["Segment"].ToString().ToLower() != "root")
                    {
                        promote = (lst["TotalPromotedCount"] == null ? "0" : lst["TotalPromotedCount"].ToString());
                    }
                    else { promote = "0"; }
                    dblike = dr["Like"].ToString();
                    dbcomment = dr["Comment"].ToString();
                    dbpromote = dr["Promote"].ToString();
                    Console.WriteLine(string.Format("List item {0} : DB {1}", like, dblike));
                    Console.WriteLine(string.Format("List item {0} : DB {1}", comment, dbcomment));
                    Console.WriteLine(string.Format("List item {0} : DB {1}", promote, dbpromote));

                    if (like == dblike && comment == dbcomment && promote == dbpromote)
                    {
                        break;
                    }
                    else
                    {
                        //update
                        //ctx.Load(lst);
                        lst["TotalLikeCount"] = dblike;
                        lst["TotalCommentCount"] = dbcomment;
                        if (dr["Segment"].ToString().ToLower() != "root")
                        {
                            lst["TotalPromotedCount"] = dbpromote;
                        }
                        lst.Update();
                        ctx.ExecuteQuery();
                    }
                }

            }
        }
    }
}
