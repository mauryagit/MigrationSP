using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using System.Diagnostics;

namespace testing
{
    class Program
    {
        static void Main(string[] args)
        {

            //if (args.Length == 0)
            //    return;
           //  ClientContext ctx = new ClientContext(string.Format("https://qaspweb1.ril.com/{0}/{1}", args[0],args[1]));
            ClientContext ctx = new ClientContext(string.Format("https://qaspweb1.ril.com"));
            Web oWeb = ctx.Web;
            ctx.Load(oWeb);
            ctx.ExecuteQuery();
            //check for root
            Console.WriteLine(oWeb.Title);

            string[] listType = new string[] { "Discussions List", "DiscussionText","MyCorner" };
            //ListItem oitem=oWeb.Lists.GetByTitle("Discussions List").GetItemById(494);

            foreach (string type in listType)
            {
                NewMethod(ctx, oWeb,type);
            }
           
        }

        private static void NewMethod(ClientContext ctx, Web oWeb, string type)
        {
            CamlQuery q = new CamlQuery();
            //Find all replies for topic with ID=1        
            q.ViewXml = @"<View >
                <Query>
                    <OrderBy><FieldRef Name='ID' /></OrderBy></Query></View>";
            //ListItemCollection itemCollection = oWeb.Lists.GetByTitle("Discussions List").GetItems(q);
            ListItemCollection itemCollection = oWeb.Lists.GetByTitle(type).GetItems(q);

            ctx.Load(itemCollection);
            try
            {
                ctx.ExecuteQuery();
            }
            catch (Exception)
            {

                return;
            }
            

            foreach (ListItem oitem in itemCollection)
            {
                #region parent               
                FieldUserValue author1 = (FieldUserValue)oitem["Author"];
                //Console.WriteLine(author1.LookupValue);
                FieldUserValue editor1 = (FieldUserValue)oitem["Editor"];
                //Console.WriteLine(editor1.LookupValue);
                Console.WriteLine(string.Format(" Item {2} : Author {0} Editor {1} ", author1.LookupValue, editor1.LookupValue, oitem["ID"]));
                if (author1.LookupValue != editor1.LookupValue)
                {
                    oitem["Editor"] = resolveUser(author1.LookupValue, ctx);
                    oitem.Update();
                    try
                    {
                        ctx.ExecuteQuery();
                    }
                    catch (Exception)
                    {

                        log(string.Format(" Item {2} : Author {0} Editor {1} ", author1.LookupValue, editor1.LookupValue, oitem["ID"]));
                    }
                  
                }
                #endregion

                childUpdate(ctx, q, oitem);
            }
        }

        private static void childUpdate(ClientContext ctx, CamlQuery q, ListItem oitem)
        {
            #region child
            // CamlQuery q = new CamlQuery();
            //Find all replies for topic with ID=1        
            q.ViewXml = @"<View Scope='Recursive'>
                <Query>
                    <Where>
                        <Eq>
                            <FieldRef Name=""ParentFolderId"" />
                            <Value Type=""Integer"">" + oitem["ID"] + "</Value></Eq></Where><OrderBy><FieldRef Name='ID' /></OrderBy></Query></View>";
            ListItemCollection replies = oitem.ParentList.GetItems(q);
            ctx.Load(replies);
            ctx.ExecuteQuery();
            Console.WriteLine(replies.Count);
            foreach (ListItem i in replies)
            {
                int parentItemId = Convert.ToInt16(i["ParentItemID"].ToString());
                int pID = Convert.ToInt32(oitem["ID"]);
                // Console.WriteLine(i["Text"]);
                Console.WriteLine(i["ID"]);
                FieldUserValue author = (FieldUserValue)i["Author"];
                FieldUserValue editor = (FieldUserValue)i["Editor"];
                FieldUserValue parentItemEditor = (FieldUserValue)i["ParentItemEditor"];
                Console.WriteLine(string.Format("Reply : Author {0} Editor {1} ParentItemEditor {2}", author.LookupValue, editor.LookupValue, parentItemEditor.LookupValue));

                if (parentItemId == pID)
                {
                    i["ParentItemEditor"] = resolveUser(author.LookupValue, ctx);
                }
                else
                {
                    var onewItem = replies.Where(e => e["ID"].ToString() == parentItemId.ToString());
                    string value = ((FieldUserValue)onewItem.First()["Author"]).LookupValue;
                    i["ParentItemEditor"] = resolveUser(value, ctx);
                }
                i.Update();
                ctx.ExecuteQuery();
                i["Editor"] = resolveUser(author.LookupValue, ctx);
                i.Update();
                ctx.ExecuteQuery();

            }
            #endregion
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
               
            }


            return returnUser;
        }

        private static void log(string message)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(message, EventLogEntryType.Information, 101, 1);
            }




        }
    }

}
