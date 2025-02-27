using Microsoft.Extensions.Configuration;

namespace Framework;

public abstract class BaseOptions
{
    private readonly IConfiguration _configuration;
    private readonly string _section;

    public BaseOptions(IConfiguration configuration, string section)
    {
        _configuration = configuration;
        _section = section;
    }

    public string GetRequired(string section, string key)
    {
        var confSection = _configuration.GetRequiredSection(section);
        return confSection.GetConnectionString(key) ?? throw new ArgumentException(key);
    }

    public string GetRequired(string key) => GetRequired(_section, key);
}