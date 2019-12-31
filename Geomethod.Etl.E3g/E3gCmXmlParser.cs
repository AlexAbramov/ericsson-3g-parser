using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Geomethod.Etl;

namespace Geomethod.Etl.E3g
{
    public class E3gCmXmlParser : XmlParser
    {
        public enum TableNames { Meta, UtranCell, UtranRelation, GsmRelation }
        public enum ColumnNames { FileFormatVersion, Vendor, RncName, TimeStamp, UtranCell }

        Dictionary<ColumnNames, string> dictValues = new Dictionary<ColumnNames, string>();

        string rncName, cellName;

        public E3gCmXmlParser(string columns, string parameters): base(columns, parameters)
        {
            Name = "E3gCm";
        }

        public override void Parse(XmlReader reader)
        {
            if (ReadHeader(reader))
            {
                while (ReadRnc(reader))
                {
                    int depth = 0;
                    while (reader.ReadToElement("un:UtranCell", ref depth))
                    {
                        if (ReadCell(reader))
                        {
                            while (reader.ReadToElement(depth + 1))
                            {
                                switch (reader.Name)
                                {
                                    case "gn:GsmRelation": ReadRow(reader, TableNames.GsmRelation); break;
                                    case "un:UtranRelation": ReadRow(reader, TableNames.UtranRelation); break;
                                }
                            }
                        }
                    }
                }
            }
        }

        bool ReadHeader(XmlReader reader)
        {
            if (reader.ReadToDescendant("fileHeader"))
            {
                var t = base.ResultSet.AddTable(TableNames.Meta);
                t.ColumnSet.AddColumn(ColumnNames.FileFormatVersion, reader.GetAttribute("fileFormatVersion"));
                t.ColumnSet2.AddColumn(ColumnNames.Vendor, reader.GetAttribute("vendorName"));
                t.AddRow();

                if (reader.ReadToFollowing("xn:SubNetwork"))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ReadRnc(XmlReader reader)
        {
            if (reader.ReadToFollowing("xn:SubNetwork"))
            {
                rncName = reader.GetAttribute("id");
                return !String.IsNullOrEmpty(rncName);
            }
            return false;
        }

        bool ReadCell(XmlReader reader)
        {
            var table = base.ResultSet.GetTableStrong(TableNames.UtranCell);
            table.ColumnSet.SetValue(ColumnNames.RncName, rncName, true);
            cellName = reader.GetAttribute("id");
            table.ColumnSet.SetValue(ColumnNames.UtranCell, cellName, true);
            if (reader.ReadToDescendant("un:attributes"))
            {
                var paramDepth = reader.Depth+1;
                while(reader.ReadElement(paramDepth))
                {
                    var key = RemoveSchema(reader.Name);
                    if (base.ContainsExtraColumn(key))
                    {
                        var val = reader.ReadElementContentAsString();
                        table.ColumnSet2.SetValue(key, val, true);
                    }
                }
                table.AddRow();
                return true;
            }
            return false;
        }

        private void ReadRow(XmlReader reader, TableNames tableName)
        {
            var table = base.ResultSet.GetTableStrong(tableName);
            table.ColumnSet.SetValue(ColumnNames.RncName, rncName, true);
            table.ColumnSet.SetValue(ColumnNames.UtranCell, cellName, true);
            var depth = reader.Depth + 1;
            while (reader.ReadToElement(depth, true))
            {
                var key = RemoveSchema(reader.Name);
                if (base.ContainsExtraColumn(key))
                {
                    var val = reader.ReadElementContentAsString();
                    table.ColumnSet2.SetValue(key, val, true);
                }
            }
            table.AddRow();
        }

    }
}
