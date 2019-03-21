using System;

namespace IctBaden.Framework.IniFile
{
    using System.Collections.Generic;
    using System.Linq;

    public class ProfileKeyCollection : List<ProfileKey>
    {
        public bool Contains(string key)
        {
            return this.Any(k => string.Compare(k.Name, 0, key, 0, 2048, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}
