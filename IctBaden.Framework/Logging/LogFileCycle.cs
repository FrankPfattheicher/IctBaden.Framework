namespace IctBaden.Framework.Logging;

public enum LogFileCycle
{
    /// Do NOT write files
    None = -1,
    OneFile,
    Daily,
    Weekly,
    Monthly,
    Yearly
}