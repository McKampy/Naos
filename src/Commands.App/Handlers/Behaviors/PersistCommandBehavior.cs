﻿namespace Naos.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Foundation;

    public class PersistCommandBehavior : ICommandBehavior
    {
        private ICommandBehavior next;

        public ICommandBehavior SetNext(ICommandBehavior next)
        {
            this.next = next;
            return next;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task ExecutePreHandleAsync<TResponse>(Command<TResponse> request, CommandBehaviorResult result)
        {
            EnsureArg.IsNotNull(request);
            // TODO: implement
            // - check if command exists in repo/filestorage
            // - if not add to repo, return CommandBehaviorResult

            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePreHandleAsync(request, result).AnyContext();
            }

            // terminate here
        }

        public async Task ExecutePostHandleAsync<TResponse>(CommandResponse<TResponse> response, CommandBehaviorResult result)
        {
            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePostHandleAsync(response, result).AnyContext();
            }
        }
    }
}
