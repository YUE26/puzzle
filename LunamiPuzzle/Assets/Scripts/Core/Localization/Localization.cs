using System;
using GamePlay;

namespace Core.Localization
{
    public enum LangState
    {
        en,
        ch
    }
    
    public static class Localization
    {
        public static Action LanguageUpdate;
        public static LangState _language;
        
        public static void SwitchToEnglish()
        {
            _language = LangState.en;
            LanguageUpdate?.Invoke();
        }

        public static void SwitchToChinese()
        {
            _language = LangState.ch;
            LanguageUpdate?.Invoke();
        }

        public static LangState GetCurLanguage()
        {
            return _language;
        }
        
        public static string Load_Gameplay(int id)
        {
            if (CsvModule.Csv.LocalizationGameplayCfgStore.TryGetValue(id, out var lc))
            {
                var text = GetLocalizedText(lc);
                return text;
            }
            return String.Empty;
        }

        public static string Load_UI(int id)
        {
            if (CsvModule.Csv.LocalizationUICfgStore.TryGetValue(id, out var lc))
            {
                var text = GetLocalizedText(lc);
                return text;
            }
            return String.Empty;
        }
        
        private static string GetLocalizedText(LocalizationGameplayCfg lc)
        {
            if (GetCurLanguage() == LangState.ch)
            {
                return lc.ch;
            }

            if (GetCurLanguage() == LangState.en)
            {
                return lc.en;
            }

            return String.Empty;
        }

        private static string GetLocalizedText(LocalizationUICfg lc)
        {
            if (GetCurLanguage() == LangState.ch)
            {
                return lc.ch;
            }

            if (GetCurLanguage() == LangState.en)
            {
                return lc.en;
            }

            return String.Empty;
        }
    }
}