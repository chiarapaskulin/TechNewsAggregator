using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechNewsAggregator.Models;
using TechNewsAggregator.Services;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly NewsScraperService _newsScraperService;

    public NewsController()
    {
        _newsScraperService = new NewsScraperService("FIRECRAWL_API_KEY", "ANTHROPIC_API_KEY");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsItem>>> Get()
    {
        var news = await _newsScraperService.ScrapeNewsSourcesAsync();
        return Ok(news);
    }
}
