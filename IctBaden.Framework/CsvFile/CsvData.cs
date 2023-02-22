using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IctBaden.Framework.Types;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

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
            if ((index < 0) || (index >= Fields.Count)) return "";
            return Fields[index];
        }

        public bool IsEmpty => Fields.All(string.IsNullOrEmpty);

        public string this[int columnIndex] => Fields[columnIndex];
        public string this[string columnName] => GetField(columnName);

        public T GetObject<T>() where T : new()
        {
            return GetObject<T>(CultureInfo.InvariantCulture);
        }

        public T GetObject<T>(CultureInfo cultureInfo) where T : new()
        {
            var obj = new T();

            // Set public properties
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var columnIndex = ColumnNames
                    .FindIndex(c => string.Compare(c, propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if(columnIndex == -1)
                    continue;

                var value = UniversalConverter.ConvertToType(GetField(Columns[columnIndex]), propertyInfo.PropertyType, cultureInfo);
                propertyInfo.SetValue(obj, value, null);
            }

            // Set public fields
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var columnIndex = ColumnNames
                    .FindIndex(c => string.Compare(c, fieldInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if(columnIndex == -1)
                    continue;

                var value = UniversalConverter.ConvertToType(GetField(Columns[columnIndex]), fieldInfo.FieldType, cultureInfo);
                fieldInfo.SetValue(obj, value);
            }

            return obj;
        }


        public static CsvData FromObject(object obj) => FromObject(obj, CultureInfo.InvariantCulture);

        public static CsvData FromObject(object obj, CultureInfo cultureInfo)
        {
            var columns = obj.GetType().GetProperties()
                .Select(prop => prop.Name)
                .ToList();

            var csvData = new CsvData(-1, columns, "");
            
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                var columnIndex = csvData.ColumnNames
                    .FindIndex(c => string.Compare(c, propertyInfo.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if(columnIndex == -1)
                    continue;

                var value = UniversalConverter.ConvertToType(propertyInfo.GetValue(obj), typeof(string), cultureInfo)?.ToString() ?? $"{obj}";
                csvData.Fields.Add(value);                
            }

            return csvData;
        }
        
    }
}
