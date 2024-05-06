using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Excursion360.Desktop.Services;

public interface IStateImagesMetricsStore
{
    void IncrementImageHit(string path);
    string[] MostPopularUrls();
}

public partial class InMemoryIStateImagesMetricsStore : IStateImagesMetricsStore
{
    /// <summary>
    /// Количества получений картинок по адресу. ключ = адрес картинки. Значение - количество получений.
    /// </summary>
    private readonly ConcurrentDictionary<string, int>
        hitCounts = new();
    private Regex stateAndImageRegex = GetStateAndImageRegex();
    public void IncrementImageHit(string path)
    {
        var match = stateAndImageRegex.Match(path);
        if (!match.Success)
        {
            return;
        }
        hitCounts.AddOrUpdate(path, 1, (_, old) => old + 1);
        Console.WriteLine(JsonSerializer.Serialize(hitCounts, new JsonSerializerOptions
        {
            WriteIndented = true,
        }));
    }
    public string[] MostPopularUrls()
    {
        return hitCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Select(kvp => kvp.Key)
            .Take(10)
            .ToArray();
    }

    [GeneratedRegex(@"state_.+[/\\].+\.jpg$")]
    private static partial Regex GetStateAndImageRegex();

}
