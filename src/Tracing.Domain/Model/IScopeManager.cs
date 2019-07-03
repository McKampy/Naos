﻿namespace Naos.Core.Tracing.Domain
{
    using System.Threading.Tasks;

    public interface IScopeManager
    {
        IScope Current { get; }

        IScope Activate(ISpan span, bool finishOnDispose = true);

        Task Deactivate(IScope scop);
    }
}