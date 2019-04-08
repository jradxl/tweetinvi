﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Tweetinvi.Core.Controllers
{
    public interface IAccountController
    {
        Task<IAccountSettings> GetAuthenticatedUserSettings();

        Task<IAccountSettings> UpdateAuthenticatedUserSettings(
            IEnumerable<Language> languages = null,
            string timeZone = null,
            long? trendLocationWoeid = null,
            bool? sleepTimeEnabled = null,
            int? startSleepTime = null,
            int? endSleepTime = null);

        Task<IAccountSettings> UpdateAuthenticatedUserSettings(IAccountSettingsRequestParameters accountSettingsRequestParameters);

        // Profile
        Task<IAuthenticatedUser> UpdateAccountProfile(IAccountUpdateProfileParameters parameters);

        Task<bool> UpdateProfileImage(byte[] imageBinary);
        Task<bool> UpdateProfileImage(IAccountUpdateProfileImageParameters parameters);


        Task<bool> UpdateProfileBanner(byte[] imageBinary);
        Task<bool> UpdateProfileBanner(IAccountUpdateProfileBannerParameters parameters);
        Task<bool> RemoveUserProfileBanner();

        bool UpdateProfileBackgroundImage(byte[] imageBinary);
        bool UpdateProfileBackgroundImage(long mediaId);
        bool UpdateProfileBackgroundImage(IAccountUpdateProfileBackgroundImageParameters parameters);

        // Mute
        Task<IEnumerable<long>> GetMutedUserIds(int maxUserIds = Int32.MaxValue);
        Task<IEnumerable<IUser>> GetMutedUsers(int maxUsersToRetrieve = 250);

        Task<bool> MuteUser(IUserIdentifier user);
        Task<bool> MuteUser(long userId);
        Task<bool> MuteUser(string screenName);

        bool UnMuteUser(IUserIdentifier user);
        bool UnMuteUser(long userId);
        bool UnMuteUser(string screenName);

        // Suggestions
        Task<IEnumerable<ICategorySuggestion>> GetSuggestedCategories(Language? language);
        Task<IEnumerable<IUser>> GetSuggestedUsers(string slug, Language? language);
        Task<IEnumerable<IUser>> GetSuggestedUsersWithTheirLatestTweet(string slug);
        IAccountSettings GenerateAccountSettingsFromJson(string json);
    }
}