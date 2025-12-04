namespace FirstWebApplication1.Models
{
    /// <summary>
    /// ViewModel for the shared error page. Provides a request identifier that helps correlate errors in logs
    /// with what the user saw (supports diagnostics without exposing sensitive details).
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Correlation identifier populated from Activity or HttpContext.TraceIdentifier.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indicates whether a request id is available for display.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
