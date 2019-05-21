// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
namespace IctBaden.Framework.Types
{
    using System;

    public class ValidatedGuid
    {
        /// <summary>
        /// Zeigt an, ob der angegebene Wert gültig ist
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Guid-Wert
        /// </summary>
        public Guid Guid { get; private set; }



        /// <summary>
        /// Zeigt an, ob ein Wert vorhanden ist (nicht zwingend valid)
        /// </summary>
        public bool HasValue { get; private set; }


        /// <summary>
        /// Zeigt an, ob es sich um eine Empty Guid handelt
        /// </summary>
        public bool IsEmptyGuid { get; private set; }


        /// <summary>
        /// String-Repräsentation
        /// </summary>
        public string Text => Guid.ToString("D");

        /// <summary>
        /// Konstruktor mit automatischer Typerkennung
        /// </summary>
        /// <param name="data"></param>
        public ValidatedGuid(object data)
        {
            if (data == null)
            {
                IsValid = false;
                HasValue = false;
                IsEmptyGuid = false;
                return;
            }

            if (data is Guid guid)
            {
                Guid = guid;
                IsValid = true;
                HasValue = true;
                if (Guid == Guid.Empty)
                {
                    IsEmptyGuid = true;
                }
                return;
            }

            var text = data.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                HasValue = true;
            }
            if (Guid.TryParse(text, out var parsedGuid))
            {
                Guid = parsedGuid;
                IsValid = true;
                if (Guid == Guid.Empty)
                {
                    IsEmptyGuid = true;
                }

                return;
            }
            IsValid = false;
        }

    }
}