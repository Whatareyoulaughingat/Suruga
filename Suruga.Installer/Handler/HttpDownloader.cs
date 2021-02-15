using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Suruga.Installer.Handler
{
    public class HttpDownloader : IDisposable
    {
        private readonly string downloadUrl;
        private readonly string destinationFilePath;

        private HttpClient httpClient;

        public delegate Task ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

        public event ProgressChangedHandler ProgressChanged;

        public HttpDownloader(string downloadUrl, string destinationFilePath)
        {
            this.downloadUrl = downloadUrl;
            this.destinationFilePath = destinationFilePath;
        }

        /// <summary>
        /// Starts an asynchronous file download.
        /// </summary>
        /// <returns>[<see cref="Task"/>] An asynchronous operation.</returns>
        public async Task DownloadAsync()
        {
            httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };

            using HttpResponseMessage response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            await DownloadFileFromHttpResponseMessageAsync(response);
        }

        public void Dispose()
            => httpClient?.Dispose();

        private async Task DownloadFileFromHttpResponseMessageAsync(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await ProcessContentStreamAsync(totalBytes, contentStream);
        }

        private async Task ProcessContentStreamAsync(long? totalDownloadSize, Stream contentStream)
        {
            long totalBytesRead = 0L;
            long readCount = 0L;
            byte[] buffer = new byte[4096];
            bool isMoreToRead = true;

            using FileStream fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            do
            {
                int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    isMoreToRead = false;
                    await TriggerProgressChangedAsync(totalDownloadSize, totalBytesRead);

                    continue;
                }

                await fileStream.WriteAsync(buffer, 0, bytesRead);

                totalBytesRead += bytesRead;
                readCount += 1;

                if (readCount % 100 == 0)
                {
                    await TriggerProgressChangedAsync(totalDownloadSize, totalBytesRead);
                }
            }
            while (isMoreToRead);
        }

        private async Task TriggerProgressChangedAsync(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
            {
                return;
            }

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
            {
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);
            }

            await ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }
    }
}
