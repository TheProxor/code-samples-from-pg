using System.Text.RegularExpressions;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class FeedbackValidator
    {
        #region Fields

        public enum Error
        {
            None = 0,
            TooShort = 1,
            IsEmpty = 2,
            BadFormat = 3
        }

        static readonly Regex InputValidateRegex = new Regex(@"([a-zA-Z0-9-_]([a-zA-Z0-9-_]| (?! ))*[a-zA-Z0-9-_]*)?");
        static readonly Regex MultipleSpacesRegex = new Regex(@"\s{2,}");

        static readonly Regex FinalNameRegex =
            new Regex(@"[a-zA-Z0-9-_]([a-zA-Z0-9-_]| (?=[a-zA-Z0-9-_]))*[a-zA-Z0-9-_]*");

        #endregion



        #region Public methods

        public static bool IsInputValid(string inputText)
        {
            Match match = InputValidateRegex.Match(inputText);
            return match.Success && match.Value.Equals(inputText) && inputText.Length <= 1000;
        }


        public static bool TryDeleteMultipleSpaces(string inputText, out string outputText)
        {
            outputText = inputText;

            bool wasTextChanged = false;
            foreach (Match multipleSpaceMatch in MultipleSpacesRegex.Matches(outputText))
            {
                outputText = outputText.Replace(multipleSpaceMatch.Value, " ");
                wasTextChanged = true;
            }

            return wasTextChanged;
        }


        public static Error TryGetName(string inputText, out string correctName)
        {
            correctName = inputText.Trim();
            TryDeleteMultipleSpaces(correctName, out correctName);

            if (correctName.Length == 0)
            {
                correctName = null;
                return Error.IsEmpty;
            }

            Match match = FinalNameRegex.Match(correctName);
            if (!match.Success || !match.Value.Equals(correctName))
            {
                correctName = null;
                return Error.BadFormat;
            }

            if (correctName.Length < 3)
            {
                correctName = null;
                return Error.TooShort;
            }

            return Error.None;
        }

        #endregion
    }
}