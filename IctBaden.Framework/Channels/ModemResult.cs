using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Framework.Channels
{
    // ReSharper disable InconsistentNaming
    public enum ModemResult
    {
        None = 0,
        OK,
        CONNECT,
        RING,
        NO_CARRIER,
        CME_ERROR,
        CMS_ERROR,
        ERROR,
        // ReSharper disable once IdentifierTypo
        NO_DIALTONE,
        BUSY,
        NO_ANSWER,
        VOICE,
        DELAYED,
        NO_MORE_DIALLING,
        CARRIER,
        COMPRESSION,
        PROTOCOL,
        DIAL_LOCKED,
        RINGING,
        User1 = 100,
        User2,
        User3,
        User4,
        User5,
        User6,
        User7,
        User8
    }
    // ReSharper restore InconsistentNaming

    public class ModemEventArgs : EventArgs
    {
        public ModemResult Result { get; private set; }

        public ModemEventArgs(ModemResult result)
        {
            Result = result;
        }
    }
}
