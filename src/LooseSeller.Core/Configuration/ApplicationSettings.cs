namespace LooseSeller.Core.Configuration;

public sealed class ApplicationSettings
{
    public ApplicationConfiguration Application { get; init; } = new();
}

public sealed class ApplicationConfiguration
{
    public string Version { get; init; } = "0.0.0";
}
