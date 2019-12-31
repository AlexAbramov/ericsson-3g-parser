using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Geomethod.Etl
{
    public abstract class XmlParser
    {
        public static string RemoveSchema(string s) { var index = s.IndexOf(':'); return index < 0 ? s : s.Substring(index+1); }

        protected static void Parse(string items, Dictionary<string, string> dict)
        {
            if (!string.IsNullOrEmpty(items))
            {
                var ss = items.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in ss)
                {
                    dict.Add(s, null);
                }
            }
        }

        Dictionary<string, string> colDict = new Dictionary<string, string>();
        Dictionary<string, string> colDict2 = new Dictionary<string, string>();

        public ResultSet ResultSet{ get; private set;}
        public bool ContainsColumn(string colName) { return colDict.Count == 0 ? true : colDict.ContainsKey(colName); }
        public bool ContainsExtraColumn(string colName) { return colDict2.Count==0? true : colDict2.ContainsKey(colName); }
        public string Name { get; protected set; }

        protected XmlParser(string columns, string extraColumns)
        {
            ResultSet = new ResultSet();
            Parse(columns, colDict);
            Parse(extraColumns, colDict2);
        }

        public void Parse(string filePath)
        {
            Clear();
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using (var reader = XmlReader.Create(filePath, settings))
            {
                Parse(reader);
            }
        }

        public void Parse(Stream input)
        {
            Clear();
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using (var reader = XmlReader.Create(input))
            {
                Parse(reader);
            }
        }

        public abstract void Parse(XmlReader reader);

        public void Clear()
        {
            ResultSet.Clear();
        }
    }
}
