// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

using System.Collections.Generic;
using System.Reflection;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable IntroduceOptionalParameters.Global

namespace IctBaden.Framework.IniFile
{
    using System;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Text.RegularExpressions;
    using AppUtils;
    using Types;

    public class Profile
    {
        public ProfileSectionCollection Sections { get; private set; }

        public string FileName { get; private set; }
        public DateTime LastFileChange { get; private set; }
        public Encoding FileEncoding { get; private set; }
        public bool ReadOnly { get; private set; }

        /// <summary>
        /// Lines starting with this are treated as comments
        /// </summary>
        public string CommentLines { get; private set; }

        // ReSharper disable UnusedMember.Global
        public static string DefaultFileName =>
            Path.Combine(AssemblyInfo.Default.DataPath, AssemblyInfo.Default.ExeBaseName + ".cfg");

        public static string CurrentDirectoryFileName => Path.Combine(Directory.GetCurrentDirectory(),
            AssemblyInfo.Default.ExeBaseName + ".cfg");

        public static string LocalToExeFileName =>
            Path.Combine(ApplicationInfo.ApplicationDirectory, AssemblyInfo.Default.ExeBaseName + ".cfg");
        // ReSharper restore UnusedMember.Global

        public Profile(string profileName)
            : this(profileName, Encoding.Default)
        {
        }

        public Profile(string profileName, Encoding desiredEncoding)
            : this(profileName, desiredEncoding, null)
        {
        }

        public Profile(string profileName, Encoding desiredEncoding, string commentLines)
        {
            FileName = profileName;
            FileEncoding = desiredEncoding;
            CommentLines = commentLines;
            Sections = new ProfileSectionCollection(this);
            if (!string.IsNullOrEmpty(profileName) && File.Exists(profileName))
            {
                LastFileChange = File.GetLastWriteTime(profileName);
                ReadOnly = (File.GetAttributes(profileName) & FileAttributes.ReadOnly) != 0;
                Load();
            }
            else
            {
                ReadOnly = false;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static Profile FromResource(Assembly assembly, string resourceName)
        {
            var profile = new Profile("res://" + resourceName) {ReadOnly = true};

            resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceName));
            if (resourceName == null) return null;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;

                // Assembly resources are stored in default (Windows) encoding as the files are.
                using (var reader = new StreamReader(stream, Encoding.Default))
                {
                    profile.LoadContent(reader);
                }
            }

            return profile;
        }

        // ReSharper disable once UnusedMember.Global
        public void SetReadOnly()
        {
            ReadOnly = true;
        }

        public ProfileSection this[string name, int index] => this[$"{name}{index}"];

        public ProfileSection this[string name]
        {
            get
            {
                var section = Sections
                    .FirstOrDefault(s => string.Compare(s.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (section != null)
                {
                    return section;
                }

                var newSection = new ProfileSection(this, name);
                Sections.Add(newSection);
                return newSection;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public bool HasSection(string name, int index)
        {
            return HasSection($"{name}{index}");
        }

        public bool HasSection(string name)
        {
            return Sections.Any(s => string.Compare(s.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static bool NeedsUnicode(string text)
        {
            var enc = Encoding.ASCII;
            var data = enc.GetBytes(text);
            var ascii = enc.GetString(data);
            return text != ascii;
        }

        #region LoadAndSave

        private static readonly Regex SectionFmt = new Regex(@"^\[(.*)\]", RegexOptions.Compiled);
        private static readonly Regex KeyFmt = new Regex(@"^([^=]+)=(.*)$", RegexOptions.Compiled);

        private void LoadContent(TextReader content)
        {
            var currentSection = new ProfileSection(this, ProfileSection.UnnamedGlobalSectionName);
            Sections.Add(currentSection);

            while (true)
            {
                var line = content.ReadLine();
                if (line == null) break;

                if (string.IsNullOrEmpty(line))
                    continue;
                if ((CommentLines != null) && line.StartsWith(CommentLines))
                    continue;

                var isSection = SectionFmt.Match(line);
                if (isSection.Success)
                {
                    var name = isSection.Groups[1].Value;
                    currentSection = new ProfileSection(this, name);
                    Sections.Add(currentSection);
                    continue;
                }

                var isKey = KeyFmt.Match(line);
                if (!isKey.Success)
                    continue;

                var thisKey = new ProfileKey(currentSection, isKey.Groups[1].Value);
                thisKey.LoadValue(isKey.Groups[2].Value);
                currentSection.Keys.Add(thisKey);
            }
        }

        public bool Load()
        {
            lock (FileName)
            {
                if (!File.Exists(FileName))
                    return false;

                try
                {
                    Sections.Clear();

                    using (var fileData = new StreamReader(FileName, FileEncoding, true))
                    {
                        string firstLine = null;
                        while (string.IsNullOrEmpty(firstLine) && !fileData.EndOfStream)
                        {
                            firstLine = fileData.ReadLine();
                        }

                        if (!FileEncoding.Equals(Encoding.Unicode) && !FileEncoding.Equals(Encoding.BigEndianUnicode) &&
                            EncodingDetector.IsUnicode(firstLine))
                        {
                            FileEncoding = Encoding.Unicode;
                        }
                        else
                        {
                            FileEncoding = fileData.CurrentEncoding;
                        }

                        fileData.Close();
                    }

                    using (var fileData = new StreamReader(FileName, FileEncoding, true))
                    {
                        LoadContent(fileData);
                        fileData.Close();
                    }

                    return true;
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    return false;
                }
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public bool Save()
        {
            lock (FileName)
            {
                if (ReadOnly)
                    return false;

                try
                {
                    var lines = new List<string>();
                    foreach (var section in Sections.Where(section => section.Keys.Count != 0))
                    {
                        lines.Add(section.Header);

                        var sectionLines = section.Keys
                            .Where(key => !string.IsNullOrEmpty(key.StringValue))
                            .Select(key => key.ToString());
                        lines.AddRange(sectionLines);

                        lines.Add(string.Empty);
                    }

                    File.WriteAllLines(FileName, lines, FileEncoding);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.Message);
                    return false;
                }
            }
        }

        #endregion
    }
}