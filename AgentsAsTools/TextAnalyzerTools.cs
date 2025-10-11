namespace MultiAgent.AgentAsTool;

public static class TextAnalyzerTools
{
    public static TextStatistics AnalyzeText(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return new TextStatistics();
        }

        var stats = new TextStatistics();

        // Character count
        stats.TotalCharacters = input.Length;

        // Word analysis
        var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        stats.TotalWords = words.Length;

        // Sentence count (approximate by counting . ! ?)
        stats.TotalSentences = Math.Max(1,
            input.Count(c => c == '.' || c == '!' || c == '?'));

        // Vowels and consonants
        stats.TotalVowels = input.Count(c => "aeiouAEIOU".Contains(c));
        stats.TotalConsonants = input.Count(c => char.IsLetter(c) && !"aeiouAEIOU".Contains(c));

        // Word length analysis
        if (words.Length > 0)
        {
            stats.AverageWordLength = words.Average(w => w.Length);
            stats.LongestWord = words.OrderByDescending(w => w.Length).First();
            stats.LongestWordLength = stats.LongestWord.Length;
            stats.UniqueWords = words.Select(w => w.ToLower()).Distinct().Count();
        }

        // Reading time (average reading speed: 200 words per minute)
        stats.ReadingTimeSeconds = (stats.TotalWords / 200.0) * 60;

        return stats;
    }

    public static string GetTextSummary(string input)
    {
        var stats = AnalyzeText(input);
        return stats.ToString();
    }

    public static double GetReadabilityScore(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var sentences = Math.Max(1, input.Count(c => c == '.' || c == '!' || c == '?'));

        if (words.Length == 0)
            return 0;

        // Simplified Flesch Reading Ease approximation
        // Score: 0-30 (Very Difficult), 30-50 (Difficult), 50-60 (Fairly Difficult),
        // 60-70 (Standard), 70-80 (Fairly Easy), 80-90 (Easy), 90-100 (Very Easy)
        double avgWordLength = words.Average(w => w.Length);
        double avgWordsPerSentence = (double)words.Length / sentences;

        // Simplified score based on word and sentence complexity
        double score = 206.835 - (1.015 * avgWordsPerSentence) - (84.6 * (avgWordLength / 5.0));

        return Math.Max(0, Math.Min(100, score));
    }
}
