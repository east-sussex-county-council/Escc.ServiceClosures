namespace Escc.ServiceClosures
{
    /// <summary>
    /// Indications of whether a service is closed or not
    /// </summary>
    public enum ClosureStatus
    {
        /// <summary>
        /// No information available on whether the service is closed
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Confirmation received that the service is closed
        /// </summary>
        Closed = 1,

        /// <summary>
        /// Confirmation received that the service is partly closed
        /// </summary>
        PartlyClosed = 2
    }
}