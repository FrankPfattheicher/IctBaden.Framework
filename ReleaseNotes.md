
* 1.2.01 - 28.05.2021 ValidatedEnum.HasValue fixed.
* 1.1.10 - 18.05.2021 AssemblyInfo fixed GetCustomAttribute<T>.
* 1.1.09 - 07.05.2021 AssemblyInfo missing attributes exception fixed.
* 1.1.08 - 10.04.2021 FileLogger with tab separation.
* 1.1.07 - 31.03.2021 ValidatedEnum supporting [Flags]. Fix CronSchedule next year schedules.
* 1.1.06 - 20.03.2021 LogFileNameFactory file names simplified, FileLogger w/uppercase level.
* 1.1.05 - 17.03.2021 FileLogger with timestamp.
* 1.1.04 - 21.01.2021 FileLoggerProvider.
* 1.1.03 - 04.01.2021 Updated nuget packages.
* 1.1.02 - 19.12.2020 Add file logger.
* 1.1.01 - 14.12.2020 Add Logging support using Microsoft.Extensions.Logging.
* 1.0.28 - 09.12.2020 CsvFile fixed rows without quoted columns.
* 1.0.27 - 09.12.2020 CsvFile supporting quoted column values with line breaks.
* 1.0.26 - 23.10.2020 ApplicationInfo fixed for unit tests, new IsRunningInUnitTest
* 1.0.25 - 07.09.2020 Replaced TronTrace with System.Trace
* 1.0.24 - 15.08.2020 SocketCommandLineServer option publicReachable
* 1.0.23 - 25.07.2020 Strip milliseconds in CronSchedule.
* 1.0.22 - 17.07.2020 Added CronSchedule.
* 1.0.21 - 18.06.2020 More naming converters.
* 1.0.20 - 08.05.2020 Using async receive to handle timeouts as expected.
* 1.0.19 - 05.05.2020 Removed Windows specific TronTrace and assembly path functionality.
* 1.0.18 - 03.04.2020 Profile LocalToExeFileName using ApplicationInfo
* 1.0.17 - 27.03.2020 Added AppUtils.Shell.OpenFile
* 1.0.16 - 04.03.2020 Added ApplicationInfo as replacement for Application, new FrameworkInfo
* 1.0.15 - 24.02.2020 Refactored SimpleHttpServer.
* 1.0.14 - 17.02.2020 INI file sections are case-insensitive.
* 1.0.13 - 17.02.2020 Fixed GetApplicationDirectory on Windows.
* 1.0.12 - 12.02.2020 ConvertTo<bool> stack overflow fixed. Do not linger on sockets on Linux.
* 1.0.11 - 26.01.2020 Refactored PropertyBag to be JSON serializable.
* 1.0.10 - 24.01.2020 GetApplicationDirectory added.
* 1.0.9	- 21.01.2020 Profile Save thread safe and performance optimized.
* 1.0.8	- 07.01.2020 MicrosecondsTimer added.
* 1.0.7	- 21.12.2019 PassiveTimer using UtcNow.Ticks, WaitTask and WaitThread added.
* 1.0.6	- 04.12.2019 Interpolation fixed for input values out of range.
* 1.0.5	- 18.11.2019 SocketCommandLineServer.Reset handle not running runner thread. AssemblyInfo.IsDebugBuild added. InterpolationDouble added.
* 1.0.4	- 31.07.2019 GetHostEntry replaced by GetHostAddresses. GetHostEntry replaced by GetHostAddresses. ResourceLoader from AppDomain, Tests added.
* 1.0.3	- 09.07.2019 Added Automat, ModemChannel, PidController.
* 1.0.2	- 28.06.2019 Added ReverseBits.
* 1.0.1	- 21.05.2019 UniversalConverter converting List<T> and T[], more unit tests.
* 1.0.0	- 21.03.2019 First release migrated from full framework.
