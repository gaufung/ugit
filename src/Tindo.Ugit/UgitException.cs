namespace Tindo.Ugit
{
    using System;

    /// <summary>
    /// Ugit exception.
    /// </summary>
    internal class UgitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UgitException"/> class.
        /// </summary>
        /// <param name="message">exception message.</param>
        public UgitException(string message)
            : base(message)
        {
        }
    }
}
