using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tweetinvi;
using Tweetinvi.Parameters;
using TweetViewer.Models;

namespace TweetViewer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITwitterClient twitterClient;

        public HomeController(ITwitterClient twitterClient)
        {
            this.twitterClient = twitterClient;
        }

        public async Task<IActionResult> Index()
        {
            var tweets = await twitterClient.Search.SearchTweetsAsync(new SearchTweetsParameters("?") { PageSize = 10 });

            return View(tweets);
        }

        public async Task<IActionResult> Trending()
        {
            var trends = await twitterClient.Trends.GetPlaceTrendsAtAsync(1);
            return View(trends.Trends);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}