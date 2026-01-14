using UnityEngine;

namespace Polychievements;


public static class PrefsHelper
{
    private const string Key = "Polychievements";
    public static Dictionary<string, bool> CreateDefault()
    {
        var dict = new Dictionary<string, bool>();
        for (int i = 0; i < Main.Achievements.Count; i++)
            dict[Main.Achievements[i].idx] = false;
        return dict;
    }

    public static void SaveDict(Dictionary<string, bool> dict)
    {
        var parts = new List<string>();
        foreach (var kvp in dict)
            parts.Add($"{kvp.Key}={(kvp.Value ? 1 : 0)}");

        string encoded = string.Join(";", parts);
        PlayerPrefs.SetString(Key, encoded);
        PlayerPrefs.Save();
    }

    public static Dictionary<string, bool> LoadDict()
    {
        var dict = CreateDefault();

        if (!PlayerPrefs.HasKey(Key))
            return dict;

        string encoded = PlayerPrefs.GetString(Key);

        foreach (var entry in encoded.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            var kv = entry.Split('=');
            if (kv.Length != 2)
                continue;

            string key = kv[0].Trim();
            string value = kv[1].Trim();

            if (!int.TryParse(value, out int val))
                continue;

            if (!dict.ContainsKey(key))
                continue;

            dict[key] = val != 0;
        }

        return dict;
    }
}