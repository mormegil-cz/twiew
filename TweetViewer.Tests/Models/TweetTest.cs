using System.Collections.Generic;
using Tweetinvi;
using Tweetinvi.Core.DTO;
using Tweetinvi.Core.Models;
using Tweetinvi.Core.Models.TwitterEntities;
using Tweetinvi.Models.Entities;
using Xunit;

namespace TweetViewer.Tests.Models
{
    public class TweetTest
    {
        [Fact]
        public void Test1()
        {
            var entities = new TweetEntitiesDTO
            {
                Hashtags = new List<IHashtagEntity>
                {
                    new HashtagEntity { Indices = new int[] { 9, 13 }, Text = "HASH" },
                    new HashtagEntity { Indices = new int[] { 32, 36 }, Text = "HASH" }
                },
                Urls = new List<IUrlEntity>
                {
                    new UrlEntity { Indices = new int[] { 0, 3 }, URL = "URL" },
                    new UrlEntity { Indices = new int[] { 45, 48 }, URL = "URL" },
                },
                UserMentions = new List<IUserMentionEntity>
                {
                    new UserMentionEntity { Indices = new int[] { 19, 26 }, Name = "MENTION" },
                    new UserMentionEntity { Indices = new int[] { 37, 44 }, Name = "MENTION" },
                },
                Medias = new List<IMediaEntity>
                {
                    new MediaEntity { Indices = new int[] { 49, 54 }, URL = "MEDIA" },
                    new MediaEntity { Indices = new int[] { 49, 54 }, URL = "MEDIA" },
                },
            };

            var data = new Tweet(new TweetDTO
            {
                //          0         1         2          3         4         5
                //          012345678901234567890123456789 012345678901234567890123
                FullText = "URL TEXT HASH TEXT MENTION TE🎮T HASH MENTION URL MEDIA",
                ExtendedTweet = new ExtendedTweet
                {
                    ExtendedEntities = entities,
                    LegacyEntities = entities,
                },
                Entities = entities,
                LegacyEntities = entities,
                CreatedBy = new UserDTO()
            }, TweetMode.None, new TwitterClient("X", "X"));
            data.TweetDTO = data.TweetDTO;

            var tweet = new TweetViewer.Models.Tweet(data);

            Assert.Equal(12, tweet.TextToDisplay.Count);
        }

        [Fact]
        public void TestUtf16IndexMapping()
        {
            var mapping = TweetViewer.Models.Tweet.ComputeUtf16IndexMapping("TE🎮T");
            Assert.Equal(4, mapping.Count);
            Assert.Equal(new[] { 0, 1, 2, 4 }, mapping.ToArray());
        }
    }
}