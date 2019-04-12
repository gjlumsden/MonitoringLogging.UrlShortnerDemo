namespace Training.UrlShortner.Functions.Configuration
{
    public interface IRuntimeConfiguration
    {
        string GetString(string settingName);
    }
}