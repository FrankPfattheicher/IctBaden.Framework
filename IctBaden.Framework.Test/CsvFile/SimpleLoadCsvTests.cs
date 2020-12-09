using System.Linq;
using Xunit;

namespace IctBaden.Framework.Test.CsvFile
{
    public class SimpleLoadCsvTests
    {
        [Fact]
        public void LoadFileShouldReadAllLinesAndColumns()
        {
            var file = new Framework.CsvFile.CsvFile("SimpleTab.csv");
            file.Load();

            Assert.Equal(4, file.Columns.Count);
            Assert.Equal(10, file.DataRows.Count);
            Assert.Empty(file.InvalidRows);
            Assert.True(string.IsNullOrEmpty(file.LoadError));
            Assert.Equal('\t', file.Separator);
        }
        
        [Fact]
        public void LoadFileShouldDetectSeparatorCharacter()
        {
            var file = new Framework.CsvFile.CsvFile("SimpleSemicolon.csv");
            file.Load();

            Assert.Equal(4, file.Columns.Count);
            Assert.Equal(10, file.DataRows.Count);
            Assert.Empty(file.InvalidRows);
            Assert.True(string.IsNullOrEmpty(file.LoadError));
            Assert.Equal(';', file.Separator);
        }

        [Fact]
        public void LoadFileShouldIncludeQuotes()
        {
            var file = new Framework.CsvFile.CsvFile("SimpleQuotes.csv");
            file.Load();

            Assert.True(file.DataRows.All(row => row.Fields.Last().Contains('"')));
        }

        [Fact]
        public void LoadFileShouldRemoveQuotes()
        {
            var file = new Framework.CsvFile.CsvFile("SimpleSemicolon.csv") {RemoveQuotes = true};
            file.Load();

            Assert.DoesNotContain(file.DataRows, row => row.Fields.Last().Contains('"'));
        }

    }
}