using Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergingActivity
{
    class Program
    {
        static void Main(string[] args)
        {
            DBM db = new DBM();
            string url = "http://sp13devwfe01:8623/Hydrocarbon/FCNA";
            ClientContext ctx = new ClientContext(url);
            Web oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();

            foreach (DataRow dr in db.getRecordForActivity().Rows)
            {
                int blogid = (int)dr["BlogID"];
                string activity = dr["Activity"].ToString();
                Console.WriteLine(blogid);
               
                ListItem oItem = oWeb.Lists.GetByTitle("DiscussionText").GetItemById(blogid);
                //ListItem oItem = oWeb.Lists.GetByTitle("Discussions List").GetItemById(blogid);
                ctx.Load(oItem);

                List lookuplist = ctx.Web.Lists.GetByTitle("Activity");
                oItem["Activity"] = GetLookFieldIDS(ctx, activity, lookuplist);  
          
               // oItem["Activity"] = lv;
              
                oItem.Update();
                ctx.ExecuteQuery();
            }
        }

        public static FieldLookupValue GetLookFieldIDS(ClientContext context, string lookupValues, List lookupSourceList)
        {
            FieldLookupValue lookupIds = new FieldLookupValue();
            string[] lookups = lookupValues.Split(new char[] {  
                    '%'  
                },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (string lookupValue in lookups)
            {
                CamlQuery query = new CamlQuery();
                query.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Title'/><Value Type='Text'>{0}</Value></Eq></Where></Query></View>", lookupValue);
                ListItemCollection listItems = lookupSourceList.GetItems(query);
                context.Load(lookupSourceList);
                context.Load(listItems);
                context.ExecuteQuery();
                foreach (ListItem item in listItems)
                {
                    FieldLookupValue value = new FieldLookupValue();
                    value.LookupId=Convert.ToInt32(item["ID"]);
                   // value.LookupValue = item["Title"].ToString();
                    lookupIds = value;
                    break;
                }
            }
            return lookupIds;
        }  
    }
}
