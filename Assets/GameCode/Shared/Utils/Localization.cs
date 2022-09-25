using FNZ.Shared.Model;
using FNZ.Shared.Model.String;

namespace FNZ.Shared.Utils
{

	public class Localization
	{
		public enum Language
		{
			ENGLISH = 0,
			SWEDISH = 1,
			GERMAN = 2,
			FRENCH = 3,
			SPANISH = 4,
		}

		private static Language m_ActiveLanguage = Language.ENGLISH;

		public static string GetString(string id)
		{
			string translation = null;
			switch (m_ActiveLanguage)
			{
				case Language.ENGLISH:
					return DataBank.Instance.GetData<StringData>(id).en;

				case Language.SWEDISH:
					translation = DataBank.Instance.GetData<StringData>(id).sv;
					break;

				case Language.GERMAN:
					translation = DataBank.Instance.GetData<StringData>(id).de;
					break;

				case Language.FRENCH:
					translation = DataBank.Instance.GetData<StringData>(id).fr;
					break;

				case Language.SPANISH:
					translation = DataBank.Instance.GetData<StringData>(id).es;
					break;

				default:
					return DataBank.Instance.GetData<StringData>(id).en;
			}

			if(string.IsNullOrEmpty(translation))
            {
				return DataBank.Instance.GetData<StringData>(id).en;
			}

			return translation;
		}

		public static void SetActiveLanguage(Language language)
		{
			m_ActiveLanguage = language;
		}

		public static Language GetActiveLanguage()
		{
			return m_ActiveLanguage;
		}
	}
}