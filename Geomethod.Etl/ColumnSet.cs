using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geomethod.Etl
{
    public class ColumnSet
    {
        ResultTable table;
        Dictionary<string, int> colDict = new Dictionary<string, int>();
        List<string> colValues = new List<string>();
        Dictionary<int, int> valueMapping = new Dictionary<int, int>();
        public ColumnSet(ResultTable table) { this.table = table; }
        public void Clear()
        {
            colDict.Clear();
            colValues.Clear();
            valueMapping.Clear();
            valueMapping.Clear();
        }
        public IEnumerable<string> Columns { get { return colDict.Keys; } }
        public int Count {
            get { return colDict.Count; }
        }
        public bool ContainsColumn(string name) { return colDict.ContainsKey(name); }
        public void AddColumn(Enum colName, string val = null) { AddColumn(colName.ToString(), val); }
        public void AddColumn(string colName, string val = null)
        {
            table.CheckRestructureAllowed();
            colDict.Add(colName, colDict.Count);
            colValues.Add(val);
        }
        public void AddColumn(string colName, int xmlIndex)
        {
            AddColumn(colName);
            valueMapping.Add(xmlIndex, Count - 1);
        }
        public void AddValueMapping(int index, string colName)
        {
            valueMapping.Add(index, GetIndex(colName));
        }
        int GetIndex(string colName) { return colDict[colName]; }
        int GetIndex(int xmlIndex)
        {
            return valueMapping.ContainsKey(xmlIndex) ? valueMapping[xmlIndex] : -1;
        }
        int GeIndexOffset()
        {
            return table.ColumnSet2 == this ? table.ColumnSet.Count : 0;
        }
        internal void CopyValues(string[] row, int offset=0)
        {
            colValues.CopyTo(row, offset);
        }
        public bool SetValue(Enum colName, string val, bool canAddCol = false) { return SetValue(colName.ToString(), val, canAddCol); }
        public bool SetValue(string colName, string val, bool canAddCol=false)
        {
            if (ContainsColumn(colName))
            {
                table.GetRow()[GetIndex(colName) + GeIndexOffset()] = val;
                return true;
            }
            else if(canAddCol && table.IsRestructureAllowed)
            {
                AddColumn(colName, val);
                return true;
            }
            return false;
        }
        public bool SetValue(int xmlIndex, string val)
        {
            var index = GetIndex(xmlIndex);
            if (index >= 0)
            {
                table.GetRow()[index + GeIndexOffset()] = val;
                return true;
            }
            return false;
        }
    }

}
