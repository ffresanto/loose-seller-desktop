namespace LooseSeller.Core.Configuration;

public sealed class ApplicationSettings
{
    public ApplicationConfiguration Application { get; set; } = new();
}

public sealed class ApplicationConfiguration
{
    public string Version { get; init; } = "0.0.0";
    public string ProductsDirectory { get; set; } = "";
    public string DescriptionTemplateFile { get; set; } = "description_example.txt";
}
