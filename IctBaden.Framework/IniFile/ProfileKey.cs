using System;
using System.Globalization;

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.IniFile;

public class ProfileKey
{
    private readonly ProfileSection _section;
    private string? _keyValue;

    public ProfileKey(ProfileSection section, string name)
    {
        _section = section;
        Name = name.Trim();
    }

    public override string ToString()
    {
        return Name + "=" +_keyValue;
    }

    public string Name { get; }

    internal void LoadValue(string newValue)
    {
        _keyValue = newValue.Trim();
    }

    public object? ObjectValue
    {
        get => _keyValue;
        set
        {
            if (value == null)
            {
                if (_keyValue == null)
                    return;
                _keyValue = null;
            }
            else
            {
                var strVal = value switch
                {
                    DateTime dt => dt.ToString("O"),
                    DateTimeOffset dt => dt.ToString("O"),
                    _ => string.Format(CultureInfo.InvariantCulture, "{0}", value).Trim()
                };
                if (string.Equals(_keyValue, strVal, StringComparison.Ordinal))
                    return;
                _keyValue = strVal;
            }
            _section.Save();
        }
    }

    /// <summary>
    /// Key value as string
    /// If value is null the key is not written
    /// empty string will write key 
    /// </summary>
    public string? StringValue
    {
        get => _keyValue;
        set
        {
            var strVal = value?.Trim();
            if (string.Equals(_keyValue, strVal, StringComparison.Ordinal))
                return;
            _keyValue = strVal;
            _section.Save();
        }
    }

    public long LongValue
    {
        get
        {
            long.TryParse(_keyValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var longVal);
            return longVal;
        }
        set
        {
            var strVal = value.ToString(CultureInfo.InvariantCulture).Trim();
            if (string.Equals(_keyValue, strVal, StringComparison.Ordinal))
                return;
            _keyValue = strVal;
            _section.Save();
        }
    }

    // ReSharper disable once UnusedMember.Global
    public double DoubleValue
    {
        get
        {
            double.TryParse(_keyValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue);
            return doubleValue;
        }
        set
        {
            var strVal = value.ToString("N", CultureInfo.InvariantCulture).Trim();
            if (string.Equals(_keyValue, strVal, StringComparison.Ordinal))
                return;
            _keyValue = strVal;
            _section.Save();
        }
    }

    public bool BoolValue
    {
        get
        {
            if (!bool.TryParse(_keyValue, out var boolVal))
            {
                boolVal = string.Equals(StringValue, "1", StringComparison.Ordinal);
            }
            return boolVal;
        }
        set
        {
            var strVal = value ? "1" : "0";
            if (string.Equals(_keyValue, strVal, StringComparison.Ordinal))
                return;
            _keyValue = strVal;
            _section.Save();
        }
    }

    // ReSharper disable once UnusedMember.Global
    public string GetStringValue(string defaultValue) => _keyValue ?? defaultValue;

    // ReSharper disable once UnusedMember.Global
    public long GetLongValue(long defaultValue) => _keyValue != null ? LongValue : defaultValue;

    // ReSharper disable once UnusedMember.Global
    public bool GetBoolValue(bool defaultValue) => _keyValue != null ? BoolValue : defaultValue;
}