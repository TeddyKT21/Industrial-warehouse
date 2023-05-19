using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Industrial_warehouse
{
    static internal class DataBaseManager
    {
        static string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WareHouse.Json");
        public static void Save(Warehouse warehouse)
        {
            string Jstring = JsonConvert.SerializeObject(warehouse, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto,
            });
            using (StreamWriter writer = new StreamWriter(_path))
            {
                writer.Write(Jstring);
                writer.Dispose();
            }
        }
        public static Warehouse Load()
        {
            if (!File.Exists(_path))
            {
                using (FileStream file = new FileStream(_path, FileMode.Create))
                {
                    file.Close();
                }
            }
            Warehouse warehouse = JsonConvert.DeserializeObject<Warehouse>(File.ReadAllText(_path), new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto,

            });
            if(warehouse == null)
                return new Warehouse();
            return warehouse;

        }

        public static void ClearDataBase()
        {
            Warehouse warehouse = new Warehouse();
            Save(warehouse);
        }
    }
}
