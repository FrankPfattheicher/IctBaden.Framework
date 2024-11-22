using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace IctBaden.Framework.Test.CsvFile;

public class SimpleLoadCsvTests
{
    public SimpleLoadCsvTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
        
    [Fact]
    public void LoadFileShouldReadAllLinesAndColumns()
    {
        var file = new Framework.CsvFile("SimpleTab.csv");
        file.Load();

        Assert.Equal(4, file.Columns.Count);
        Assert.Equal(10, file.DataRows.Count);
        Assert.Empty(file.InvalidRows);
        Assert.True(string.IsNullOrEmpty(file.LoadError));
        Assert.Equal('\t', file.Separator);
        Assert.Equal("utf-8", file.FileEncoding.WebName);
    }
        
    [Fact]
    public void LoadFileWithBomShouldReadAllLinesAndColumns()
    {
        var file = new Framework.CsvFile("Utf8WithBom.csv");
        file.Load();

        Assert.Equal(4, file.Columns.Count);
        Assert.Equal(10, file.DataRows.Count);
        Assert.Empty(file.InvalidRows);
        Assert.True(string.IsNullOrEmpty(file.LoadError));
        Assert.Equal('\t', file.Separator);
        Assert.Equal("utf-8", file.FileEncoding.WebName);
    }
        
    [Fact]
    public void LoadFileWindowsEncodedWithoutBomShouldReadAllLinesAndColumns()
    {
        var file = new Framework.CsvFile("WindowsWithoutBom.csv")
        {
            FileEncoding = Encoding.GetEncoding(1250)
        };
        file.Load();

        Assert.Equal(4, file.Columns.Count);
        Assert.Equal(10, file.DataRows.Count);
        Assert.Empty(file.InvalidRows);
        Assert.True(string.IsNullOrEmpty(file.LoadError));
        Assert.Equal('\t', file.Separator);
        Assert.Contains("Eingänge", file.DataRows.Last().RawData);
        Assert.Equal("windows-1250", file.FileEncoding.WebName);
    }
        
    [Fact]
    public void LoadFileShouldDetectSeparatorCharacter()
    {
        var file = new Framework.CsvFile("SimpleSemicolon.csv");
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
        var file = new Framework.CsvFile("SimpleQuotes.csv");
        file.Load();

        Assert.True(file.DataRows.All(row => row.Fields.Last().Contains('"')));
    }

    [Fact]
    public void LoadFileShouldRemoveQuotes()
    {
        var file = new Framework.CsvFile("SimpleSemicolon.csv") {RemoveQuotes = true};
        file.Load();

        Assert.DoesNotContain(file.DataRows, row => row.Fields.Last().Contains('"'));
    }

    [Fact]
    public void SaveFileNormallyShouldNotContainBom()
    {
        var file = new Framework.CsvFile("SimpleTab.csv");
        file.Load();

        file.SaveAs("Temp.csv");

        var text = File.ReadAllBytes("Temp.csv").Take(4).ToArray();
        Assert.Equal("Numm"u8.ToArray(), text);
    }

    [Fact]
    public void SaveFileForcingBomShouldContainBom()
    {
        var file = new Framework.CsvFile("SimpleTab.csv");
        file.Load();

        file.WriteBom = true;
        file.SaveAs("Temp.csv");

        var text = File.ReadAllBytes("Temp.csv").Take(4).ToArray();
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF, (byte)'N' }, text);
    }

}