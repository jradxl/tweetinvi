﻿using System;
using Tweetinvi.Models;
using Tweetinvi.Streaming.Events;

namespace Tweetinvi.Events
{
    public class UserEventArgs : EventArgs
    {
        public UserEventArgs(IUser target, long sourceId)
        {
            SourceId = sourceId;
            Target = target;
        }

        public long SourceId { get; }
        public IUser Target { get; }
    }

    public class UserFollowedEventArgs : UserEventArgs
    {
        public UserFollowedEventArgs(IUser source, IUser target, long sourceId) : base(target, sourceId)
        {
            Source = source;
        }

        public IUser Source { get; }
    }

    public class UserUnFollowedEventArgs : UserEventArgs
    {
        public UserUnFollowedEventArgs(IUser source, IUser target, long sourceId) : base(target, sourceId)
        {
            Source = source;
        }

        public IUser Source { get; }
    }

    public class UserBlockedEventArgs : UserEventArgs
    {
        public UserBlockedEventArgs(IUser target, long sourceId) : base(target, sourceId)
        {
        }
    }

    public class UserMutedEventArgs : UserEventArgs
    {
        public UserMutedEventArgs(IUser target, long sourceId) : base(target, sourceId)
        {

        }
    }

    public class UserWitheldEventArgs : EventArgs
    {
        public UserWitheldEventArgs(IUserWitheldInfo userWitheldInfo)
        {
            UserWitheldInfo = userWitheldInfo;
        }

        public IUserWitheldInfo UserWitheldInfo { get; private set; }
    }
}