using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace AaltoWindraw.Utilities
{
    public static class NetSerializer
    {

        public static string Serialize(Object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static T DeSerialize<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }
    }
}
