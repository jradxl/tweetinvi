﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi.Core.Controllers;
using Tweetinvi.Core.Factories;
using Tweetinvi.Core.Parameters;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Tweetinvi.Controllers.Timeline
{
    public class TimelineController : ITimelineController
    {
        private readonly ITweetFactory _tweetFactory;
        private readonly ITimelineQueryExecutor _timelineQueryExecutor;
        private readonly IUserFactory _userFactory;
        private readonly ITimelineQueryParameterGenerator _timelineQueryParameterGenerator;

        public TimelineController(
            ITweetFactory tweetFactory,
            ITimelineQueryExecutor timelineQueryExecutor,
            IUserFactory userFactory,
            ITimelineQueryParameterGenerator timelineQueryParameterGenerator)
        {
            _tweetFactory = tweetFactory;
            _timelineQueryExecutor = timelineQueryExecutor;
            _userFactory = userFactory;
            _timelineQueryParameterGenerator = timelineQueryParameterGenerator;
        }

        // Home Timeline
        public Task<IEnumerable<ITweet>> GetHomeTimeline(int maximumNumberOfTweetsToRetrieve)
        {
            var timelineRequestParameter = _timelineQueryParameterGenerator.CreateHomeTimelineParameters();
            timelineRequestParameter.MaximumNumberOfTweetsToRetrieve = maximumNumberOfTweetsToRetrieve;

            return GetHomeTimeline(timelineRequestParameter);
        }

        public async Task<IEnumerable<ITweet>> GetHomeTimeline(IHomeTimelineParameters parameters)
        {
            if (parameters == null)
            {
                parameters = _timelineQueryParameterGenerator.CreateHomeTimelineParameters();
            }

            var timelineDTO = await _timelineQueryExecutor.GetHomeTimeline(parameters);
            return _tweetFactory.GenerateTweetsFromDTO(timelineDTO);
        }

        // User Timeline
        public Task<IEnumerable<ITweet>> GetUserTimeline(long userId, int maximumNumberOfTweets = 40)
        {
            var user = _userFactory.GenerateUserIdentifierFromId(userId);
            return GetUserTimeline(user, maximumNumberOfTweets);
        }

        public Task<IEnumerable<ITweet>> GetUserTimeline(string userScreenName, int maximumNumberOfTweets = 40)
        {
            var user = _userFactory.GenerateUserIdentifierFromScreenName(userScreenName);
            return GetUserTimeline(user, maximumNumberOfTweets);
        }

        public Task<IEnumerable<ITweet>> GetUserTimeline(IUserIdentifier user, int maximumNumberOfTweets = 40)
        {
            var requestParameters = _timelineQueryParameterGenerator.CreateUserTimelineParameters();
            requestParameters.MaximumNumberOfTweetsToRetrieve = maximumNumberOfTweets;

            return GetUserTimeline(user, requestParameters);
        }

        public Task<IEnumerable<ITweet>> GetUserTimeline(long userId, IUserTimelineParameters parameters)
        {
            return GetUserTimeline(new UserIdentifier(userId), parameters);
        }

        public Task<IEnumerable<ITweet>> GetUserTimeline(string userScreenName, IUserTimelineParameters parameters)
        {
            return GetUserTimeline(new UserIdentifier(userScreenName), parameters);
        }

        public Task<IEnumerable<ITweet>> GetUserTimeline(IUserIdentifier user, IUserTimelineParameters parameters)
        {
            if (parameters == null)
            {
                parameters = _timelineQueryParameterGenerator.CreateUserTimelineParameters();
            }

            var queryParameters = _timelineQueryParameterGenerator.CreateUserTimelineQueryParameters(user, parameters);
            return GetUserTimeline(queryParameters);
        }

        private async Task<IEnumerable<ITweet>> GetUserTimeline(IUserTimelineQueryParameters queryParameters)
        {
            var tweetsDTO = await _timelineQueryExecutor.GetUserTimeline(queryParameters);

            return _tweetFactory.GenerateTweetsFromDTO(tweetsDTO);
        }

        // Mention Timeline
        public Task<IEnumerable<IMention>> GetMentionsTimeline(int maximumNumberOfTweets = 40)
        {
            var timelineRequestParameter = _timelineQueryParameterGenerator.CreateMentionsTimelineParameters();
            timelineRequestParameter.MaximumNumberOfTweetsToRetrieve = maximumNumberOfTweets;

            return GetMentionsTimeline(timelineRequestParameter);
        }

        public async Task<IEnumerable<IMention>> GetMentionsTimeline(IMentionsTimelineParameters parameters)
        {
            if (parameters == null)
            {
                parameters = _timelineQueryParameterGenerator.CreateMentionsTimelineParameters();
            }

            var timelineDTO = await _timelineQueryExecutor.GetMentionsTimeline(parameters);
            return _tweetFactory.GenerateMentionsFromDTO(timelineDTO);
        }

        // Retweets Of Me Timeline
        public async Task<IEnumerable<ITweet>> GetRetweetsOfMeTimeline(IRetweetsOfMeTimelineParameters parameters)
        {
            if (parameters == null)
            {
                parameters = _timelineQueryParameterGenerator.CreateRetweetsOfMeTimelineParameters();
            }

            var timelineDTO = await _timelineQueryExecutor.GetRetweetsOfMeTimeline(parameters);
            return _tweetFactory.GenerateTweetsFromDTO(timelineDTO);
        }
    }
}