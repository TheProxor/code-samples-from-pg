using System.Collections.Generic;

namespace I2.Loc
{
    public class I2LocStringFormatBehaviour : ILocalizationFormatBehaviour
    {
        public string SetFormat(Localize localize, object[] args)
        {
			string result = Localize.MainTranslation;

			if (args != null && args.Length > 0)
			{
				Dictionary<string, object> parameters = new Dictionary<string, object>(args.Length);

				for (int i = 0; i < args.Length; i++)
				{
					parameters.Add(i.ToString(), args[i]);
				}

				LocalizationManager.ApplyLocalizationParams(ref result, parameters);
			}
			else
			{
				LocalizationManager.ApplyLocalizationParams(ref result, localize.gameObject, false);
			}

			return result;
        }
             
    }
}