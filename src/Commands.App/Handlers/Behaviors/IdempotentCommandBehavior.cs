﻿namespace Naos.Core.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;

    public class IdempotentCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> command)
        {
            EnsureArg.IsNotNull(command);

            // TODO: implement
            // - check if command exists in repo
            // - if so return CommandBehaviorResult cancelled = true + reason

            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
            //return await Task.FromResult(new BehaviorResult("command already handled")).ConfigureAwait(false);
        }
    }
}
