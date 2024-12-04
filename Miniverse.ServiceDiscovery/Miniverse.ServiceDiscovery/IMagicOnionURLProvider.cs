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