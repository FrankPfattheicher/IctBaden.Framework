namespace IctBaden.Framework.Types
{
    using System;

    public class ValidatedGuid
    {
        /// <summary>
        /// Zeigt an, ob der angegebene Wert gültig ist
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        /// Guid-Wert
        /// </summary>
        public readonly Guid Guid;



        /// <summary>
        /// Zeigt an, ob ein Wert vorhanden ist (nicht zwingend valid)
        /// </summary>
        public readonly bool HasValue;


        /// <summary>
        /// Zeigt an, ob es sich um eine Empty Guid handelt
        /// </summary>
        public readonly bool IsEmptyGuid;


        /// <summary>
        /// String-Repräsentation
        /// </summary>
        public string Text { get { return Guid.ToString("D"); } }

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

            if (data is Guid)
            {
                Guid = (Guid)data;
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
            if (Guid.TryParse(text, out Guid))
            {
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