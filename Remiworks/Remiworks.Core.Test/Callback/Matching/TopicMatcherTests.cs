using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Remiworks.Core.Callbacks.Matching;
using Shouldly;

namespace Remiworks.Core.Test.Callback.Matching
{
    [TestClass]
    public class TopicMatcherTests
    {
        private const string ShortAddedTopic = "user.added";
        private const string UserAddedTopic = "user.event.added";
        private const string LongAddedTopic = "user.something.event.added";
        private const string VeryLongAddedTopic = "user.foo.event.bar.added";
        private const string UserDeletedTopic = "user.event.deleted";

        #region EmptyInput
        
        [TestMethod]
        public void MatchReturnsEmptyListIfInputListIsNull()
        {
            TopicMatcher.Match(UserAddedTopic, null).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchReturnsEmptyListIfInputIsEmpty()
        {
            TopicMatcher.Match(UserAddedTopic, new string[0]).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchReturnsEmptyListIfTopicIsNull()
        {
            TopicMatcher.Match(null, new []{"foo.bar"}).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchReturnsEmptyListIfTopicIsEmpty()
        {
            TopicMatcher.Match("", new []{"foo.bar"}).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchReturnsEmptyListIfTopicIsWhiteSpace()
        {
            TopicMatcher.Match(" ", new []{"foo.bar"}).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchReturnsEmptyListIfNoTopicsMatch()
        {
            TopicMatcher.Match(UserAddedTopic, new []{"something.else"}).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchMatches_ExactlySameTopic()
        {
            var topicList = new []{UserAddedTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList).ToList()
                .ShouldHaveSingleItem()
                .ShouldBe(UserAddedTopic);
        }

        [TestMethod]
        public void MatchMatches_StarInMiddle()
        {
            const string starTopic = "user.*.added";
            var topicList = new [] {starTopic, UserDeletedTopic};
            
            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(starTopic);
        }
        
        #endregion

        #region HashtagInBeginning
        
        [TestMethod]
        public void MatchMatches_HashtagInBeginning_When1Segment()
        {
            const string hashtagTopic = "#.event.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);   
        }

        [TestMethod]
        public void MatchMatches_HashTagInBeginning_When2Segments()
        {
            const string hashtagTopic = "#.event.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);   
        }
        
        [TestMethod]
        public void MatchDoesntMatch_HashTagOnBeginning_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "..user";
            var topicList = new[] {"#.user", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_HashTagOnBeginning_WhenNoSegments()
        {
            var topicList = new[] {"#.user.added", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }

        #endregion
        
        #region HashtagInMiddle
             
        [TestMethod]
        public void MatchMatches_HashtagInMiddle_When1Segment()
        {
            const string hashtagTopic = "user.#.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }

        [TestMethod]
        public void MatchMatches_HashtagInMiddle_When2Segments()
        {
            const string hashtagTopic = "user.#.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }

        [TestMethod]
        public void MatchDoesntMatch_HashTagInMiddle_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "user..added";
            var topicList = new[] {"user.#.added", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_HashtagInMiddle_WhenNoSegments()
        {
            var topicList = new[] {"user.#.added", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }
        
        #endregion

        #region HashtagOnEnd
        
        [TestMethod]
        public void MatchMatches_HashtagOnEnd_When1Segment()
        {   
            const string hashtagTopic = "user.event.#";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }
        
        [TestMethod]
        public void MatchMatches_HashtagOnEnd_When2Segment()
        {
            const string hashtagTopic = "user.something.#";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }

        [TestMethod]
        public void MatchDoesntMatch_HashtagOnEnd_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "user..";
            var topicList = new[] {"user.#", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_HashtagOnEnd_WhenNoSegments()
        {
            var topicList = new[] {"user.added.#", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }

        #endregion
        
        #region StarInBeginning
        
        [TestMethod]
        public void MatchMatches_StarInBeginning_When1Segment()
        {
            const string hashtagTopic = "*.event.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);   
        }

        [TestMethod]
        public void MatchDoesntMatch_StarInBeginning_When2Segments()
        {
            const string hashtagTopic = "*.event.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList).ShouldBeEmpty();   
        }
        
        [TestMethod]
        public void MatchDoesntMatch_StarOnBeginning_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "..user";
            var topicList = new[] {"*.user", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_StarOnBeginning_WhenNoSegments()
        {
            var topicList = new[] {"*.user.added", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }
        
        #endregion
        
        #region StarInMiddle
                
        [TestMethod]
        public void MatchMatches_StarInMiddle_When1Segment()
        {
            const string hashtagTopic = "user.*.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }

        [TestMethod]
        public void MatchDoesntMatch_StarInMiddle_When2Segments()
        {
            const string hashtagTopic = "user.*.added";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_StarInMiddle_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "user..added";
            var topicList = new[] {"user.*.added", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_StarInMiddle_WhenNoSegments()
        {
            var topicList = new[] {"user.*.added", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }
     
        #endregion
        
        #region StarOnEnd
        
        [TestMethod]
        public void MatchMatches_StarOnEnd_When1Segment()
        {   
            const string hashtagTopic = "user.event.*";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(UserAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }
        
        [TestMethod]
        public void MatchMatches_StarOnEnd_When2Segment()
        {
            const string hashtagTopic = "user.something.*";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(LongAddedTopic, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_StarOnEnd_WhenEmptySegments()
        {
            const string topicWithEmptySegments = "user..";
            var topicList = new[] {"user.*", UserDeletedTopic};
            
            TopicMatcher.Match(topicWithEmptySegments, topicList).ShouldBeEmpty();
        }

        [TestMethod]
        public void MatchDoesntMatch_StarOnEnd_WhenNoSegments()
        {
            var topicList = new[] {"user.added.*", UserDeletedTopic};
            
            TopicMatcher.Match(ShortAddedTopic, topicList).ShouldBeEmpty();
        }

        #endregion
        
        #region Combinations

        [TestMethod]
        public void MatchMatches_MultipleHashtags()
        {
            const string hashtagTopic = "user.#.bar.#";
            var topicList = new[] {hashtagTopic, UserDeletedTopic};

            TopicMatcher.Match(VeryLongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagTopic);
        }

        [TestMethod]
        public void MatchMatches_MultipleStarts()
        {
            const string starTopic = "user.*.event.*.added";
            var topicList = new[] {starTopic, UserDeletedTopic};

            TopicMatcher.Match(VeryLongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(starTopic);
        }

        [TestMethod]
        public void MatchMatches_HashtagStartCombination()
        {
            const string hashtagStarTopic = "user.#.bar.*";
            var topicList = new[] {hashtagStarTopic, UserDeletedTopic};

            TopicMatcher.Match(VeryLongAddedTopic, topicList)
                .ShouldHaveSingleItem()
                .ShouldBe(hashtagStarTopic);
        }
        
        #endregion
    }
}