using IctBaden.Framework.CsvFile;
using Xunit;

namespace IctBaden.Framework.Test.CsvFile;

public class CsvDataTests
{
    [Fact]
    public void CsvDataFromObjectShouldContainAllProperties()
    {
        var obj = new CsvDataObject();
        var expected = obj.GetType().GetProperties().Length;

        var csvData = CsvData.FromObject(obj);
        Assert.Equal(expected, csvData.Columns.Count);
        Assert.Equal(expected, csvData.Fields.Count);
    }

    [Fact]
    public void CsvDataFromObjectShouldConvertableBackToObject()
    {
        var obj = new CsvDataObject();
        var csvData = CsvData.FromObject(obj);

        var backData = csvData.GetObject<CsvDataObject>();
            
        Assert.True(PropertiesEqual(obj, backData));
    }


    private static bool PropertiesEqual(object a, object b)
    {
        var type = a.GetType();
        if (b.GetType() != type) return false;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var propertyInfo in type.GetProperties())
        {
            var aValue = propertyInfo.GetValue(a)?.ToString() ?? "";
            var bValue = propertyInfo.GetValue(b)?.ToString() ?? "";
            if (aValue != bValue) return false;
        }
            
        return true;
    }
        
}