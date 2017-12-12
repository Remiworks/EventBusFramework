using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Remiworks.Core.Event.Matching
{
    public static class TopicMatcher
    {
        private const string RegexHashtag = @"\w+(\.\w+)*";
        private const string RegexStar = @"[\w]+";
        
        public static IEnumerable<string> Match(string topic, params string[] topicsToMatch)
        {
            if(topicsToMatch == null || !topicsToMatch.Any() || string.IsNullOrWhiteSpace(topic))
            {
                return new List<string>();
            }

            return topicsToMatch
                .Where(topicToMatch => IsMatch(topic, topicToMatch))
                .ToList();
        }

        private static bool IsMatch(string topic, string topicToMatch)
        {
            var parsedTopic = topicToMatch
                .Replace(".", "\\.")
                .Replace("*", RegexStar)
                .Replace("#", RegexHashtag);

            return Regex.IsMatch(topic, $"^{parsedTopic}$");
        }
    }
}