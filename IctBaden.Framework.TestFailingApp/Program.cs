using System.Diagnostics;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.TestFailingApp;

internal static class Program
{
    public static void Main()
    {
        Console.WriteLine("Failing Test App");
        Trace.Listeners.Add(new ConsoleTraceListener());

        PostMortemDebugging.Enable(PostMortemDebugging.HandlerMode.ExitApplication);

        var thread = new Thread(Start);
        thread.Start();
        
        Console.WriteLine("Thread started");

        Task.Delay(1000).Wait();
        
        Trace.TraceInformation(">>> should not get here");
        throw new Exception("should not get here");
    }

    private static void Start()
    {
        Trace.TraceInformation(">>> NotImplementedException in thread");
        throw new Exception("Should be caught by PostMortemDebugging");
    }
}