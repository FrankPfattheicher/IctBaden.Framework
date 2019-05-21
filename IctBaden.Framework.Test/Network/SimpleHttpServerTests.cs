using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IctBaden.Framework.Network;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo

namespace IctBaden.Framework.Test.Network
{
    public partial class SimpleHttpServerTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private int _testPort = NetworkInfo.GetFreeLocalTcpPort();
        private readonly TestHttpServer _httpServer;

        public SimpleHttpServerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _httpServer = new TestHttpServer(_testPort);
            _httpServer.Start();
        }

        public void Dispose()
        {
            _httpServer.Terminate();
            _testPort++;
        }

        [Fact]
        public void GetRequestSuccess()
        {
            string response = null;

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    response = client.DownloadString($"http://localhost:{_testPort}/Test.htm");
                }
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

            Assert.True(response != null);
            Assert.Contains(@"Test äöüß", response);
        }

        [Fact]
        public void GetRequestWithParameters()
        {
            string response = null;

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    response = client.DownloadString($"http://localhost:{_testPort}/Query.htm?a=123&b=x%20y");
                }
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

            Assert.True(response != null);
            Assert.Contains("a=123", response);
            Assert.Contains("b=x y", response);
        }

        [Fact]
        public void GetRequestFail()
        {
            string response = null;

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadString($"http://localhost:{_testPort}/Unknown.htm");
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }

            Assert.True(response != null);
            Assert.Contains("404", response);
        }

        [Fact]
        public void PostRequestSuccess()
        {
            const string data = "{ Data: 1234 }";
            string response = null;

            try
            {
                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    response = client.UploadString($"http://localhost:{_testPort}/Write", data);
                }
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

            Assert.True(response != null);
            Assert.Equal(data, response);
        }

        [Fact]
        public void MultipleParallelGetRequests()
        {
            var requests = Enumerable.Range(1, 10)
                .Select(ix => Task.Run(() => { GetRequestSuccess(); }))
                .ToArray();
            Task.WaitAll(requests);
        }

    }
}
