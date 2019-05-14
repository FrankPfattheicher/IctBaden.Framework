using System;
using System.Diagnostics;
using System.IO;
using IctBaden.Framework.Network;
using Xunit;

// ReSharper disable StringLiteralTypo

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IctBaden.Framework.Test.Network
{
    public partial class SimpleHttpServerTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        internal class TestHttpServer : SimpleHttpServer
        {
            public TestHttpServer(int port)
                : base(port)
            {
            }
            public override void HandleGetRequest(SimpleHttpProcessor processor)
            {
                Debug.WriteLine("GET request: {0}", processor.HttpUrl);

                if (processor.HttpUrl.Equals("/Test.htm"))
                {
                    const string content = @"<html>
                            <body>
                                <p>Test äöüß</b>
                            </body>
                        </html>";
                    processor.WriteSuccess(content, "text/html; charset=utf-8");
                }
                else if (processor.HttpUrl.Equals("/Query.htm"))
                {
                    var content = processor.HttpUrl + Environment.NewLine;
                    foreach (var queryParameter in processor.QueryParameters)
                    {
                        content += $"{queryParameter.Key}={queryParameter.Value}" + Environment.NewLine;
                    }
                    processor.WriteSuccess(content, "text/plain; charset=utf-8");
                }
                else
                {
                    processor.WriteNotFound();
                }
            }

            public override void HandlePostRequest(SimpleHttpProcessor processor, StreamReader inputData)
            {
                Debug.WriteLine("POST request: {0}", processor.HttpUrl);
                var data = inputData.ReadToEnd();

                processor.WriteSuccess(data, "application/json; charset=utf-8");
            }
        }
    }
}