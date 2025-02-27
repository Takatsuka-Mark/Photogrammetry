namespace Framework;

public abstract class BaseOptions
{
    public string GetRequired(string key) =>
        Environment.GetEnvironmentVariable(key) ?? throw new ArgumentNullException(key);
}