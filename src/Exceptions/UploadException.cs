namespace YouTubeShortAutomator.Exceptions;

/// <summary>
/// Represents an exception that occurs during upload operations.
/// </summary>
public class UploadException : Exception
{
    /// <summary>
    /// Gets or sets the identifier of the upload job.
    /// </summary>
    public int? UploadJobId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the video short.
    /// </summary>
    public int? VideoShortId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation can be retried.
    /// </summary>
    public bool IsRetryable { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UploadException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public UploadException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadException"/> class with a specified error message and additional context information.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="uploadJobId">The identifier of the upload job.</param>
    /// <param name="videoShortId">The identifier of the video short.</param>
    /// <param name="isRetryable">Indicates whether the operation can be retried.</param>
    public UploadException(string message, int uploadJobId, int videoShortId, bool isRetryable = true)
        : base(message)
    {
        UploadJobId = uploadJobId;
        VideoShortId = videoShortId;
        IsRetryable = isRetryable;
    }
}
