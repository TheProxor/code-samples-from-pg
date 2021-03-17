using System;


namespace Drawmasters.Utils
{
    public static class CSVParseUtility
    {
        private const string Separator = ",";
        private const string ArraySeparator = "/";


        public static string[] ParseRowsToArray(string line, StringSplitOptions options = StringSplitOptions.None) =>
            line.Split(new string[] { Separator }, options);

        public static string[] ParseToArray(string row, StringSplitOptions options = StringSplitOptions.None) =>
            row.Split(new string[] { ArraySeparator }, options);


        public static int[] ParseToIntArray(string row, StringSplitOptions options = StringSplitOptions.None)
        {
            if (string.IsNullOrEmpty(row))
            {
                return Array.Empty<int>();
            }

            string[] stringsArray = ParseToArray(row, options);
            return Array.ConvertAll(stringsArray, int.Parse);
        }
    }
}
