using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Copyfile
{
    public  class XmlHe
    {
        private static List<T> XmlNodes2Entities<T>(XmlNodeList nodes)where T:new()
        {
            var res = new List<T>();

            try
            {
                if (nodes.Count <= 0)
                {
                    return res;
                }
                var pros = typeof(T).GetProperties();
                foreach(XmlNode item in nodes)
                {
                    var a = new T();
                    foreach(string attr in item.Attributes)
                    {
                        PropertyInfo pp = pros.First(x => x.Name == attr);
                        pp.SetValue(a, attr);

                        
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return res;

        }

    }
}
