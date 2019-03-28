using System;
using System.Collections.Generic;
using System.Linq;
using IctBaden.Framework.Types;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.CsvFile
{
    public class CsvData
    {
        public int LineNumber { get; private set; }
        public string RawData { get; private set; }

        public List<string> Columns { get; private set; }
        public List<string> ColumnNames { get; private set; }
        public List<string> Fields { get; set; }

        public CsvData(int lineNumber, List<string> columns, string rawData)
        {
            LineNumber = lineNumber;
            Columns = columns;
            ColumnNames = columns
                .Select(name => name.Replace(" ", "")
                .Replace("-", "").Replace("-", ""))
                .ToList();
            RawData = rawData;
            Fields = new List<string>();
        }

        public string GetField(string columnName)
        {
            var index = Columns.IndexOf(columnName);
            if ((index < 0) || (index >= Fields.Count)) return null;
            return Fields[index];
        }

        public bool IsEmpty => Fields.All(string.IsNullOrEmpty);

        public string this[int columnIndex] => Fields[columnIndex];
        public string this[string columnName] => GetField(columnName);

        public T GetObject<T>() where T : new()
        {
            var obj = new T();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var columnIndex = ColumnNames
                    .FindIndex(c => string.Compare(c, propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if(columnIndex == -1)
                    continue;

                propertyInfo.SetValue(obj, UniversalConverter.ConvertToType(GetField(Columns[columnIndex]), propertyInfo.PropertyType), null);
            }

            return obj;
        }

    }
}
