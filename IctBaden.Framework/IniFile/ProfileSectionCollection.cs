
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
            return this.Any(section => section.Name == name);
        }

        public ProfileSection this[string name]
        {
            get
            {
                foreach (var section in this)
                {
                    if (section.Name == name)
                        return section;
                }
                var newSection = new ProfileSection(_profile, name);
                Add(newSection);
                return newSection;
            }
        }
    }
}
