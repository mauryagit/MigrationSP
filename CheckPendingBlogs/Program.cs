using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckPendingBlogs
{
    class Program
    {
        static void Main(string[] args)
        {
            DBM db = new DBM();
            string val = string.Empty;
            foreach (DataRow dr in db.getSegmentChannel().Rows)
            {
                string url = "http://sp13devwfe01:46809/{0}/{1}";

                if (dr["Segment"].ToString().ToLower() == "root")
                {
                    url = "http://sp13devwfe01:46809";
                }

                ClientContext ctx = new ClientContext(string.Format(url, dr["Segment"].ToString(), dr["Channel"].ToString()));
                Web oWeb = ctx.Web;
                ctx.Load(oWeb);
                ctx.ExecuteQuery();
                List lst = null;
                string type = string.Empty;
                if (dr["Segment"].ToString().ToLower() != "root")
                {
                    //lst = oWeb.Lists.GetByTitle("DiscussionText");//.GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                    //type = "T";
                    lst = oWeb.Lists.GetByTitle("Discussions List");//.GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                    type = "V";
                }
                else
                {
                    continue;
                    //lst = oWeb.Lists.GetByTitle("MyCorner");//.GetItemById(Convert.ToInt32(dr["BlogID"].ToString()));
                    //type = "T";
                }

                
                ctx.Load(lst);
                CamlQuery varQuery = new CamlQuery();
                varQuery.ViewXml = @"<View>  
                                        <Query> 
                                           <Where><Eq><FieldRef Name='ApprovalStatus' /><Value Type='Choice'>Pending</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy>  
                                        </Query>             
                                  </View>";
                ListItemCollection lstitemcol = lst.GetItems(varQuery);
                ctx.Load(lstitemcol);
                ctx.ExecuteQuery();
                foreach (ListItem it in lstitemcol)
                {
                    val += string.Format("{0}{1}{2}','", dr["ID"].ToString(), type, it["ID"].ToString()) ;
                }
            }
        }
    }
}
