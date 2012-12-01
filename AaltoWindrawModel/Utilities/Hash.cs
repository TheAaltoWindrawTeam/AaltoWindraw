using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaltoWindraw.Utilities
{
    public class Hash
    {
        public static string ComputeHash(Object objectToHash)
        {
            // Hash here is MD5, but anything else could do the trick
            return MD5HashGenerator.GenerateKey(objectToHash);
        }
    }
}
