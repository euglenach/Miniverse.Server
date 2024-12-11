namespace Miniverse.ServiceDiscovery;

public interface IMagicOnionURLProvider
{
    string[] GetURLs();
}

public class MagicOnionURLProvider : IMagicOnionURLProvider
{
    // todo: 一旦適当にハードコード
    public string[] GetURLs()
    {
        return ["http://localhost:5209"];
    }
}

public class EnvironmentVariableURLProvider : IMagicOnionURLProvider
{
    public string[] GetURLs()
    {
        if(Environment.GetEnvironmentVariable("MAGICONION_ADDRESS") is not {} addresses) return [];

        return addresses.Split(",");
    }
}