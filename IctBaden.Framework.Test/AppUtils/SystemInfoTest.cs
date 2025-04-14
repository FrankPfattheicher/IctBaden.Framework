using System;
using System.IO;
using System.Threading.Tasks;
using IctBaden.Framework.AppUtils;
using Xunit;
using Xunit.Abstractions;

namespace IctBaden.Framework.Test.AppUtils;

public class SystemInfoTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SystemInfoTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void GetDiskUsageShouldReturnPercentage()
    {
        var currentDrive = Path.GetPathRoot(Environment.CurrentDirectory) ?? "C";
        var usage = SystemInfo.GetDiskUsage(currentDrive);

        _testOutputHelper.WriteLine($"Disk({currentDrive}) Usage: {usage}%");
        Assert.True(usage > 1.0f);
        Assert.True(usage < 100.0f);
    }
        
    [Fact]
    public async Task GetCpuUsageShouldReturnPercentage()
    {
        SystemInfo.GetCpuUsage();
        await Task.Delay(2000);
        var usage = SystemInfo.GetCpuUsage();

        _testOutputHelper.WriteLine($"CPU Usage: {usage}%");
        Assert.True(usage > 1.0f);
        Assert.True(usage < 100.0f);
    }

}