using MessagePack;
using Microsoft.AspNetCore.Mvc;
using MiniverseShared.WebAPI;
using System.Net;

namespace Miniverse.ServiceDiscovery;

[FormatFilter]
[Route("api/")]
public class MagicOnionServiceDiscoveryController(IMagicOnionURLResolver urlResolver) : Controller
{
    [HttpGet(nameof(MagicOnionURLRequest))]
    public ActionResult<MagicOnionURLResponse> MagicOnionURL() => new MagicOnionURLResponse(urlResolver.Resolve());
}