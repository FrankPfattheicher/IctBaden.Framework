using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework
{
    public class CommandlinePropertyProvider : PropertyBag
    {
        public CommandlinePropertyProvider()
            : this(Environment.GetCommandLineArgs().Skip(1))
        {
        }

        public CommandlinePropertyProvider(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var setting = arg.Split('=');
                if (setting.Length != 2) continue;

                Set(setting[0], setting[1]);
            }
        }
    }
}
