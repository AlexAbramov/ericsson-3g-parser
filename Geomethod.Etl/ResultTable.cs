using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geomethod.Etl
{
    public class ResultTable
    {
        public string Name { get; internal set; }
        ColumnSet colSet;
        ColumnSet colSet2;
        string[] row = null;
        List<string[]> rows = new List<string[]>();

        public ColumnSet ColumnSet { get { return colSet; } }
        public ColumnSet ColumnSet2 { get { return colSet2; } }

        public IEnumerable<string> Columns { get { return colSet.Columns.Union(colSet2.Columns); } }
        public IEnumerable<string[]> Rows { get { return rows; } }
        public int ColumnCount { get { return colSet.Count + colSet2.Count; } }
        public int RowCount { get { return rows.Count; } }

        public ResultTable(string name)
        {
            Name = name;
            colSet = new ColumnSet(this);
            colSet2 = new ColumnSet(this);
        }

        public void Clear()
        {
            colSet.Clear();
            colSet2.Clear();
            row = null;
            rows.Clear();
        }

        public void ToCsv(StreamWriter sw, char sep=';')
        {
            var csvConverter = new CsvConverter(sep);
            var s = csvConverter.ToCsvLine(Columns);
            sw.WriteLine(s);
            foreach(var r in rows)
            {
                s = csvConverter.ToCsvLine(r);
                sw.WriteLine(s);
            }
        }

        public bool IsRestructureAllowed {
            get { return row == null; }
        }


        internal void CheckRestructureAllowed()
        {
            if (row != null)
            {
                throw new GeomethodEtlException("ResultTable restructure not allowed when data present.");
            }
        }

        internal string[] GetRow()
        {
            if (row == null)
            {
                row = new string[ColumnCount];
                colSet.CopyValues(row);
                colSet2.CopyValues(row, colSet.Count);
            }
            return row;
        }

        public void AddRow()
        {
            rows.Add(GetRow());
            this.row = new string[ColumnCount];
        }

        public string[] GetRow(int i)
        {
            return rows[i];
        }
    }
}
