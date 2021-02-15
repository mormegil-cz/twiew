using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;
using TweetViewer.Models;

namespace TweetViewer.Controllers
{
    public class TweetController : Controller
    {
        private readonly ITwitterClient twitterClient;

        public TweetController(ITwitterClient twitterClient)
        {
            this.twitterClient = twitterClient;
        }

        public async Task<IActionResult> Detail(string id)
        {
            ITweet tweet;
            try
            {
                tweet = await twitterClient.Tweets.GetTweetAsync(long.Parse(id));
            }
            catch (TwitterException e)
            {
                return View("TweetError", e);
            }
            return View(new Tweet(tweet));
        }

        public async Task<IActionResult> Retweets(string id)
        {
            ITweet[] retweets;
            try
            {
                retweets = await twitterClient.Tweets.GetRetweetsAsync(long.Parse(id));
            }
            catch (TwitterException e)
            {
                return View("TweetError", e);
            }
            return View(retweets);
        }
    }
}