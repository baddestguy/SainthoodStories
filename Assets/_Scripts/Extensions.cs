using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Extensions 
{
    public static T GetRandomEnumValue<T>() where T : Enum
    {
        Array enumValues = Enum.GetValues(typeof(T));
        int randomIndex = UnityEngine.Random.Range(0, enumValues.Length);
        return (T)enumValues.GetValue(randomIndex);
    }

    public static bool TryExtractColorFromRichText(string richText, out Color color)
    {
        color = Color.white; // Default value in case of failure

        // Regex to find <color="#RRGGBB"> or <color=colorname>
        Match match = Regex.Match(richText, @"<color=[""']?#?([0-9A-Fa-f]{6})[""']?>");

        if (match.Success)
        {
            string hexCode = match.Groups[1].Value;
            return ColorUtility.TryParseHtmlString("#" + hexCode, out color);
        }

        // For named colors like <color="white">, you'd need a mapping or manual parsing
        // Example for "white":
        match = Regex.Match(richText, @"<color=[""']?(white)[""']?>", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string colorName = match.Groups[1].Value.ToLower();
            if (colorName == "white")
            {
                color = Color.white;
                return true;
            }
            // Add more named colors as needed
        }

        return false;
    }
}
