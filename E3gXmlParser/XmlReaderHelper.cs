using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomethod.Xml
{
    public enum TagScope {Any=0, Current, NextElement}
    public class XmlReaderHelper: IDisposable
    {
        XmlReader reader;
        int level = 0;        
        public XmlReaderHelper(string filePath)
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            this.reader = XmlReader.Create(filePath, settings);
        }

        public int Level {
            get { return level; }
        }

        public void Dispose()
        {
            if (reader != null)
            {
                reader.Dispose();
            }
        }

        public bool Read(string tag, TagScope scope=TagScope.Current)
        {
            var prevLevel = level;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (!reader.IsEmptyElement) level++;
                        if (reader.Name == tag) return true;
                        if (scope==TagScope.NextElement) return false;
                        break;
                    case XmlNodeType.EndElement:
                        level--;
                        if (level < prevLevel && scope==TagScope.Current) return false;
                        break;
                }
            }
            return false;
        }

        public string ReadElementContent()
        {
            return reader.ReadElementContentAsString();
        }

        public int ReadElementContent(Dictionary<string, string> dict)
        {
            int count = 0;
            var ss = ReadElementContentAsArray();
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

        public string ReadElementContent(string key)
        {
            var ss = ReadElementContentAsArray();
            if (ss != null)
            {
                for (var i = 0; i < ss.Length - 1; i += 2)
                {
                    if (ss[i] == key) return ss[i + 1];
                }
            }
            return null;
        }

        public void ReadElementContent(Action<string, string> action)
        {
            var ss = ReadElementContentAsArray();
            if (ss != null)
            {
                for (var i = 0; i < ss.Length - 1; i += 2)
                {
                    var key = ss[i];
                    var val = ss[i + 1];
                    action(key, val);
                }
            }
        }

        public string[] ReadElementContentAsArray(string sep=",=")
        {
            var s = this.ReadElementContent();
            if (!string.IsNullOrEmpty(s))
            {
                return s.Split(sep.ToCharArray());
            }
            return null;
        }

    }
}
