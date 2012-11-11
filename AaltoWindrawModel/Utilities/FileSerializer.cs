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

    /*
    Use case:
    public void RunExample()
    {
      Car car1 = new Car("Ford", "Mustang GT", 2007);
      Car car2 = new Car("Dodge", "Viper", 2006);
      car1.Owner = new Owner("Rich", "Guy");
      car2.Owner = new Owner("Very", "RichGuy");
 
      //save cars individually
      //note: type-safe as Car
      FileSerializer.Serialize(@"C:\Car1.dat", car1);
      //note: implicit casting to ISerializable
      FileSerializer<Car>.Serialize(@"C:\Car2.dat", car2);

      //save as a collection
      Cars cars = new Cars();
      cars.Add(car1);
      cars.Add(car2);
      //note: type-safe as Cars
      FileSerializer.Serialize(@"C:\Cars.dat", cars);

      //now read them back in (note, you can use either FileSerializer
      //note: casting required
      Car savedCar1 = (Car)FileSerializer.DeSerialize(@"C:\Car1.dat");
      //note: no casting required  
      Car savedCar2 = FileSerializer<Car>.DeSerialize(@"C:\Car2.dat");

      //and for the collection…
      Cars savedCars =
        FileSerializer<Cars>.DeSerialize(@"C:\Cars.dat");
    }
    */
}