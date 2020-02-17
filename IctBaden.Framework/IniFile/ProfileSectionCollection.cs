
// ReSharper disable UnusedMember.Global

using System;

namespace IctBaden.Framework.IniFile
{
    using System.Collections.Generic;
    using System.Linq;

    public class ProfileSectionCollection : List<ProfileSection>
    {
        private readonly Profile _profile;

        public ProfileSectionCollection(Profile parentProfile)
        {
            _profile = parentProfile;
        }

        public bool Contains(string name)
        {
            return this.Any(section => string.Compare(section.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public ProfileSection this[string name]
        {
            get
            {
                var section = this
                    .FirstOrDefault(s => string.Compare(s.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0);
                if (section != null)
                {
                    return section;
                }

                var newSection = new ProfileSection(_profile, name);
                Add(newSection);
                return newSection;
            }
        }
    }
}
