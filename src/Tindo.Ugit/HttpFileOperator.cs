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
        private readonly string remoteUrl;

        private readonly HttpClient client;

        private readonly ILogger<HttpFileOperator> logger;

        public HttpFileOperator(string remoteUrl, IHttpClientFactory httpClientFactory, ILogger<HttpFileOperator> logger)
        {
            this.remoteUrl = remoteUrl;
            this.client = httpClientFactory.CreateClient(this.remoteUrl);
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
            throw new NotImplementedException();
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
