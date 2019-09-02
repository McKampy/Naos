﻿namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    /// <summary>
    /// A base implementation for handling application commands and ensuring all behaviors are executed with proper responses (not cancelled).
    /// </summary>
    /// <typeparam name="TCommand">The type of the request.</typeparam>
    /// <typeparam name="TResponse">Return value of the wrapped command handler.</typeparam>
    /// <seealso cref="App.CommandHandler{TRequest, TResponse}" />
    /// <seealso cref="MediatR.IRequestHandler{Command{TResponse}, CommandResponse{TResponse}}" />
    public abstract class BehaviorCommandHandler<TCommand, TResponse>
        : CommandHandler<TCommand, TResponse>
        where TCommand : Command<TResponse>
    {
        private readonly IEnumerable<ICommandBehavior> behaviors;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorCommandHandler{TRequest, TResponse}" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="behaviors">The behaviors.</param>
        protected BehaviorCommandHandler(
            ILogger logger,
            IEnumerable<ICommandBehavior> behaviors = null)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.Logger = logger;
            this.behaviors = behaviors;
        }

        public ILogger Logger { get; }

        /// <summary>
        /// Handles the specified request. All behaviors will be called first.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public override async Task<CommandResponse<TResponse>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            foreach (var behavior in this.behaviors.Safe())
            {
                var behaviorResult = await behavior.ExecuteAsync(request).AnyContext();
                if (behaviorResult.Cancelled) // abort if this behavior did not succeed
                {
                    // TODO: log reason
                    return new CommandResponse<TResponse>(behaviorResult.CancelledReason);
                }
            }

            var commandName = typeof(TCommand).PrettyName();
            this.Logger.LogJournal(LogKeys.AppCommand, $"command handle (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id})", LogPropertyKeys.TrackHandleCommand);
            //this.Logger.LogTrace(LogKeys.AppCommand, request.Id, commandName, LogTraceNames.Command);

            using (var timer = new Foundation.Timer())
            {
                var result = await this.HandleRequest(request, cancellationToken).AnyContext();

                timer.Stop();
                //this.Logger.LogTrace(LogKeys.AppCommand, request.Id, commandName, LogTraceNames.Command, timer.Elapsed);
                this.Logger.LogInformation($"{{LogKey:l}} command handled (name={commandName}, handler={this.GetType().PrettyName()}, id={request.Id}) -> took {timer.Elapsed.Humanize()}", LogKeys.AppCommand);

                return result;
            }
        }
    }
}
