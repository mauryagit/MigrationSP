using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client.UserProfiles;
using Common;
using System.Reflection;
using System.Data;

namespace UserProfile
{
    class UserProfile
    {
        static void Main(string[] args)
        {
            DBM db = new DBM();
            string url = "http://sp13devwfe01:46809";
            ClientContext ctx = new ClientContext(url);
            PeopleManager pm = new PeopleManager(ctx);
            foreach (DataRow dr in db.getRecordForUserProfile().Rows)
            {
                Console.WriteLine(dr["User"].ToString());
                User u = resolveUser(dr["User"].ToString(), ctx);
               // User u = resolveUser("Rupali Choudhary", ctx);
                if (u == null) { continue; }
                try
                {
                    PersonProperties pp = pm.GetPropertiesFor(u.LoginName);
                    ctx.Load(pp, p => p.AccountName, p => p.UserProfileProperties);
                    ctx.ExecuteQuery();
                    userProfileInFo info = new userProfileInFo();
                    foreach (var property in pp.UserProfileProperties)
                    {
                        //Console.WriteLine(string.Format("{0}: {1}",
                        //    property.Key.ToString(), property.Value.ToString()));
                        switch (property.Key.ToString().ToLower())
                        {
                            case "department":
                                info.department = property.Value.ToString();
                                break;
                            case "manager":
                                info.manager = property.Value.ToString();
                                break;
                            case "preferredname":
                                info.name = property.Value.ToString();
                                break;
                            case "employeecode":
                                info.employeecode = property.Value.ToString();
                                break;
                            case "workemail":
                                if (string.IsNullOrEmpty(property.Value.ToString())) { break; }
                                if (string.IsNullOrEmpty(info.emailAddress))
                                {
                                    info.emailAddress = property.Value.ToString();
                                }
                                else
                                {
                                    if (info.emailAddress.ToLower() != property.Value.ToString().ToLower() )
                                    {
                                        info.emailAddress = property.Value.ToString();
                                    }
                                }
                                break;
                            case "sps-userprincipalname":
                                if (string.IsNullOrEmpty(info.emailAddress))
                                {
                                    info.emailAddress = property.Value.ToString().ToLower().Replace("in.","");
                                }
                                break;
                        }
                    }

                    // Add user to Profile DB                    
                    var user = new List<KeyValuePair<string, string>>();
                    foreach (PropertyInfo pv in info.GetType().GetProperties())
                    {
                        var value = pv.GetValue(info, null) ?? "(null)";
                        //Console.WriteLine(value.ToString());
                        user.Add(new KeyValuePair<string, string>(pv.Name, value.ToString()));
                    }
                    db.insertUserProfile(user);
                }
                catch (Exception ex) { continue; }
            }

        }
                
        private static User resolveUser(string user, ClientContext ctx)
        {
            try
            {
                User returnUser = ctx.Web.EnsureUser(user);
                ctx.Load(returnUser);
                ctx.ExecuteQuery();
                return returnUser;
            }
            catch (Exception)
            {

                return null;
            }

        }
    }

    public class userProfileInFo
    {
        public string emailAddress { get; set; }
        public string name { get; set; }
        public string employeecode { get; set; }
        public string manager { get; set; }
        public string department { get; set; }
    }
}
