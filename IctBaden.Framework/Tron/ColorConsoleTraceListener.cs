using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IctBaden.Framework.Types;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Tron
{
    public class ColorConsoleTraceListener : TextWriterTraceListener
    {
        private readonly Dictionary<TraceEventType, ConsoleColor> _typeColors;
        public ConsoleColor CurrentColor { get; private set; }

        public static ConsoleColor GetConsoleColor(Color color)
        {
            ConsoleColorConverter.TryGetConsoleColor(color, out var cc);
            return cc;
        }

        public ColorConsoleTraceListener()
        {
            _typeColors = new Dictionary<TraceEventType, ConsoleColor>
            {
                { TraceEventType.Critical, ConsoleColor.Red },
                { TraceEventType.Error, ConsoleColor.Red },
                { TraceEventType.Warning, ConsoleColor.Yellow },
                { TraceEventType.Information, ConsoleColor.White },
                { TraceEventType.Verbose, ConsoleColor.Gray },
                { TraceEventType.Start, ConsoleColor.Green },
                { TraceEventType.Stop, ConsoleColor.Green },
                { TraceEventType.Suspend, ConsoleColor.DarkBlue },
                { TraceEventType.Resume, ConsoleColor.Blue },
                { TraceEventType.Transfer, ConsoleColor.DarkGray }
            };

            Writer = Console.Out;
        }

        private void SetColorOnEventType(TraceEventType eventType)
        {
            var color = _typeColors[eventType];
            if (color == CurrentColor) return;

            CurrentColor = color;
            Console.ForegroundColor = color;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            try
            {
                SetColorOnEventType(eventType);
                base.TraceEvent(eventCache, source, eventType, id, format, args);
            }
            catch
            {
                // ignore
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            try
            {
                SetColorOnEventType(eventType);
                base.TraceEvent(eventCache, source, eventType, id, message);
            }
            catch
            {
                // ignore
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            try
            {
                SetColorOnEventType(eventType);
                base.TraceEvent(eventCache, source, eventType, id);
            }
            catch
            {
                // ignore
            }
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            try
            {
                SetColorOnEventType(TraceEventType.Transfer);
                base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
            }
            catch
            {
                // ignore
            }
        }
    }
}