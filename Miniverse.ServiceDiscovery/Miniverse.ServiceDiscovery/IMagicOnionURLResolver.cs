namespace Miniverse.ServiceDiscovery;

public interface IMagicOnionURLResolver
{
    string Resolve();
}

public class RandomURLResolver(IMagicOnionURLProvider urlProvider) : IMagicOnionURLResolver
{
    public string Resolve()
    {
        var urls = urlProvider.GetURLs();
        if(urls.Length == 0) return "";
        return urls[Random.Shared.Next(0, urls.Length)];
    }
}