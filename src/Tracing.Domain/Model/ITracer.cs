﻿namespace Naos.Tracing.Domain
{
    using System;

    public interface ITracer
    {
        ISpan CurrentSpan { get; }

        IScopeManager ScopeManager { get; }

        ISampler Sampler { get; }

        ISpanBuilder BuildSpan(string operationName, string logKey = null, SpanKind kind = SpanKind.Internal, ISpan parent = null, bool ignoreCurrentSpan = false);

        void End(IScope scope = null, SpanStatus status = SpanStatus.Succeeded, string statusDescription = null);

        void Fail(IScope scope = null, Exception exception = null);
    }
}