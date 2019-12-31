using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Geomethod.Etl;

namespace Geomethod.Etl.E3g
{
    public class E3gPmXmlParser : XmlParser
    {
        public enum TableNames { Meta, UtranCell, UtranRelation, GsmRelation }
        public enum ColumnNames {RncName, TimeStamp}

        public E3gPmXmlParser(string columns, string counters): base(columns, counters)
        {
            Name = "E3gPm";
        }

        public override void Parse(XmlReader reader)
        {
            if (ReadHeader(reader))
            {
                while(StartTable(reader))
                {
                    var table = ResultSet.Tables.Last();
                    while (ReadRow(reader, table))
                    {
                        table.AddRow();
                    }
                }
            }
        }

        bool ReadHeader(XmlReader reader)
        {
            if (reader.ReadToDescendant("mfh"))
            {
                if (reader.ReadToDescendant("sn"))
                {
                    var RncName = reader.ReadElementContent("MeContext");
                    if (reader.ReadToNextSibling("cbt"))
                    {
                        var TimeStr = reader.ReadElementContentAsString();
                        var res = !string.IsNullOrEmpty(TimeStr) && !string.IsNullOrEmpty(RncName);
                        if (res)
                        {
                            var t=base.ResultSet.AddTable(TableNames.Meta);
                            t.ColumnSet.AddColumn(ColumnNames.RncName, RncName);
                            t.ColumnSet2.AddColumn(ColumnNames.TimeStamp, TimeStr);
                            t.AddRow();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool StartTable(XmlReader reader)
        {
            var table = base.ResultSet.CreateTable();
            bool? res;
            while((res = ReadTableHeader(reader, table))==null)
            {
                table.Clear();
            }
            if (res == true)
            {
                var tableName = base.ResultSet.TableCount.ToString();
                base.ResultSet.AddTable(tableName,table);
                return true;
            }
            return false;
        }

        bool? ReadTableHeader(XmlReader reader, ResultTable table)
        {
            if (reader.ReadToFollowing("md"))
            {
                if (reader.ReadToDescendant("mi"))
                {
                    if(reader.ReadToDescendant("mt"))// counters
                    {
                        int xmlIndex = 0;
                        do
                        {
                            var key = reader.ReadElementContentAsString();
                            if (base.ContainsExtraColumn(key))
                            {
                                table.ColumnSet2.AddColumn(key, xmlIndex);
                            }
                            xmlIndex++;
                        }
                        while (reader.ReadElement("mt"));
                    }
                    if (table.ColumnSet2.Count == 0) return null;
                    if (reader.ReadToNextSibling("mv"))
                    {
                        if (reader.ReadToDescendant("moid"))
                        {
                            if (ReadColValues(reader, table, true))
                            {
                                return true;
                            }
                            return null;
                        }
                    }
                }
            }
            return false;
        }

        bool ReadColValues(XmlReader reader, ResultTable table, bool addColumns)
        {
            int count = 0;
            var ss = reader.ReadElementContentAsArray();
            if (ss != null)
            {
                for (int i = 0; i < ss.Length - 1; i += 2)
                {
                    var key = ss[i];
                    var val = ss[i + 1];
                    if (addColumns)
                    {
                        if (base.ContainsColumn(key))
                        {
                            table.ColumnSet.AddColumn(key, val);
                            count++;
                        }
                    }
                    else
                    {
                        if (table.ColumnSet.ContainsColumn(key))
                        {
                            table.ColumnSet.SetValue(key, val);
                            count++;
                        }
                    }
                }
            }
            return count == table.ColumnSet.Count && count> 0;
        }

        public bool ReadRow(XmlReader reader, ResultTable table)
        {
            if (table.RowCount > 0)
            {
                if (reader.ReadToDescendant("moid") && ReadColValues(reader, table, false))
                {
                }
                else return false;
            }
            int xmlIndex = 0;
            int count = 0;
            while (reader.ReadElement("r"))
            {
                var s = reader.ReadElementContentAsString();
                if (table.ColumnSet2.SetValue(xmlIndex, s))
                {
                    count++;
                }
                xmlIndex++;
            }
            if (count == table.ColumnSet2.Count)
            {
                return true;
            }
            return false;
        }

        /*      
                int UpdateColumns(Dictionary<string, string> dict, bool isCounter){
                    var count = 0;
                    foreach (var key in dict.Keys)
                    {
                        var val = dict[key];
                        if (val != null){
                            columns.Add(key);
                            row.Add(val);
                            if (!isCounter)
                            {
                                colIndexes.Add(key, count);
                            }
                            count++;
                        }
                    }
                    return count;
                }*/

    }
}
