namespace Escc.ServiceClosures
{
    /// <summary>
    /// Indications of whether a service is closed or not
    /// </summary>
    public enum ClosureStatus
    {
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