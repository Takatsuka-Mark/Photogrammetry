using Microsoft.Extensions.Configuration;

namespace Framework;

public abstract class BaseOptions
{
    public string GetRequired(IConfiguration configuration, string section, string key)
    {
        var confSection = configuration.GetRequiredSection(section);
        return confSection.GetConnectionString(key) ?? throw new ArgumentException(key);
    }
}