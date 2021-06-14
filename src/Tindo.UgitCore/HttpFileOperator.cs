using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Tindo.UgitCore
{
    public class HttpFileOperator : IFileOperator
    {

        private readonly IHttpClientFactory httpClientFactory;

        private readonly ILogger<HttpFileOperator> logger;

        private readonly string remoteUrl;

        public HttpFileOperator(string remoteUrl, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            this.remoteUrl = remoteUrl;
            this.logger = loggerFactory.CreateLogger<HttpFileOperator>();
            this.httpClientFactory = httpClientFactory;
        }

        public string CurrentDirectory => string.Empty;

        public void CreateDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path, bool isFile = true)
        {
            var httpClient = this.httpClientFactory.CreateClient();
            Uri uri = new Uri($"{this.remoteUrl}/{path}").AddQuery("isFile", isFile.ToString().ToLower());
            this.logger.LogInformation($"Exists url: {uri.ToString()}");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri
            };

            var response = httpClient.Send(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            throw new InvalidOperationException($"failed to check the path: {path}");
        }

        public byte[] Read(string path)
        {
            var httpClient = this.httpClientFactory.CreateClient();
            Uri uri = new Uri($"{this.remoteUrl}/{path}");
            this.logger.LogInformation($"Read url: {uri}");

            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uri,
            };

            var response = httpClient.Send(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Content.ReadAsByteArrayAsync().Result;
            }

            throw new InvalidOperationException($"failed to read the path: {path}");
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            if (this.Exists(path))
            {
                bytes = this.Read(path);
                return true;
            }

            bytes = null;
            return false;
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new NotImplementedException();
        }

        public void Write(string path, byte[] bytes)
        {
            HttpClient httpClient = this.httpClientFactory.CreateClient();
            Uri uri = new Uri($"{this.remoteUrl}/{path}");
            this.logger.LogInformation($"Write url: {uri}");
            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = new ByteArrayContent(bytes)
            };

            httpClient.Send(request);
        }
    }
}
