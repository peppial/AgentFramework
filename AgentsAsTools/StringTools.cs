namespace MultiAgent.AgentAsTool;

public static class StringTools
{
    public static int CountWords(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        return input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static string TitleCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }

    public static string RemoveSpaces(string input)
    {
        return input.Replace(" ", "");
    }

    public static int CountCharacters(string input)
    {
        return input?.Length ?? 0;
    }

    public static int CountVowels(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        return input.Count(c => "aeiouAEIOU".Contains(c));
    }

    public static int[] ExtractNumbers(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Array.Empty<int>();

        var numbers = new List<int>();
        var currentNumber = "";

        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                currentNumber += c;
            }
            else if (currentNumber.Length > 0)
            {
                numbers.Add(int.Parse(currentNumber));
                currentNumber = "";
            }
        }

        if (currentNumber.Length > 0)
            numbers.Add(int.Parse(currentNumber));

        return numbers.ToArray();
    }
}
