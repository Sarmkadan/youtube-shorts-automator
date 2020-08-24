using System.Collections.Generic;
using System.Linq;
using YouTubeShortAutomator.Domain.Models;
using YouTubeShortAutomator.Exceptions;

namespace YouTubeShortAutomator.Services
{
    /// <summary>
    /// Provides extension methods for the <see cref="ChannelService"/> class.
    /// </summary>
    public static class ChannelServiceExtensions
    {
        /// <summary>
        /// Batch updates the status of multiple YouTube channels.
        /// </summary>
        /// <param name="service">The <see cref="ChannelService"/> instance.</param>
        /// <param name="channels">The collection of <see cref="YouTubeChannel"/> instances to update.</param>
        /// <param name="isActive">The new status to apply to all channels.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="channels"/> is null.</exception>
        public static void BatchUpdateChannelStatus(this ChannelService service, IEnumerable<YouTubeChannel> channels, bool isActive)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(channels);

            foreach (var channel in channels)
            {
                service.UpdateChannelStatus(channel, isActive);
            }
        }

        /// <summary>
        /// Returns all channels that have invalid credentials.
        /// </summary>
        /// <param name="service">The <see cref="ChannelService"/> instance.</param>
        /// <param name="channels">The collection of <see cref="YouTubeChannel"/> instances to validate.</param>
        /// <returns>An <see cref="IReadOnlyList{YouTubeChannel}"/> of channels with invalid credentials.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="channels"/> is null.</exception>
        public static IReadOnlyList<YouTubeChannel> GetInvalidChannels(this ChannelService service, IEnumerable<YouTubeChannel> channels)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(channels);

            return channels
                .Where(channel => !service.ValidateChannelCredentials(channel))
                .ToList();
        }

        /// <summary>
        /// Returns all channels that need their tokens refreshed.
        /// </summary>
        /// <param name="service">The <see cref="ChannelService"/> instance.</param>
        /// <param name="channels">The collection of <see cref="YouTubeChannel"/> instances to check.</param>
        /// <returns>An <see cref="IReadOnlyList{YouTubeChannel}"/> of channels needing token refresh.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> or <paramref name="channels"/> is null.</exception>
        public static IReadOnlyList<YouTubeChannel> FindChannelsNeedingTokenRefresh(this ChannelService service, IEnumerable<YouTubeChannel> channels)
        {
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(channels);

            return channels
                .Where(channel => service.NeedsTokenRefresh(channel))
                .ToList();
        }
    }
}
