using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Blog
    {
        public string siteCollection { get; set; }
        public string subSite { get; set; }
        public int ID { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string likedBy { get; set; }
        public string categories { get; set; }
        public string keyArea { get; set; }
        public string contentType { get; set; }
        public string moderationStatus { get; set; }
        public bool hasComment { get; set; }
        public int likeCount { get; set; }
        public string author { get; set; }
        public string editor { get; set; }
        public string typeOfContent { get; set; }
        public int parentID { get; set; }
        public bool hasAttachement { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public DateTime published { get; set; }
        public int rowID { get; set; }
        public string categoryType { get; set; }
        public string attachementFiles { get; set; }
        public string thumnailPath { get; set; }
        public string project { get; set; }
        public string filepath { get; set; }
        public string channel { get; set; }
    }
    public class KeyArea_Channel
    {
        public string title { get; set; }
        public string segment { get; set; }
        public string channel { get; set; }
        public int rowID { get; set; }
    }
    public class ContentType
    {
        public string title { get; set; }
        public int rowID { get; set; }
    }
    
}
