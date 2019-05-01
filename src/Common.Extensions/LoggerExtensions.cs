﻿namespace Naos.Core.Common
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
                type ??= LogEventPropertyKeys.TrackMisc;
                duration ??= TimeSpan.Zero;
                using(source.BeginScope(new Dictionary<string, object>(properties.Safe())
                {
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                    [LogEventPropertyKeys.TrackDuration] = duration.Value.Milliseconds,
                    [LogEventPropertyKeys.LogKey] = logKey,
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

        public static ILogger LogTraceEvent(
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
                    [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Trace,
                    [LogEventPropertyKeys.TrackDuration] = duration.Value.Milliseconds,
                    [LogEventPropertyKeys.TrackSpan] = span,
                    [LogEventPropertyKeys.TrackName] = name,
                    [LogEventPropertyKeys.LogKey] = logKey
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
