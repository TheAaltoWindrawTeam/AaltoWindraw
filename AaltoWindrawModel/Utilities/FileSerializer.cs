using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;


namespace AaltoWindraw
{
   
    namespace Utilities
    {
          public static class FileSerializer<T>
            where T : class, ISerializable
        {
            public static void Serialize( string filename, T objectToSerialize )
            {
              Stream stream = File.Open(filename, FileMode.Create);
              BinaryFormatter bFormatter = new BinaryFormatter();
              bFormatter.Serialize(stream, objectToSerialize);
              stream.Close();
            }

            public static T DeSerialize( string filename )
            {
              T objectToDeSerialize;
              Stream stream = File.Open(filename, FileMode.Open);
              BinaryFormatter bFormatter = new BinaryFormatter();
              objectToDeSerialize = (T)bFormatter.Deserialize(stream);
              stream.Close();
              return objectToDeSerialize;
            }
      }
    }
}