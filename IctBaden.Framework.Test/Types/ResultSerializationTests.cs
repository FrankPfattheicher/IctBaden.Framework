using System.Text.Json;
using Xunit;

namespace IctBaden.Framework.Test.Types;

public class ResultSerializationTests
{
    [Fact]
    public void OkResultOfTShouldBeSerialzable()
    {
        var result = Result<string>.Ok("TEST");
        
        var json = JsonSerializer.Serialize(result);

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json);
        
        Assert.NotNull(deserialized);
        Assert.Equal(result.Success, deserialized.Success);
        Assert.Equal(result.Data, deserialized.Data);
        Assert.Equal(result.Errors, deserialized.Errors);
    }

    [Fact]
    public void FailedResultOfTShouldBeSerialzable()
    {
        var result = Result<string>.Failed("TEST");
        
        var json = JsonSerializer.Serialize(result);

        var deserialized = JsonSerializer.Deserialize<Result<string>>(json);
        
        Assert.NotNull(deserialized);
        Assert.Equal(result.Success, deserialized.Success);
        Assert.Equal(result.Data, deserialized.Data);
        Assert.Equal(result.Errors, deserialized.Errors);
    }

}