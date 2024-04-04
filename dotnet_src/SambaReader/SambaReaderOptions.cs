namespace SambaReader;

public class SambaReaderOptions
{
    
    public string Username { get; init; }
    public string Password { get; init; }
    public string RootDirectory { get; init; }

    private const string UsernameKey = "samba_reader:username";
    private const string PasswordKey = "samba_reader:password";
    private const string RootDirectoryKey = "samba_reader:root_directory";
    
    public SambaReaderOptions()
    {
        // TODO set from appsettings?
        Username = Environment.GetEnvironmentVariable(UsernameKey) ?? throw new ArgumentNullException($"{UsernameKey} must be set in environment variables");
        Password = Environment.GetEnvironmentVariable(PasswordKey) ?? throw new ArgumentNullException($"{PasswordKey} must be set in environment variables");
        RootDirectory = Environment.GetEnvironmentVariable(RootDirectoryKey) ?? throw new ArgumentNullException($"{RootDirectoryKey} must be set in environment variables");
    }
}