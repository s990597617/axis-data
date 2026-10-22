using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axis.Scaffold;

public sealed class ScaffoldManifest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("files")]
    public List<ScaffoldFileEntry> Files { get; set; } = [];

    public static ScaffoldManifest Load(string manifestPath)
    {
        var json = File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<ScaffoldManifest>(json)
                       ?? throw new InvalidOperationException($"无法解析清单：{manifestPath}");
        if (manifest.Files.Count == 0)
            throw new InvalidOperationException("清单 files 为空。");
        return manifest;
    }
}

public sealed class ScaffoldFileEntry
{
    [JsonPropertyName("from")]
    public string From { get; set; } = "";

    [JsonPropertyName("to")]
    public string To { get; set; } = "";

    /// <summary>
    /// backend（默认）或 frontend。
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    /// <summary>
    /// areaPermissionFull / areaPermissionSnippet / frontendPermissionFull / frontendPermissionSnippet。
    /// </summary>
    [JsonPropertyName("when")]
    public string? When { get; set; }
}
