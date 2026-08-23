public readonly struct RenderResolutionPreset
{
    public RenderResolutionPreset(string aspectRatio, int width, int height)
    {
        Label = $"{aspectRatio}  |  {width} x {height}";
        Width = width;
        Height = height;
    }

    public string Label { get; }
    public int Width { get; }
    public int Height { get; }
}

public static class RenderResolutionPresets
{
    public static readonly RenderResolutionPreset[] All =
    {
        new("16:9", 1280, 720),
        new("16:9", 1920, 1080),
        new("16:9", 2560, 1440),
        new("16:9", 3840, 2160),
        new("4:3", 640, 480),
        new("4:3", 800, 600),
        new("4:3", 1024, 768),
        new("4:3", 1600, 1200),
        new("16:10", 1280, 800),
        new("16:10", 1920, 1200),
        new("16:10", 2560, 1600),
        new("21:9", 2560, 1080),
        new("21:9", 3440, 1440),
    };

    public static int FindIndex(int width, int height)
    {
        for (int index = 0; index < All.Length; index++)
        {
            if (All[index].Width == width && All[index].Height == height)
            {
                return index;
            }
        }

        return -1;
    }
}
