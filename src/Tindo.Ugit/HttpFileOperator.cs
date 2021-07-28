namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Http File Operator.
    /// </summary>
    public class HttpFileOperator : IFileOperator
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

        public string CurrentDirectory => throw new NotImplementedException();

        public void CreateDirectory(string directory, bool force = true)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        public byte[] Read(string path)
        {
            string url = $"{this.remotePath}/{path}";
            this.logger.LogInformation($"Get: {url}");
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var response = this.client.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult().Encode();
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new NotImplementedException();
        }

        public void Write(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
