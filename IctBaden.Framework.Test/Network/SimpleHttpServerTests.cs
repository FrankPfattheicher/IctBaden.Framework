using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IctBaden.Framework.Network;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Test.Network;

[CollectionDefinition("TcpClientServerTests", DisableParallelization = true)]
public sealed partial class SimpleHttpServerTests : IDisposable
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ITestOutputHelper _testOutputHelper;
    private int _testPort = NetworkInfo.GetFreeLocalTcpPort();
    private readonly TestHttpServer _httpServer;
    private readonly HttpClient _client;

    public SimpleHttpServerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _httpServer = new TestHttpServer(_testPort);
        _httpServer.Start();
        
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
    }

    public void Dispose()
    {
        _client.Dispose();
        _httpServer.Terminate();
        _testPort++;
    }

    [Fact]
    public async Task GetRequestSuccess()
    {
        string? response = null;

        try
        {
            response = await _client.GetStringAsync($"http://localhost:{_testPort}/Test.htm", TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }

        Assert.NotNull(response);
        Assert.Contains("Test äöüß", response);
    }

    [Fact]
    public async Task GetRequestWithParameters()
    {
        string? response = null;

        try
        {
            response = await _client.GetStringAsync($"http://localhost:{_testPort}/Query.htm?a=123&b=x%20y", TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }

        Assert.NotNull(response);
        Assert.Contains("a=123", response);
        Assert.Contains("b=x y", response);
    }

    [Fact]
    public async Task GetRequestFail()
    {
        string? response = null;

        try
        {
            await _client.GetStringAsync($"http://localhost:{_testPort}/Unknown.htm", TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            response = ex.Message;
        }

        Assert.NotNull(response);
        Assert.Contains("404", response);
    }

    [Fact]
    public async Task PostRequestSuccess()
    {
        const string data = "{ Data: 1234 }";
        string? response = null;

        try
        {
            using var content = new StringContent(data, Encoding.UTF8, "application/json");
            using var httpResponse = await _client.PostAsync($"http://localhost:{_testPort}/Write", content, TestContext.Current.CancellationToken);
            response = await httpResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }

        Assert.NotNull(response);
        Assert.Equal(data, response);
    }

    [Fact]
    public async Task MultipleParallelGetRequests()
    {
        var requests = Enumerable.Range(1, 10)
            .Select(_ => Task.Run(GetRequestSuccess))
            .ToArray();
        await Task.WhenAll(requests);
    }

}
