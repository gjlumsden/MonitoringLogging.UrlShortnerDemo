namespace Training.UrlShortner.Functions.Configuration
{
    internal interface IRuntimeConfiguration
    {
        string GetString(string settingName);
    }
}