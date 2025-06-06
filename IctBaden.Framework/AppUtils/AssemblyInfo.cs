﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Framework.AppUtils;

public class AssemblyInfo
{
    public static Assembly DefaultAssembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    public static readonly AssemblyInfo Default = new AssemblyInfo();

    private readonly Assembly _assembly;
    private readonly AssemblyContactAttribute _contact;

    public AssemblyInfo() : this(DefaultAssembly)
    {
    }

    public AssemblyInfo(Assembly infoAssembly)
    {
        _assembly = infoAssembly;
        if ((_assembly.GetCustomAttributes(typeof(AssemblyContactAttribute), true) is AssemblyContactAttribute[]
                contacts) && (contacts.Length > 0))
            _contact = contacts[0];
        else
            _contact = new AssemblyContactAttribute();
    }

    public string Version => _assembly.GetName().Version?.ToString() ?? string.Empty;

    public string DisplayVersion
    {
        get
        {
            var raw = Version.Split('.');
            if (raw.Length != 4)
            {
                return Version;
            }

            var display = new StringBuilder();
            display.Append(raw[0]);
            display.Append('.');
            display.Append(int.Parse(raw[1], CultureInfo.InvariantCulture).ToString("D2", CultureInfo.InvariantCulture));
            display.Append('.');
            display.Append(int.Parse(raw[2], CultureInfo.InvariantCulture).ToString("D3", CultureInfo.InvariantCulture));
            var code = int.Parse(raw[3], CultureInfo.InvariantCulture);
            if (code > 0)
                display.Append(char.ConvertFromUtf32('a' - 1 + code));
            return display.ToString();
        }
    }

    private T? GetCustomAttribute<T>()
    {
        var attribute = _assembly
            .GetCustomAttributes(typeof(T), inherit: true)
            .FirstOrDefault(a => a.GetType() == typeof(T));
        return (T?)attribute;
    }

    public string ExeBaseName => Path.GetFileNameWithoutExtension(_assembly.Location);

    public string ExePath
    {
        get
        {
            var location = _assembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                // NET 5 and up packed applications
                return AppContext.BaseDirectory;
            }

            return Path.GetDirectoryName(_assembly.Location) ?? ".";
        }
    }

    private string GetPath(string name) => Path.Combine(ApplicationInfo.ApplicationDirectory, name);
    public string DataPath => GetPath("Data");


    public string SettingsFileName => Path.ChangeExtension(Path.Combine(DataPath, ExeBaseName), "cfg");
    public string LocalSettingsFileName => Path.ChangeExtension(Path.Combine(ExePath, ExeBaseName), "cfg");

    public DateTime FileTime => File.GetLastWriteTime(_assembly.Location);

    public string Title => GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? ExeBaseName;
    public string Description => GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;

    public string Configuration =>
        GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? string.Empty;

    public string CompanyName => GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

    public string CompanyAddress => _contact.Address ?? string.Empty;
    public string CompanyCity => _contact.City ?? string.Empty;
    public string CompanyMail => _contact.Mail ?? string.Empty;
    public string CompanyPhone => _contact.Phone ?? string.Empty;
    public string CompanyFax => _contact.Fax ?? string.Empty;
    public string CompanyMobile => _contact.Mobile ?? string.Empty;
    public string CompanyUrl => _contact.Url ?? string.Empty;
    public string Product => GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? string.Empty;
    public string Copyright => GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;
    public string Trademark => GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark ?? string.Empty;
    public string Culture => GetCustomAttribute<AssemblyCultureAttribute>()?.Culture ?? string.Empty;

    public string NeutralResourcesLanguage =>
        GetCustomAttribute<NeutralResourcesLanguageAttribute>()?.CultureName ?? string.Empty;

    public bool IsDebugBuild => _assembly.GetCustomAttributes(inherit: false).OfType<DebuggableAttribute>()
        .Any(da => da.IsJITTrackingEnabled);
}