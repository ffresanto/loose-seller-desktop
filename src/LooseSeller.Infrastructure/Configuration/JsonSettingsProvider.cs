using System.Text.Json;
using LooseSeller.Core.Configuration;

namespace LooseSeller.Infrastructure.Configuration;

public sealed class JsonSettingsProvider
{
    public ApplicationSettings Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("O arquivo de configurações não foi encontrado.", filePath);
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ApplicationSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ApplicationSettings();
    }
}
