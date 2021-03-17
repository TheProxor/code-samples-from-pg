using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

namespace Drawmasters.Helpers
{
    public static class StringExtensions
    {
        public static string SafeStringFormat(this string text, params object[] args)
        {
            if (text == null)
            {
                CustomDebug.Log("Text cannot be null!");
                return string.Empty;
            }

            List<int> argIndexes = FindArgIndexes();


            if (argIndexes.Count != args.Length)
            {
                string argsString = CollectionToString(args);
                string indexString = CollectionToString(argIndexes);

                if (argIndexes.Count < args.Length)
                {
                    CustomDebug.Log("Args count less than string params count!\n" +
                                    $"String: <b>'{text}'</b>\n" +
                                    $"Args: <b>{argsString}</b>\n" +
                                    $"Indexes: <b>{indexString}</b>");
                }
                else if (argIndexes.Count > args.Length)
                {
                    CustomDebug.Log("String params count less than args count!\n" +
                                    $"String: <b>'{text}'</b>\n" +
                                    $"Args: <b>{argsString}</b>\n" +
                                    $"Indexes: <b>{indexString}</b>");
                }
            }

            foreach (var argIndex in argIndexes)
            {
                if (argIndex < args.Length && argIndex >= 0)
                {
                    text = text.Replace("{" + argIndex.ToString() + "}", args[argIndex].ToString());
                }
                else
                {
                    CustomDebug.Log($"There is no index {argIndex} among the string params args!");
                }             
            }

            return text;

            List<int> FindArgIndexes()
            {
                List<int> result = new List<int>(args.Length);

                foreach (var match in new Regex(@"{\d{1,}}").Matches(text))
                {
                    string matchString = match.ToString();

                    matchString = matchString.Remove(0, 1);
                    matchString = matchString.Remove(matchString.Length - 1, 1);

                    int index = int.Parse(matchString);

                    result.Add(index);
                }

                return result;
            }

            string CollectionToString(ICollection collection)
            {
                string result = string.Empty;

                result += "(";

                foreach (var item in collection)
                {
                    result += $"{item}, ";
                }

                result = result.Remove(result.Length - 2, 2);
                result += ")";

                return result;
            }
        }
    }
}