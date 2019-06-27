﻿namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public static class LoggerExtensions
    {
        public static ILogger LogJournal(
            this ILogger source,
            string logKey,
            string message,
            string type,
            TimeSpan? duration = null,
            IDictionary<string, object> properties = null,
            params object[] args)
        {
            if(!message.IsNullOrEmpty())
            {
                type ??= LogPropertyKeys.TrackMisc;
                duration ??= TimeSpan.Zero;
                using(source.BeginScope(new Dictionary<string, object>(properties.Safe())
                {
                    [LogPropertyKeys.TrackType] = LogTrackTypeValues.Journal,
                    [LogPropertyKeys.TrackDuration] = duration.Value.Milliseconds,
                    [LogPropertyKeys.LogKey] = logKey,
                    [type] = true
                }))
                {
                    try
                    {
                        source.Log(LogLevel.Information, $"{{LogKey:l}} {message:l}", args.Insert(logKey).ToArray());
                    }
                    catch(AggregateException ex) // IndexOutOfRangeException
                    {
                        if(ex.InnerException is IndexOutOfRangeException)
                        {
                            source.Log(LogLevel.Warning, $"{{LogKey:l}} {message:l}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            return source;
        }

        public static ILogger LogTrace(
            this ILogger source,
            string logKey,
            string span,
            string message, // span id
            string name = null, // LogTraceEventNames.Http
            TimeSpan? duration = null,
            IDictionary<string, object> properties = null,
            params object[] args)
        {
            if(!message.IsNullOrEmpty())
            {
                duration ??= TimeSpan.Zero;
                using(source.BeginScope(new Dictionary<string, object>(properties.Safe())
                {
                    [LogPropertyKeys.TrackType] = LogTrackTypeValues.Trace,
                    [LogPropertyKeys.TrackDuration] = duration.Value.Milliseconds,
                    [LogPropertyKeys.TrackSpan] = span,
                    [LogPropertyKeys.TrackName] = name,
                    [LogPropertyKeys.LogKey] = logKey
                }))
                {
                    try
                    {
                        source.Log(LogLevel.Information, $"{{LogKey:l}} {message:l}", args.Insert(logKey).ToArray());
                    }
                    catch(AggregateException ex) // IndexOutOfRangeException
                    {
                        if(ex.InnerException is IndexOutOfRangeException)
                        {
                            source.Log(LogLevel.Warning, $"{{LogKey:l}} {message:l}");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            // TODO: publish tracing notification (so other tracers can pick it up?), mediator publish

            return source;
        }
    }
}
