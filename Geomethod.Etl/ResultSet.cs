using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geomethod.Etl
{
    public class ResultSet
    {
        List<ResultTable> tables = new List<ResultTable>();
        public IEnumerable<ResultTable> Tables { get { return tables; } }
        public int TableCount { get { return tables.Count; } }
        public bool IsEmpty { get { return TableCount == 0; } }
        public void Clear() { tables.Clear(); }
        public int TotalRowCount { get { return tables.Sum(t => t.RowCount); } }
        public ResultTable AddTable(Enum tableName, ResultTable table=null) {return AddTable(tableName.ToString());}
        public ResultTable AddTable(string tableName, ResultTable table = null)
        {
            if(string.IsNullOrEmpty(tableName)) throw new GeomethodEtlException("Table name is null or empty.");
            if (tables.Exists(t => t.Name == tableName)) throw new GeomethodEtlException("Table name not unique in result set: "+tableName);
            if (table != null)
            {
                if (tables.Contains(table)) throw new GeomethodEtlException("Table already attached: " + table.Name);
                table.Name = tableName;
            }
            else
            {
                table = new ResultTable(tableName);
            }
            tables.Add(table);
            return table;
        }
        public ResultTable CreateTable()
        {
            var table = new ResultTable(null);
            return table;
        }
        public ResultTable GetTableStrong(Enum tableName) { return GetTableStrong(tableName.ToString()); }
        public ResultTable GetTableStrong(string tableName)
        {
            var table=tables.FirstOrDefault(t => t.Name == tableName);
            if (table == null)
            {
                table = AddTable(tableName);
            }
            return table;
        }
    }

}
