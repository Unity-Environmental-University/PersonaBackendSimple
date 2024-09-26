using System.Text.Json;

public class ViteManifestEntry
{
    public string file { get; set; }
    public List<string> css { get; set; }
}

public static class ViteHelper
{
    public static ViteManifestEntry GetViteAssetPaths(string assetName)
    {
        var manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "dist", "manifest.json");
        if (!System.IO.File.Exists(manifestPath))
            throw new FileNotFoundException($"Vite manifest file not found at {manifestPath}");

        var json = System.IO.File.ReadAllText(manifestPath);
        var manifest = JsonSerializer.Deserialize<Dictionary<string, ViteManifestEntry>>(json);

        if (manifest == null || !manifest.ContainsKey(assetName))
            throw new KeyNotFoundException($"Asset '{assetName}' not found in Vite manifest");

        return manifest[assetName];
    }
}