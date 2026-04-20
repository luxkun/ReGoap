using System;

namespace ReGoap.Core
{
    /// <summary>
    /// Optional helper interface used by framework adapters to inspect generic type arguments.
    /// </summary>
    public interface IReGoapAgentHelper
    {
        /// <summary>
        /// Returns concrete generic type arguments used by the agent implementation.
        /// </summary>
        Type[] GetGenericArguments();
    }
}
