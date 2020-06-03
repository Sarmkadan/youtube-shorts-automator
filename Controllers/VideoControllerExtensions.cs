// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using YouTubeShortsAutomator.Domain.Models;

namespace YouTubeShortsAutomator.Controllers;

/// <summary>
/// Extension methods for VideoController providing common operations and convenience methods
/// </summary>
public static class VideoControllerExtensions
{
    /// <summary>
    /// Gets videos by their IDs in a single batch request
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="videoIds">Collection of video IDs to fetch</param>
    /// <returns>IActionResult containing found videos</returns>
    /// <exception cref="ArgumentNullException">Thrown when videoIds is null</exception>
    public static async Task<IActionResult> GetVideosBatchAsync(
        this VideoController controller,
        IEnumerable<Guid> videoIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(videoIds);

        if (!videoIds.Any())
        {
            return controller.Ok(new { success = true, data = Array.Empty<object>() });
        }

        var videos = new List<object>();
        foreach (var videoId in videoIds)
        {
            var result = await controller.GetVideo(videoId);
            if (result is OkObjectResult okResult)
            {
                videos.Add(okResult.Value);
            }
        }

        return controller.Ok(new { success = true, data = videos });
    }

    /// <summary>
    /// Gets videos for multiple users in parallel
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="userIds">Collection of user IDs to fetch videos for</param>
    /// <param name="pageSize">Number of videos per page</param>
    /// <returns>IActionResult containing all user videos grouped by user</returns>
    /// <exception cref="ArgumentNullException">Thrown when userIds is null</exception>
    public static async Task<IActionResult> GetUserVideosBatchAsync(
        this VideoController controller,
        IEnumerable<Guid> userIds,
        int pageSize = 10)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(userIds);

        if (!userIds.Any())
        {
            return controller.Ok(new { success = true, data = new Dictionary<Guid, object>() });
        }

        var resultDictionary = new Dictionary<Guid, object>();

        foreach (var userId in userIds)
        {
            var result = await controller.GetUserVideos(userId, 1, pageSize);
            if (result is OkObjectResult okResult)
            {
                resultDictionary[userId] = okResult.Value;
            }
        }

