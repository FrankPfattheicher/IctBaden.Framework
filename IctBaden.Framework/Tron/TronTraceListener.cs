using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace IctBaden.Framework.Tron
{
    public class TronTraceListener : TraceListener
    {
        private readonly bool _useColor;
        private readonly Dictionary<TraceEventType, Color> _typeColors;
        private Color _currentColor;

        // ReSharper disable once UnusedMember.Global
        public TronTraceListener()
            : this(false)
        {
        }
        public TronTraceListener(bool useColor)
        {
            _useColor = useColor;
            _typeColors = new Dictionary<TraceEventType, Color>
            {
                { TraceEventType.Critical, TraceColor.FatalError },
                { TraceEventType.Error, TraceColor.Error },
                { TraceEventType.Warning, TraceColor.Warning },
                { TraceEventType.Information, TraceColor.Info },
                { TraceEventType.Verbose, TraceColor.Text },
                { TraceEventType.Start, TraceColor.Green },
                { TraceEventType.Stop, TraceColor.Green },
                { TraceEventType.Suspend, TraceColor.DarkBlue },
                { TraceEventType.Resume, TraceColor.DarkBlue },
                { TraceEventType.Transfer, TraceColor.DarkGray }
            };
        }

        public override void Write(string message)
        {
            TronTrace.Print(message);
        }

        public override void WriteLine(string message)
        {
            TronTrace.PrintLine(message);
        }

        private void SetColorOnEventType(TraceEventType eventType)
        {
            if (!_useColor) return;

            var color = _typeColors[eventType];
            if (color == _currentColor) return;

            _currentColor = color;
            TronTrace.SetColor(color);
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
            catch (Exception)
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