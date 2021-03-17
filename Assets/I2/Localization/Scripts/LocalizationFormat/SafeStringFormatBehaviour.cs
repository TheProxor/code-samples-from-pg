namespace I2.Loc
{
    public class SafeStringFormatBehaviour : ILocalizationFormatBehaviour
    {
        public string SetFormat(Localize localize, object[] args)
        {
            string result = Localize.MainTranslation;

            if (args != null && args.Length > 0)
            {
                result = result.SafeStringFormat(args);
            }

            return result;
        }
    }
}