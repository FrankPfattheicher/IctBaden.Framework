using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Network;

public class SimpleHttpProcessor(Socket socket, SimpleHttpServer server)
{
    public readonly Socket? Socket = socket;
    public readonly SimpleHttpServer Server = server;

    public string? HttpMethod;
    public string? HttpUrl;
    public IDictionary<string, string>? QueryParameters;
    public string? HttpProtocolVersionString;
    public readonly Hashtable HttpHeaders = new Hashtable();

    private const int MaxPostSize = 10 * 1024 * 1024; // 10MB

    public void Process()
    {
        string exception;
        try
        {
            if (Socket == null) return;
                
            var networkStream = new NetworkStream(Socket, FileAccess.Read);
            using var inputStream = new StreamReader(networkStream);

            ParseRequest(inputStream);
            ReadHeaders(inputStream);
            if (string.Equals(HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                HandleGetRequest();
            }
            else if (string.Equals(HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                HandlePostRequest(inputStream);
            }
            return;
        }
        catch (Exception ex)
        {
            exception = ex.Message;
        }

        try
        {
            if (!string.IsNullOrEmpty(exception))
                WriteServerError(exception, "text/plain");
            else
                WriteNotFound();
        }
        catch
        {
            // ignore
        }
    }

    public void Abort()
    {
        Socket?.Close();
    }

    private void ParseRequest(TextReader inputStream)
    {
        var request = inputStream.ReadLine();
        if (request == null)
        {
            throw new Exception("invalid http request line");
        }
        var tokens = request.Split(' ');
        if (tokens.Length != 3)
        {
            throw new Exception("invalid http request line");
        }
        HttpMethod = tokens[0].ToUpper(CultureInfo.InvariantCulture);
        var route = tokens[1];
        HttpProtocolVersionString = tokens[2];

        if (route.Contains('?'))
        {
            var parts = route.Split('?');
            HttpUrl = parts[0];
            var queryString = System.Web.HttpUtility.UrlDecode(parts[1]);
            try
            {
                QueryParameters = queryString.Split('&')
                    .Select(param => param.Split('='))
                    .ToDictionary(kv => kv[0], kv => kv[^1], StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Trace.TraceError("SimpleHttpProcessor: Error parsing query parameters: " + ex.Message);
            }
        }
        else
        {
            HttpUrl = route;
        }

        Console.WriteLine("starting: " + request);
    }

    private void ReadHeaders(TextReader inputStream)
    {
        Console.WriteLine("ReadHeaders()");
        while (inputStream.ReadLine() is { } line)
        {
            if (line.Equals(""))
            {
                Console.WriteLine("got headers");
                return;
            }

            var separator = line.IndexOf(':');
            if (separator == -1)
            {
                throw new Exception("invalid http header line: " + line);
            }
            var name = line.Substring(0, separator);
            var pos = separator + 1;
            while ((pos < line.Length) && (line[pos] == ' '))
            {
                pos++; // strip any spaces
            }

            var value = line.Substring(pos, line.Length - pos);
            Console.WriteLine("header: {0}:{1}", name, value);
            HttpHeaders[name.ToLower(CultureInfo.InvariantCulture)] = value;
        }
    }

    public void HandleGetRequest()
    {
        Server.HandleGetRequest(this);
    }

    private const int BufSize = 4096;

    private void HandlePostRequest(StreamReader inputStream)
    {
        // this post data processing just reads everything into a memory stream.
        // this is fine for smallish things, but for large stuff we should really
        // hand an input stream to the request processor. However, the input stream 
        // we hand him needs to let him see the "end of the stream" at this content 
        // length, because otherwise he won't know when he's seen it all! 

        Console.WriteLine("get post data start");
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        var toRead = 0;
        if (HttpHeaders.ContainsKey("content-length"))
        {
            var contentLen = Convert.ToInt32(HttpHeaders["content-length"], CultureInfo.InvariantCulture);
            if (contentLen > MaxPostSize)
            {
                throw new Exception($"POST Content-Length({contentLen}) too big for this simple server");
            }
            toRead = contentLen;
        }

        var buf = new char[BufSize];
        if (toRead > 0)
        {
            while (toRead > 0)
            {
                Console.WriteLine("starting Read, toRead={0}", toRead);

                var numRead = inputStream.Read(buf, 0, Math.Min(BufSize, toRead));
                Console.WriteLine("read finished, numRead={0}", numRead);
                if (numRead == 0)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (toRead == 0)
                    {
                        break;
                    }

                    throw new Exception("client disconnected during post");
                }
                toRead -= numRead;
                bw.Write(buf, 0, numRead);
            }
        }
        else
        {
            Console.WriteLine("starting Read, toRead=<END>");

            var text = inputStream.ReadToEnd();
            Console.WriteLine("read finished, text.Length={0}", text.Length);
            bw.Write(buf, 0, text.Length);
        }
        bw.Flush();
        ms.Seek(0, SeekOrigin.Begin);

        Console.WriteLine("get post data end");
        using var streamReader = new StreamReader(ms);
        Server.HandlePostRequest(this, streamReader);
    }

    public void WriteSuccess(string result, string contentType)
    {
        WriteStringResponse(HttpStatusCode.OK, result, contentType);
    }

    public void WriteNotFound()
    {
        WriteStringResponse(HttpStatusCode.NotFound, null, null);
    }

    public void WriteServerError(string result, string contentType)
    {
        WriteStringResponse(HttpStatusCode.InternalServerError, result, contentType);
    }

    public void WriteStringResponse(HttpStatusCode statusCode, string? body, string? contentType)
    {
        if (Socket is not { Connected: true }) return;
            
        using var outputStream = new StreamWriter(new NetworkStream(Socket, FileAccess.Write), new UTF8Encoding(false));
        outputStream.NewLine = "\r\n";

        using var httpResponseMessage = new HttpResponseMessage(statusCode);
        var reasonPhrase = httpResponseMessage.ReasonPhrase;
        outputStream.WriteLine($"HTTP/1.0 {(int)statusCode} {reasonPhrase}");

        // these are the HTTP headers
        if (!string.IsNullOrEmpty(contentType))
        {
            outputStream.WriteLine("Content-Type: " + contentType);
        }
        outputStream.WriteLine("Connection: close");
        outputStream.WriteLine(""); // this terminates the HTTP headers

        if (body != null)
        {
            // finally add the body data
            outputStream.Write(body);
        }
    }

}