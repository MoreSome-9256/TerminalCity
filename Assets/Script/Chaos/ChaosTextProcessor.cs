using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ChaosTextProcessor
{
    private static readonly char[] chaosChars = new char[]
    {
        '█','▒','░','#','@','%','&','¥','?','!','*','~','§'
    };

    private static string ApplyChaosToText(string text, float chaos, System.Random rng)
    {
        char[] chars = text.ToCharArray();
        int length = chars.Length;

        for (int i = 0; i < length; i++)
        {
            if (char.IsWhiteSpace(chars[i])) continue;
            if (rng.NextDouble() < chaos)
            {
                chars[i] = chaosChars[rng.Next(chaosChars.Length)];
            }
        }

        return new string(chars);
    }

    private static string ApplyChaosShift(string text, float chaos, System.Random rng)
    {
        char[] chars = text.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i])) continue;
            if (rng.NextDouble() < chaos)
            {
                int shift = rng.Next(10, 80);
                chars[i] = (char)(chars[i] + shift);
            }
        }

        return new string(chars);
    }

    public static string ApplyChaosMixed(string input, float chaos)
    {
        System.Random rng = new System.Random();

        string pattern = @"(<.*?>)";
        string[] parts = Regex.Split(input, pattern);

        for (int i = 0; i < parts.Length; i++)
        {
            if (Regex.IsMatch(parts[i], pattern)) continue;

            float replaceProb = 0f;

            if (chaos >= 0.8f && chaos < 0.9f)
            {
                replaceProb = 0.1f; // 10%
                parts[i] = ApplyChaosToText(parts[i], replaceProb, rng);
            }
            else if (chaos >= 0.9f && chaos < 1.0f)
            {
                replaceProb = 0.3f; // 30%
                parts[i] = ApplyChaosShift(parts[i], replaceProb, rng);
            }
            else if (chaos >= 1.0f)
            {
                replaceProb = 0.6f; // 60% 甚至 0.8f
                parts[i] = ApplyChaosShift(ApplyChaosToText(parts[i], replaceProb, rng), replaceProb, rng);
            }
        }

        return string.Join("", parts);
    }

}
