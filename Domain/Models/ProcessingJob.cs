// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace YouTubeShortsAutomator.Domain.Models;

public class ProcessingJob
{
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Video? Video { get; set; }
    public ProcessingJobType JobType { get; set; }
    public ProcessingJobStatus Status { get; set; } = ProcessingJobStatus.Queued;
    public ProcessingStepType CurrentStep { get; set; } = ProcessingStepType.Validation;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public List<ProcessingStep> Steps { get; set; } = new();
    public List<ProcessingError> Errors { get; set; } = new();
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;
    public TimeSpan EstimatedDuration { get; set; }
    public float ProgressPercentage { get; set; }
    public string? WorkerId { get; set; }

    /// <summary>
    /// Starts the processing job
    /// </summary>
    public void Start()
    {
        Status = ProcessingJobStatus.Running;
        StartedAt = DateTime.UtcNow;
        CurrentStep = ProcessingStepType.Validation;
    }

    /// <summary>
    /// Completes the processing job
    /// </summary>
    public void Complete()
    {
        Status = ProcessingJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ProgressPercentage = 100f;
    }

    /// <summary>
    /// Fails the processing job
    /// </summary>
    public void Fail(string errorMessage)
    {
        Status = ProcessingJobStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        var error = new ProcessingError
        {
            Id = Guid.NewGuid(),
            JobId = Id,
            ErrorMessage = errorMessage,
            ErrorType = ProcessingErrorType.ProcessingFailure,
            OccurredAt = DateTime.UtcNow
        };
        Errors.Add(error);
    }

    /// <summary>
    /// Advances to the next processing step
    /// </summary>
    public void AdvanceStep(ProcessingStepType nextStep)
    {
        CurrentStep = nextStep;
        var step = new ProcessingStep
        {
            Id = Guid.NewGuid(),
            JobId = Id,
            StepType = nextStep,
            StartedAt = DateTime.UtcNow,
            Status = StepStatus.InProgress
        };
        Steps.Add(step);
    }

    /// <summary>
    /// Marks current step as completed
    /// </summary>
    public void CompleteCurrentStep(string? output = null)
    {
        var currentStep = Steps.FirstOrDefault(s => s.Status == StepStatus.InProgress);
        if (currentStep != null)
        {
            currentStep.CompletedAt = DateTime.UtcNow;
            currentStep.Status = StepStatus.Completed;
            currentStep.StepOutput = output;
        }
    }

    /// <summary>
    /// Determines if the job can be retried
    /// </summary>
    public bool CanRetry()
    {
        return Status == ProcessingJobStatus.Failed && RetryCount < MaxRetries;
    }

    /// <summary>
    /// Resets job for retry
    /// </summary>
    public void ResetForRetry()
    {
        if (!CanRetry())
            throw new InvalidOperationException("Job cannot be retried");

        RetryCount++;
        Status = ProcessingJobStatus.Queued;
        StartedAt = null;
        CompletedAt = null;
        Errors.Clear();
        Steps.Clear();
        CurrentStep = ProcessingStepType.Validation;
    }

    /// <summary>
    /// Updates job progress
    /// </summary>
    public void UpdateProgress(float percentage)
    {
        ProgressPercentage = Math.Min(percentage, 100f);
    }
}

public enum ProcessingJobType
{
    VideoOptimization = 0,
    ThumbnailGeneration = 1,
    MetadataExtraction = 2,
    Encoding = 3,
    FullProcessing = 4
}

public enum ProcessingJobStatus
{
    Queued = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Paused = 5
}

public enum ProcessingStepType
{
    Validation = 0,
    Preparation = 1,
    Encoding = 2,
    Optimization = 3,
    ThumbnailGeneration = 4,
    MetadataExtraction = 5,
    Finalization = 6
}

public class ProcessingStep
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public ProcessingStepType StepType { get; set; }
    public StepStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? StepOutput { get; set; }
}

public enum StepStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}