        return controller.Ok(new { success = true, data = resultDictionary });
    }

    /// <summary>
    /// Processes multiple videos in parallel with individual job tracking
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="videoIds">Collection of video IDs to process</param>
    /// <returns>IActionResult containing all processing jobs</returns>
    /// <exception cref="ArgumentNullException">Thrown when videoIds is null</exception>
    public static async Task<IActionResult> ProcessVideosAsync(
        this VideoController controller,
        IEnumerable<Guid> videoIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(videoIds);

        if (!videoIds.Any())
        {
            return controller.Ok(new { success = true, data = Array.Empty<object>() });
        }

        var jobs = new List<object>();
        var processingErrors = new List<object>();

        foreach (var videoId in videoIds)
        {
            try
            {
                var result = await controller.ProcessVideo(videoId);
                if (result is AcceptedResult acceptedResult && acceptedResult.Value != null)
                {
                    jobs.Add(acceptedResult.Value);
                }
            }
            catch (Exception ex)
            {
                processingErrors.Add(new
                {
                    videoId,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        return controller.Ok(new
        {
            success = true,
            data = new
            {
                jobs,
                processingErrors,
                totalVideos = videoIds.Count(),
                successfulJobs = jobs.Count,
                failedJobs = processingErrors.Count
            }
        });
    }

    /// <summary>
    /// Uploads multiple videos to YouTube in parallel
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="videoIdUserIdPairs">Collection of (videoId, userId) pairs</param>
    /// <returns>IActionResult containing all upload results</returns>
    /// <exception cref="ArgumentNullException">Thrown when videoIdUserIdPairs is null</exception>
    public static async Task<IActionResult> UploadVideosAsync(
        this VideoController controller,
        IEnumerable<(Guid videoId, Guid userId)> videoIdUserIdPairs)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(videoIdUserIdPairs);

        if (!videoIdUserIdPairs.Any())
        {
            return controller.Ok(new { success = true, data = Array.Empty<object>() });
        }

        var uploadResults = new List<object>();
        var uploadErrors = new List<object>();

        foreach (var (videoId, userId) in videoIdUserIdPairs)
        {
            try
            {
                var result = await controller.UploadVideo(videoId, userId);
                if (result is AcceptedResult acceptedResult && acceptedResult.Value != null)
                {
                    uploadResults.Add(acceptedResult.Value);
                }
            }
            catch (Exception ex)
            {
                uploadErrors.Add(new
                {
                    videoId,
                    userId,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        return controller.Ok(new
        {
            success = true,
            data = new
            {
                uploadResults,
                uploadErrors,
                totalUploads = videoIdUserIdPairs.Count(),
                successfulUploads = uploadResults.Count,
                failedUploads = uploadErrors.Count
            }
        });
    }

    /// <summary>
    /// Gets job status for multiple processing jobs
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="jobIds">Collection of job IDs to check status for</param>
    /// <returns>IActionResult containing all job statuses</returns>
    /// <exception cref="ArgumentNullException">Thrown when jobIds is null</exception>
    public static async Task<IActionResult> GetJobStatusesAsync(
        this VideoController controller,
        IEnumerable<Guid> jobIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(jobIds);

        if (!jobIds.Any())
        {
            return controller.Ok(new { success = true, data = Array.Empty<object>() });
        }

        var jobStatuses = new Dictionary<Guid, object>();
        var missingJobs = new List<Guid>();

        foreach (var jobId in jobIds)
        {
            var result = await controller.GetJobStatus(jobId);
            if (result is OkObjectResult okResult)
            {
                jobStatuses[jobId] = okResult.Value;
            }
            else
            {
                missingJobs.Add(jobId);
            }
        }

        return controller.Ok(new
        {
            success = true,
            data = new
            {
                jobStatuses,
                missingJobs,
                totalJobs = jobIds.Count(),
                foundJobs = jobStatuses.Count,
                notFoundJobs = missingJobs.Count
            }
        });
    }

    /// <summary>
    /// Publishes multiple videos that have been uploaded
    /// </summary>
    /// <param name="controller">The VideoController instance</param>
    /// <param name="videoIds">Collection of video IDs to publish</param>
    /// <returns>IActionResult containing publish results</returns>
    /// <exception cref="ArgumentNullException">Thrown when videoIds is null</exception>
    public static async Task<IActionResult> PublishVideosAsync(
        this VideoController controller,
        IEnumerable<Guid> videoIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(videoIds);

        if (!videoIds.Any())
        {
            return controller.Ok(new { success = true, data = Array.Empty<object>() });
        }

        var publishResults = new List<object>();
        var publishErrors = new List<object>();
        var videosNotUploaded = new List<Guid>();

        foreach (var videoId in videoIds)
        {
            try
            {
                var videoResult = await controller.GetVideo(videoId);
                if (videoResult is OkObjectResult okVideoResult)
                {
                    var video = okVideoResult.Value;
                    if (video?.GetType().GetProperty("UploadResult") != null)
                    {
                        var result = await controller.PublishVideo(videoId);
                        if (result is OkObjectResult publishResult && publishResult.Value != null)
                        {
                            publishResults.Add(publishResult.Value);
                        }
                    }
                    else
                    {
                        videosNotUploaded.Add(videoId);
                    }
                }
            }
            catch (Exception ex)
            {
                publishErrors.Add(new
                {
                    videoId,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        return controller.Ok(new
        {
            success = true,
            data = new
            {
                publishResults,
                publishErrors,
                videosNotUploaded,
                totalVideos = videoIds.Count(),
                successfullyPublished = publishResults.Count,
                failedPublishes = publishErrors.Count,
                notUploaded = videosNotUploaded.Count
            }
        });
    }
}