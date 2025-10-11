namespace MultiAgent.AgentAsTool;

public class TextStatistics
{
    public int TotalCharacters { get; set; }
    public int TotalWords { get; set; }
    public int TotalSentences { get; set; }
    public int TotalVowels { get; set; }
    public int TotalConsonants { get; set; }
    public double AverageWordLength { get; set; }
    public int LongestWordLength { get; set; }
    public string LongestWord { get; set; } = "";
    public int UniqueWords { get; set; }
    public double ReadingTimeSeconds { get; set; }

    public override string ToString()
    {
        return $@"Text Statistics:
- Total Characters: {TotalCharacters}
- Total Words: {TotalWords}
- Total Sentences: {TotalSentences}
- Total Vowels: {TotalVowels}
- Total Consonants: {TotalConsonants}
- Average Word Length: {AverageWordLength:F2} characters
- Longest Word: '{LongestWord}' ({LongestWordLength} characters)
- Unique Words: {UniqueWords}
- Estimated Reading Time: {ReadingTimeSeconds:F1} seconds";
    }
}
