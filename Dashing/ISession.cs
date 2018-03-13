namespace Dashing {
    using System;

    using Dashing.Configuration;

    public partial interface ISession : IDisposable {
        /// <summary>
        ///     Gets the Configuration object for this Session
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        ///     Use Dapper functions in the context of this Session
        /// </summary>
        IDapper Dapper { get; }

        /// <summary>
        ///     Completes the session, effectively calling commit on the underlying transaction
        /// </summary>
        void Complete();

        /// <summary>
        ///     Rejects the session, effectively rolling back the underlying transaction
        /// </summary>
        /// <remarks>This will rollback the transaction at the point the Session is disposed</remarks>
        void Reject();
    }
}