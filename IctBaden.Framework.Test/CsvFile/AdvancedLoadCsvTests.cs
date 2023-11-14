using System.Linq;
using Xunit;

namespace IctBaden.Framework.Test.CsvFile
{
    public class AdvancedLoadCsvTests
    {
        [Fact]
        public void LoadFileShouldDetectSeparatorCharacterWithLineBreaks()
        {
            var file = new Framework.CsvFile.CsvFile("QuotedWithLineBreaks.csv")
            {
                RemoveQuotes = true
            };
            file.Load();

            Assert.Equal(4, file.Columns.Count);
            Assert.Equal(10, file.DataRows.Count);
            Assert.Empty(file.InvalidRows);
            Assert.True(string.IsNullOrEmpty(file.LoadError));
            Assert.Equal(';', file.Separator);
            Assert.DoesNotContain(file.DataRows, row => row.Fields.Last().Contains('"'));
        }

        [Fact]
        public void LoadFileShouldDetectSeparatorCharacterInQuotedHeader()
        {
            var file = new Framework.CsvFile.CsvFile("QuotedHeaderAndFields.csv")
            {
                RemoveQuotes = true
            };
            file.Load();

            Assert.Equal(27, file.Columns.Count);
            Assert.Equal(10, file.DataRows.Count);
            Assert.Empty(file.InvalidRows);
            Assert.True(string.IsNullOrEmpty(file.LoadError));
            Assert.Equal(',', file.Separator);
            Assert.DoesNotContain(file.DataRows, row => row.Fields.Last().Contains('"'));
        }

        

    }
}