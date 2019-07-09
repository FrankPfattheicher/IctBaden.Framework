using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Channels
{
    internal class ModemTrigger
    {
        public string Text { get; }

        public ModemTrigger(string triggerText, ModemResult triggerResult)
        {
            Text = triggerText;
            Result = triggerResult;
            Reset();
        }

        public void Reset()
        {
            Count = 0;
            Triggered = false;
        }

        public int Length => Text.Length;

        public int Count { get; private set; }

        private bool IsReady => (Text != null) && !Triggered;

        public bool IsActive => Count > 0;

        public bool Triggered { get; private set; }

        public ModemResult Result { get; }

        public bool Match(char theChar)
        {
            if (!IsReady)
                return false;

            if ((Count < Length) && (theChar == Text[Count]))
            {
                Count++;
                if (Count >= Length)
                    Triggered = true;
            }
            else
                Count = 0;

            return Triggered;
        }

    }

    internal class ModemTriggerCollection
    {
        private readonly List<ModemTrigger> _list;
        private readonly ModemTrigger _noneTrigger;

        public ModemTriggerCollection()
        {
            _list = new List<ModemTrigger>
               {
                 new ModemTrigger("OK", ModemResult.OK),
                 new ModemTrigger("CONNECT", ModemResult.CONNECT),
                 new ModemTrigger("RING", ModemResult.RING),
                 new ModemTrigger("NO CARRIER", ModemResult.NO_CARRIER),
                 new ModemTrigger("+CME ERROR", ModemResult.CME_ERROR),
                 new ModemTrigger("+CMS ERROR", ModemResult.CMS_ERROR),
                 new ModemTrigger("ERROR", ModemResult.ERROR),
                 // ReSharper disable once StringLiteralTypo
                 new ModemTrigger("NO DIALTONE", ModemResult.NO_DIALTONE),
                 new ModemTrigger("BUSY", ModemResult.BUSY),
                 new ModemTrigger("NO ANSWER", ModemResult.NO_ANSWER),
                 new ModemTrigger("VOICE", ModemResult.VOICE),
                 new ModemTrigger("DELAYED", ModemResult.DELAYED),
                 new ModemTrigger("NO MORE DIALLING", ModemResult.NO_MORE_DIALLING),
                 new ModemTrigger("CARRIER", ModemResult.CARRIER),
                 new ModemTrigger("COMPRESSION", ModemResult.COMPRESSION),
                 new ModemTrigger("PROTOCOL", ModemResult.PROTOCOL),
                 new ModemTrigger("DIAL LOCKED", ModemResult.DIAL_LOCKED),
                 new ModemTrigger("RINGING", ModemResult.RINGING)
               };
            _noneTrigger = new ModemTrigger(string.Empty, ModemResult.None);
        }

        public void SetUserTrigger(ModemResult result, string pattern)
        {
            if ((result < ModemResult.User1) || (result > ModemResult.User8))
                return;

            foreach (var mt in _list)
            {
                if (mt.Result == result)
                {
                    _list.Remove(mt);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pattern))
                _list.Add(new ModemTrigger(pattern, result));
        }

        public void Reset()
        {
            foreach (var mt in _list)
                mt.Reset();
        }

        public ModemTrigger Match(char theChar)
        {
            var resetLowerThan = 0;
            var checkTriggers = false;
            foreach (var mt in _list)
            {
                if (mt.Match(theChar))
                {
                    // reset all lower triggers (i.e. less characters)
                    resetLowerThan = Math.Max(resetLowerThan, mt.Length);
                    checkTriggers = true;
                }
                else if (mt.Triggered)
                {
                    checkTriggers = true;
                }
            }

            var total = 0;
            ModemTrigger triggered = null;
            if (checkTriggers)
            {
                // if there is only one trigger remaining return this
                foreach (var mt in _list)
                {
                    // reset all lower triggers (i.e. less characters)
                    if ((resetLowerThan > 0) && mt.IsActive && (mt.Count < resetLowerThan))
                        mt.Reset();

                    if (mt.IsActive)
                    {
                        triggered = mt;
                        total++;
                    }
                }
            }

            if ((total == 1) && (triggered != null) && (triggered.Triggered))
            {
                triggered.Reset();
                return triggered;
            } // Total == 1

            return _noneTrigger;
        }
    }
}