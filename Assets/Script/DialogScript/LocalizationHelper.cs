using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocalizationHelper
{
    public static string GetText(string key, string fallbackValue)
    {
        // ==========================================
        // 🌟 后期真正做多语言时，只需在这里写一次：
        // try {
        //     string translated = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString("MyTable", key);
        //     if (!string.IsNullOrEmpty(translated)) return translated;
        // } catch { }
        // ==========================================

        return fallbackValue; // 目前直接返回原中文
    }
}