using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing
{
    class autogenerate
    {
    }


    public class Rootobject
    {
        public Stages stages { get; set; }
    }

    public class Stages
    {
        public Stage[] stage { get; set; }
    }

    public class Stage
    {
        public _Attributes _attributes { get; set; }
        public Read Read { get; set; }
        public Write Write { get; set; }
    }

    public class _Attributes
    {
        public string name { get; set; }
    }

    public class Read
    {
        public Usertype[] usertype { get; set; }
    }

    public class Usertype
    {
        public string _text { get; set; }
    }

    public class Write
    {
        public Usertype1[] usertype { get; set; }
    }

    public class Usertype1
    {
        public string _text { get; set; }
    }

}
