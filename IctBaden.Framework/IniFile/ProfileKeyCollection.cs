using System;

namespace IctBaden.Framework.IniFile
{
    using System.Collections.Generic;
    using System.Linq;

    public class ProfileKeyCollection : List<ProfileKey>
    {
        public bool Contains(string key)
        {
            return this.Any(k => string.Compare(k.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}
