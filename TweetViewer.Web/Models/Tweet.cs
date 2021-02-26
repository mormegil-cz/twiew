using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tweetinvi.Core.Attributes;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace TweetViewer.Models
{
    public class Tweet
    {
        private static readonly Dictionary<Language, string> languageCodes = BuildLanguageCodes();

        public Tweet(ITweet data)
        {
            Data = data;
            TextToDisplay = ParseTextPieces(data);
        }

        public IList<TextPiece> TextToDisplay { get; }

        public ITweet Data { get; }

        public string? LanguageCode
        {
            get
            {
                var lang = Data.Language;
                return lang == null ? null : languageCodes[lang.GetValueOrDefault()];
            }
        }

        private static IList<TextPiece> ParseTextPieces(ITweet data)
        {
            var tweetText = data.FullText;
            var indexMapping = ComputeUtf16IndexMapping(tweetText);

            var fragments = new List<TextPiece>((data.UserMentions.Count + data.Hashtags.Count + data.Urls.Count) * 2 + 1)
            {
                new TextPiece<object> { Text = tweetText, Start = 0, End = tweetText.Length, Type = PieceType.Text }
            };

            foreach (var mention in data.UserMentions)
            {
                var start = indexMapping[mention.Indices[0]];
                var end = indexMapping[mention.Indices[1]];
                SplitFragment(fragments, start, end, new TextPiece<IUserMentionEntity> { Text = tweetText.Substring(start, end - start), Start = start, End = end, Type = PieceType.Mention, AdditionalData = mention });
            }
            foreach (var hashtag in data.Hashtags)
            {
                var start = indexMapping[hashtag.Indices[0]];
                var end = indexMapping[hashtag.Indices[1]];
                SplitFragment(fragments, start, end, new TextPiece<IHashtagEntity> { Text = tweetText.Substring(start, end - start), Start = start, End = end, Type = PieceType.Hashtag, AdditionalData = hashtag });
            }
            foreach (var url in data.Urls)
            {
                var start = indexMapping[url.Indices[0]];
                var end = indexMapping[url.Indices[1]];
                SplitFragment(fragments, start, end, new TextPiece<IUrlEntity> { Text = tweetText.Substring(start, end - start), Start = start, End = end, Type = PieceType.Url, AdditionalData = url });
            }
            foreach (var media in data.Media)
            {
                var start = indexMapping[media.Indices[0]];
                var end = indexMapping[media.Indices[1]];
                // var mediaPiece = new TextPiece<IMediaEntity> { Text = tweetText.Substring(start, end - start), Start = start, End = end, Type = PieceType.Media, AdditionalData = media };
                // media are not inserted into text, are shown separately below
                SplitFragment(fragments, start, end, null);
                if (fragments.Count == 0)
                {
                    // ugly hack for Tweets containing _only_ medias (with repeated (?) entries in the Media collection)
                    break;
                }
            }

            return fragments;
        }

        public static List<int> ComputeUtf16IndexMapping(string text)
        {
            var len = text.Length;
            var result = new List<int>(len * 2);
            for (int i = 0; i < len; ++i)
            {
                if (!Char.IsLowSurrogate(text[i]))
                {
                    result.Add(i);
                }
            }
            // +end index
            result.Add(len);
            return result;
        }

        private static void SplitFragment(List<TextPiece> fragments, int start, int end, TextPiece? piece)
        {
            var matchingFragments = fragments.Where(frag => frag.Start <= end && frag.End >= start).ToList();
            if (matchingFragments.Count == 0) throw new ArgumentException($"No fragment at {start}–{end}");
            if (matchingFragments.Count > 1) throw new ArgumentException($"{matchingFragments.Count} crossing fragments {matchingFragments[0].Type} {matchingFragments[0].Start}–{matchingFragments[0].End} / {matchingFragments[1].Type} {matchingFragments[1].Start}–{matchingFragments[1].End} when trying to insert {piece?.Type} {start}–{end}");
            var matchingFragment = matchingFragments[0];
            var index = fragments.IndexOf(matchingFragment);
            fragments.RemoveAt(index);
            if (matchingFragment.Start < start)
            {
                fragments.Insert(index, matchingFragment.Split(matchingFragment.Start, start));
                ++index;
            }
            if (piece != null) fragments.Insert(index, piece);
            ++index;
            if (matchingFragment.End > end)
            {
                fragments.Insert(index, matchingFragment.Split(end, matchingFragment.End));
            }
        }

        private static Dictionary<Language, string> BuildLanguageCodes()
        {
            var members = typeof(Language).GetMembers(BindingFlags.Public | BindingFlags.Static);
            var result = new Dictionary<Language, string>(members.Length);
            foreach (var member in members)
            {
                var attr = member.GetCustomAttribute<LanguageAttribute>(false);
                if (attr != null)
                {
                    result.Add(Enum.Parse<Language>(member.Name), attr.Code);
                }
            }
            return result;
        }
    }

    public abstract record TextPiece
    {
        public string Text { get; init; }
        public int Start { get; init; }
        public int End { get; init; }

        // TODO: Better HTML decoding
        public string DecodedText => Text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");

        public PieceType Type { get; init; }

        public abstract TextPiece Split(int from, int to);
    }

    public record TextPiece<TAdditionalData> : TextPiece
    {
        public TAdditionalData? AdditionalData { get; init; }

        public override TextPiece Split(int from, int to)
        {
            return new TextPiece<TAdditionalData> { Text = Text.Substring(from - Start, to - from), Start = from, End = to, Type = Type, AdditionalData = AdditionalData };
        }
    }

    public enum PieceType
    {
        None,
        Text,
        Hashtag,
        Url,
        Mention,
        Media,
    }
}