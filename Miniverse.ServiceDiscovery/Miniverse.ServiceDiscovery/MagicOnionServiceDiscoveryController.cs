using MessagePack;
using Microsoft.AspNetCore.Mvc;
using MiniverseShared.WebAPI;
using System.Net;

namespace Miniverse.ServiceDiscovery;

[FormatFilter]
[Route("{controller}")]
// [ApiController]
public class MagicOnionServiceDiscoveryController(IMagicOnionURLResolver urlResolver) : Controller
{
    [HttpGet(nameof(GetUrl))]
    public ActionResult<MagicOnionURLResponse> GetUrl() => new MagicOnionURLResponse(urlResolver.Resolve());
}