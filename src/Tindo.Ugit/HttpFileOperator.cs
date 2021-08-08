namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Http File Operator.
    /// </summary>
    internal class HttpFileOperator : IFileOperator
    {
        private readonly string remotePath;

        private readonly HttpClient client;

        private readonly ILogger<HttpFileOperator> logger;

        public HttpFileOperator(string remoteUrl, IHttpClientFactory httpClientFactory, ILogger<HttpFileOperator> logger)
        {
            this.remotePath = remoteUrl;
            this.client = httpClientFactory.CreateClient(remoteUrl);
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string CurrentDirectory => throw new NotImplementedException();

        /// <inheritdoc/>
        public void CreateDirectory(string directory, bool force = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Delete(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Exists(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public byte[] Read(string path)
        {
            string url = $"{this.remotePath}/{path}";
            this.logger.LogInformation($"Read byte from {url}");
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var response = this.client.SendAsync(requestMessage).Result;
            return response.Content.ReadAsByteArrayAsync().Result;
        }

        /// <inheritdoc/>
        public bool TryRead(string path, out byte[] bytes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> Walk(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Write(string path, byte[] bytes)
        {
            string url = $"{this.remotePath}/{path}";
            this.logger.LogInformation($"Write bytes to {url}");
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Post,
                Content = new ByteArrayContent(bytes),
            };

            this.client.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
