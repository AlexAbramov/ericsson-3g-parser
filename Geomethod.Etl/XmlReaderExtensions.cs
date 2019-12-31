using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geomethod.Etl
{
    public static class XmlReaderExtensions
    {
        #region Element
        public static bool ReadElement(this XmlReader reader, int depth)
        {
            return reader.ReadElement() && reader.Depth == depth;
        }

        public static bool ReadElement(this XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element) return true;
            }
            return false;
        }

        public static bool ReadElement(this XmlReader reader, string name)
        {
            return reader.ReadElement() && reader.Name == name;
        }

        public static bool ReadToElement(this XmlReader reader, string name, ref int depth)
        {
            if (depth <= 0)
            {
                if (reader.ReadToDescendant(name))
                {
                    depth = reader.Depth;
                    return true;
                }
            }
            else
            {
                if (reader.ReadToElement(name, depth)) return true;
            }
            return false;
        }

        public static bool ReadToElement(this XmlReader reader, string name, int depth, bool upperDepth=false)
        {
            while(reader.ReadElement() && depth <= reader.Depth)
            {
                if ((upperDepth || depth == reader.Depth) && reader.Name == name) return true;
            }
            return false;
        }

        public static bool ReadToElement(this XmlReader reader, int depth, bool upperDepth = false)
        {
            while (reader.ReadElement() && depth <= reader.Depth)
            {
                if (upperDepth || depth == reader.Depth) return true;
            }
            return false;
        }
        #endregion

        #region Content
        public static int ReadElementContent(this XmlReader reader, Dictionary<string, string> dict)
        {
            int count = 0;
            var ss = reader.ReadElementContentAsArray();
            if (ss != null)
            {
                for (var i = 0; i < ss.Length - 1; i += 2)
                {
                    var key = ss[i];
                    if (dict.ContainsKey(key))
                    {
                        var val = ss[i + 1];
                        dict[key] = val;
                        count++;
                    }
                }
            }
            return count;
        }

        public static string ReadElementContent(this XmlReader reader, string key)
        {
            var ss = reader.ReadElementContentAsArray();
            if (ss != null)
            {
                for (var i = 0; i < ss.Length - 1; i += 2)
                {
                    if (ss[i] == key) return ss[i + 1];
                }
            }
            return null;
        }

        public static string[] ReadElementContentAsArray(this XmlReader reader, string sep = ",=")
        {
            var s = reader.ReadElementContentAsString();
            if (!string.IsNullOrEmpty(s))
            {
                return s.Split(sep.ToCharArray());
            }
            return null;
        }
        #endregion
    }
}