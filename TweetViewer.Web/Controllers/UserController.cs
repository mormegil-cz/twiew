using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using TweetViewer.Models;

namespace TweetViewer.Controllers
{
    public class UserController : Controller
    {
        private readonly ITwitterClient twitterClient;

        public UserController(ITwitterClient twitterClient)
        {
            this.twitterClient = twitterClient;
        }

        public async Task<IActionResult> Detail(string id)
        {
            ITweet[] tweets;
            try
            {
                tweets = await twitterClient.Timelines.GetUserTimelineAsync(id);
            }
            catch (TwitterException e)
            {
                return View("UserError", e);
            }
            return View(tweets.Select(tweet => new Tweet(tweet)).ToList());
        }
    }
}