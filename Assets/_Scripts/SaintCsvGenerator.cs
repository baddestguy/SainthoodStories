#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class SaintCsvGenerator : EditorWindow
{
    // Inputs
    private string keyPrefix = "SANMARTIN";
    private string saintName = "St. Martin de Porres";
    private string storyTitle = "The Broom of God's Joy";
    private Vector2 scroll;
    [TextArea(10, 40)]
    private string fullStory = "";

    // Settings
    private int maxCharsPerChunk = 160; // soft limit for each row
    private int maxClausesPerChunk = 2; // "maybe two at most"

    [MenuItem("Tools/Saint CSV Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<SaintCsvGenerator>("Saint CSV Generator");
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Saint CSV Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        keyPrefix = EditorGUILayout.TextField("Key Prefix", keyPrefix);
        saintName = EditorGUILayout.TextField("Saint Name (001)", saintName);
        storyTitle = EditorGUILayout.TextField("Story Title (002)", storyTitle);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Full Story Text", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        fullStory = EditorGUILayout.TextArea(fullStory, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        maxCharsPerChunk = EditorGUILayout.IntSlider("Max chars per row", maxCharsPerChunk, 60, 260);
        maxClausesPerChunk = EditorGUILayout.IntSlider("Max clauses per row", maxClausesPerChunk, 1, 4);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate CSV", GUILayout.Height(40)))
        {
            GenerateCsv();
        }

        if (GUILayout.Button("Preview Chunks", GUILayout.Height(30)))
        {
            var normalized = NormalizeStory(fullStory);
            var chunks = SplitStoryIntoChunks(normalized);

            StringBuilder preview = new StringBuilder();
            for (int i = 0; i < chunks.Count; i++)
            {
                preview.AppendLine($"[{i + 1}] {chunks[i]}");
            }

            EditorUtility.DisplayDialog("Preview", preview.ToString(), "OK");
        }
    }

    private void GenerateCsv()
    {
        if (string.IsNullOrWhiteSpace(keyPrefix))
        {
            EditorUtility.DisplayDialog("Error", "Key Prefix cannot be empty.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(saintName))
        {
            EditorUtility.DisplayDialog("Error", "Saint Name (001) cannot be empty.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(storyTitle))
        {
            EditorUtility.DisplayDialog("Error", "Story Title (002) cannot be empty.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(fullStory))
        {
            EditorUtility.DisplayDialog("Error", "Full Story Text cannot be empty.", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Save Saint CSV",
            Application.dataPath,
            keyPrefix.ToUpperInvariant() + ".csv",
            "csv"
        );

        if (string.IsNullOrEmpty(path))
            return;

        string normalizedText = NormalizeStory(fullStory);
        var chunks = SplitStoryIntoChunks(normalizedText);

        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine("Key,English");

        int index = 1;
        string prefix = keyPrefix.ToUpperInvariant();

        // 001: Saint name
        WriteCsvRow(sb, $"{prefix}_{index:000}", saintName);
        index++;

        // 002: Story title
        WriteCsvRow(sb, $"{prefix}_{index:000}", storyTitle);
        index++;

        // 003+: story chunks
        foreach (var chunk in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunk))
                continue;

            WriteCsvRow(sb, $"{prefix}_{index:000}", chunk.Trim());
            index++;
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success",
            $"CSV generated with {index - 1} rows.\n\nSaved to:\n{path}", "OK");
    }

    /// <summary>
    /// Normalizes text:
    /// - Converts fancy apostrophes to '
    /// - Removes superscript numerals
    /// - Softens hyphens
    /// - Normalizes line endings and whitespace
    /// </summary>
    private string NormalizeStory(string text)
    {
        string result = text;

        // Normalize line endings
        result = result.Replace("\r\n", "\n").Replace("\r", "\n");

        // Replace curly apostrophes with straight apostrophe
        result = result.Replace('\u2019', '\'').Replace('\u2018', '\'');

        // Remove superscript numerals: ¹²³⁰ⁱ etc.
        result = Regex.Replace(result, "[\u00B2\u00B3\u00B9\u2070-\u2079]", "");

        // Collapse multiple spaces
        result = Regex.Replace(result, @"[ \t]+", " ");

        // Trim
        result = result.Trim();

        return result;
    }

    /// <summary>
    /// Splits the story into reasonably short chunks:
    /// - First by sentence boundaries (. ! ?)
    /// - Then further splits long sentences by commas into up to N clauses per row
    /// </summary>
    private List<string> SplitStoryIntoChunks(string text)
    {
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        // Pre-split text into dash-defined sections
        var dashSections = SplitDashSections(text);

        foreach (var section in dashSections)
        {
            // Then split each dash-section into sentences
            string[] sentences = Regex.Split(section, @"(?<=[\.!\?])\s+");

            foreach (var sentenceRaw in sentences)
            {
                string sentence = sentenceRaw.Trim();
                if (string.IsNullOrEmpty(sentence)) continue;

                // If short enough, just use as is
                if (sentence.Length <= maxCharsPerChunk)
                {
                    chunks.Add(sentence);
                    continue;
                }

                // Otherwise split by commas into clauses
                string[] clauses = SplitByCommasOutsideQuotes(sentence);
                var currentBuilder = new StringBuilder();
                int clauseCountInChunk = 0;

                void FlushChunk()
                {
                    if (currentBuilder.Length > 0)
                    {
                        string chunk = currentBuilder.ToString().Trim();

                        // If mid-sentence, add soft continuation mark
                        if (!EndsSentence(chunk))
                            chunk += " ...";  // or "..." if you prefer

                        chunk = CapitalizeFirstLetter(chunk);
                        chunks.Add(chunk);

                        currentBuilder.Length = 0;
                        clauseCountInChunk = 0;
                    }
                }

                foreach (var clauseRaw in clauses)
                {
                    string clause = clauseRaw.Trim();
                    if (string.IsNullOrEmpty(clause))
                        continue;

                    // If starting a new chunk
                    if (currentBuilder.Length == 0)
                    {
                        currentBuilder.Append(clause);
                        clauseCountInChunk = 1;
                    }
                    else
                    {
                        // Check if adding this clause would exceed limits
                        string candidate = currentBuilder.ToString() + ", " + clause;
                        // Try not to start a chunk with a weak clause (and/but/however/etc.)
                        bool weak = IsWeakClauseStart(clause);

                        if (!weak && (candidate.Length > maxCharsPerChunk || clauseCountInChunk >= maxClausesPerChunk))
                        {
                            FlushChunk();
                            currentBuilder.Append(CapitalizeFirstLetter(clause));
                            clauseCountInChunk = 1;
                        }
                        else
                        {
                            currentBuilder.Append(", ");
                            currentBuilder.Append(clause);
                            clauseCountInChunk++;
                        }
                    }
                }

                FlushChunk();
            }
        }

        return chunks;
    }

    private void WriteCsvRow(StringBuilder sb, string key, string text)
    {
        sb.Append(key);
        sb.Append(',');
        sb.Append(EscapeCsv(text));
        sb.AppendLine();
    }

    /// <summary>
    /// Standard CSV quoting:
    /// - If value contains comma, quote, or newline, wrap in quotes
    /// - Double any internal quotes
    /// </summary>
    private string EscapeCsv(string value)
    {
        if (value == null)
            return "";

        bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n");
        string v = value.Replace("\"", "\"\"");

        if (mustQuote)
            return $"\"{v}\"";

        return v;
    }

    private string CapitalizeFirstLetter(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        char[] chars = s.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsLetter(chars[i]))
            {
                chars[i] = char.ToUpper(chars[i]);
                break;
            }
        }

        return new string(chars);
    }

    private string[] SplitByCommasOutsideQuotes(string sentence)
    {
        var parts = new System.Collections.Generic.List<string>();
        var sb = new StringBuilder();
        bool insideQuotes = false;

        for (int i = 0; i < sentence.Length; i++)
        {
            char c = sentence[i];

            if (c == '"')
                insideQuotes = !insideQuotes;

            if (c == ',' && !insideQuotes)
            {
                parts.Add(sb.ToString());
                sb.Length = 0;
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0)
            parts.Add(sb.ToString());

        return parts.ToArray();
    }

    private bool IsWeakClauseStart(string clause)
    {
        string[] weakStarts = { "and ", "but ", "or ", "so ", "yet ",
                            "because ", "however ", "then ", "thus ",
                            "therefore ", "meanwhile ", "in ", "at ", "for " };

        string lower = clause.ToLowerInvariant() + " ";

        foreach (var w in weakStarts)
            if (lower.StartsWith(w))
                return true;

        return false;
    }

    private bool EndsSentence(string chunk)
    {
        return chunk.EndsWith(".") || chunk.EndsWith("!") || chunk.EndsWith("?");
    }

    private List<string> SplitDashSections(string text)
    {
        var results = new List<string>();

        // Pattern 1: Detect text wrapped in em dashes: — like this —
        string patternWrapped = @"—([^—]+)—";
        // Pattern 2: Detect standalone em-dash breaks: part — part
        string patternBreak = @"([^—]+)—([^—]+)";

        // First split on wrapped sections
        var tokens = Regex.Split(text, patternWrapped);

        foreach (var token in tokens)
        {
            if (string.IsNullOrWhiteSpace(token))
                continue;

            string trimmed = token.Trim();
            bool isWrapped = Regex.IsMatch(text, $"—{Regex.Escape(trimmed)}—");

            if (isWrapped)
            {
                // This entire part is a standalone chunk
                results.Add(CapitalizeFirstLetter(trimmed));
            }
            else
            {
                // Now split on em-dash sentence breaks inside non-wrapped text
                var innerParts = Regex.Split(trimmed, @"\s*—\s*");
                foreach (var p in innerParts)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                        results.Add(CapitalizeFirstLetter(p.Trim()));
                }
            }
        }

        return results;
    }

}
#endif