﻿namespace Naos.Core.Tracing.Domain
{
    using System;
    using EnsureThat;
    using Naos.Foundation;

    /// <summary>
    /// <para>
    /// A tracer is responsible for managing the current scope and the creation of it's spans.
    /// When current scope gets deactivated (dispose) a specific domain event is published <see cref="SpanEndedDomainEvent"/>
    /// </para>
    /// <para>
    ///            (scoped)                         (scoped)                  (scoped)
    ///        ┌─────────┐                  ┌─────────────┐             ┌──────────┐
    ///        │ Tracer  │                  │ ScopeManager│             │ Mediator │
    ///        └─────────┘  ┌──────────── * └─────────────┘             └──────────┘
    ///             │       │ SpanBuilder │        │                          │
    ///             │       └─────────────┘        │                          │
    ///             │       create │               │                          │
    ///             x------------->│               │                          │
    ///             │      withtag │               │                          │
    ///             x------------->│               │                          │
    ///             │              x─┐             │                          │
    ///             │              │ │build()      │                          │
    ///             │              │<┘             │        ┌────── *         │
    ///             │              | activate(span)│        │ Scope │         │
    ///             |              x-------------->│        └───────┘         │
    ///             │              │               x----------->│             │
    ///             │              │               │     create │             │
    ///             │        scope │               │<-----------x             │
    ///             │<-----------------------------x            │             │
    ///             │              │               │            │             │
    ///  DISPOSE   ...            ...             ...          ...            │
    ///    scope    │              │               │            │             │
    ///             │              │               │<-----------x             │
    ///             │              │               │  deactivate              │
    ///             │              │               │                          │
    ///             │              │               x------------------------->│
    ///             │              │               │      publish             │─┐----> handler
    ///             │              │               │      SpanEndedDomainEvent│ │----> handler
    ///                                                                       │<┘
    /// * = newly created, no shared state
    /// </para>
    /// <para>
    ///    -----|-----|-----|-----|-----|-----|-----|-----|-----|-----|-----|
    ///
    ///       [ span A -------------------------------]
    ///            [ span B ------------------------]
    ///                       [ span C (http) ----]
    /// </para>
    /// </summary>
    /// <seealso cref="ITracer" />
    public class Tracer : ITracer
    {
        public Tracer(IScopeManager scopeManager) // needs correlationid (=traceid) get from ICorrelationContextAccessor
        {
            EnsureArg.IsNotNull(scopeManager, nameof(scopeManager));

            this.ScopeManager = scopeManager;
        }

        public ISpan CurrentSpan => this.ScopeManager.Current?.Span; // use in outbound httpclient

        public IScopeManager ScopeManager { get; }

        public ISpanBuilder BuildSpan(string operationName, SpanKind kind = SpanKind.Internal, ISpan parent = null)
        {
            return new SpanBuilder(this, operationName, kind, parent ?? this.CurrentSpan)
                .WithTag(SpanTagKey.SpanKind, kind.ToString()); // pass correlationid as traceid
        }

        public void End(IScope scope = null, SpanStatus status = SpanStatus.Succeeded, string statusDescription = null)
        {
            scope ??= this.ScopeManager.Current;
            scope?.Span?.End(status, statusDescription);
            this.ScopeManager.Deactivate(scope);
        }

        public void Fail(IScope scope = null, Exception exception = null)
        {
            scope ??= this.ScopeManager.Current;
            scope?.Span?
                .WithTag(SpanTagKey.Error, true)
                .AddLog(SpanLogKey.ErrorKind, "exception")
                .AddLog(SpanLogKey.Message, $"[{exception.GetType().Name}] {exception.GetFullMessage()}")
                .AddLog(SpanLogKey.StackTrace, exception.StackTrace);

            scope?.Span?.End(SpanStatus.Failed, exception?.GetFullMessage());
            this.ScopeManager.Deactivate(scope);
        }
    }
}