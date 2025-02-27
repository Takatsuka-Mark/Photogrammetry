namespace Framework

public abstract class BaseOptions
{
    public string GetRequired(string key) => Environment.GetEnvironmentVariable(RootDirectoryKey) ?? throw new ArgumentNullException(RootDirectoryKey);
}