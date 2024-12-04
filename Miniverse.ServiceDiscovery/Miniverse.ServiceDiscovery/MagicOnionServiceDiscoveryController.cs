using MessagePack;
using Microsoft.AspNetCore.Mvc;
using MiniverseShared.WebAPI;
using System.Net;

namespace Miniverse.ServiceDiscovery;

[FormatFilter]
[Route("{controller}/")]
// [ApiController]
public class MagicOnionServiceDiscoveryController(IMagicOnionURLResolver urlResolver) : Controller
{
    [HttpGet]
    public ActionResult<MagicOnionURLResponse> GetUrl() => new MagicOnionURLResponse(urlResolver.Resolve());
}


[FormatFilter]
[Route("[controller]/{id}.{format?}")]
public class PeopleController : Controller
{
    [HttpGet]
    public Person Get(string id)
    {
        switch (id)
        {
            case "Alice": return new Person("Alice", 17);
            case "Karen": return new Person("Karen", 17);
            default: return null;
        }
    }
}

[MessagePackObject]
public class Person
{
    [Key(0)]
    public string Name { get; }
    [Key(1)]
    public int Age { get; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}