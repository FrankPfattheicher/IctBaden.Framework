using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using IctBaden.Framework.Types;
// ReSharper disable UnusedMember.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Framework.CsvFile
{
    public class CsvFile
    {
        public string FileName { get; private set; }
        public DateTime LastFileChange { get; private set; }
        public string LoadError { get; private set; }
        public char Separator { get; set; }
        public Encoding FileEncoding { get; set; }
        public List<string> Columns { get; set; }
        public List<CsvData> DataRows { get; set; }
        public List<CsvData> InvalidRows { get; set; }
        public bool RemoveQuotes { get; set; }

        public CsvFile(string csvFileName)
        {
            FileName = csvFileName;
            Separator = '\t';
            RemoveQuotes = false;
            FileEncoding = Encoding.Default;
            Columns = new List<string>();
            DataRows = new List<CsvData>();
            InvalidRows = new List<CsvData>();
        }

        public bool Load()
        {
            LoadError = string.Empty;

            if (!File.Exists(FileName))
            {
                LoadError = "File does not exist.";
                return false;
            }

            LastFileChange = File.GetLastWriteTime(FileName);
            DataRows = new List<CsvData>();
            try
            {
                var lineNumber = 0;
                using (var fileData = new StreamReader(FileName, FileEncoding, true))
                {
                    while (!fileData.EndOfStream)
                    {
                        lineNumber++;
                        var lineData = fileData.ReadLine();
                        if (string.IsNullOrEmpty(lineData))
                            continue;

                        if (lineNumber == 1)
                        {
                            DetectSeparator(lineData);
                            Columns.Clear();
                            var columns = lineData
                                .Split(Separator)
                                .Select(TextEscaping.RemoveQuotes);
                            Columns.AddRange(columns);
                            continue;
                        }

                        var data = new CsvData(lineNumber, Columns, lineData);
                        var fields = lineData.Split(Separator);
                        if (RemoveQuotes)
                        {
                            fields = fields
                                .Select(TextEscaping.RemoveQuotes)
                                .ToArray();
                        }
                        data.Fields.AddRange(fields);
                        if (data.Fields.Count == Columns.Count)
                        {
                            DataRows.Add(data);
                        }
                        else
                        {
                            LoadError = "Invalid rows detected.";
                            InvalidRows.Add(data);
                        }
                    }

                    FileEncoding = fileData.CurrentEncoding;
                    fileData.Close();
                }
                return true;
            }
            catch (IOException ex)
            {
                LoadError = ex.Message;
                Trace.TraceError(ex.Message);
            }
            return false;
        }

        private void DetectSeparator(string lineData)
        {
            var possibleSeparators = new[] { '\t', ';', ',' };

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var separator in possibleSeparators)
            {
                if (lineData.IndexOf(separator) != -1)
                {
                    Separator = separator;
                    return;
                }
            }
        }

        public bool Save()
        {
            try
            {
                using (var fileData = new StreamWriter(FileName, false, FileEncoding))
                {
                    var lineData = string.Join(Separator.ToString(), Columns.ToArray());
                    fileData.WriteLine(lineData);

                    foreach (var row in DataRows)
                    {
                        lineData = string.Join(Separator.ToString(), row.Fields.ToArray());
                        fileData.WriteLine(lineData);
                    }

                    fileData.Close();
                }
                return true;
            }
            catch (IOException ex)
            {
                Trace.TraceError(ex.Message);
            }
            return false;
        }

        public List<T> LoadData<T>() where T : new()
        {
            return Load()
                ? DataRows.Select(row => row.GetObject<T>()).ToList()
                : new List<T>();
        }

    }
}
