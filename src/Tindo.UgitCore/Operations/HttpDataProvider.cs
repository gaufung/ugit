using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore
{
    /// <summary>
    /// Http Data provider.
    /// </summary>
    public class HttpDataProvider : IDataProvider
    {

        private readonly string remoteUrl;

        private readonly IHttpClientFactory httpClientFactory;

        private readonly ILogger<HttpDataProvider> logger;

        public HttpDataProvider(string remoteUrl, IHttpClientFactory httpClientFactory
        , ILoggerFactory loggerFactory)
        {
            this.remoteUrl = remoteUrl;
            this.httpClientFactory = httpClientFactory;
            this.logger = loggerFactory.CreateLogger<HttpDataProvider>();
        }
        
        public bool Exist(string path, bool isFile = true)
        {
            throw new System.NotImplementedException();
        }

        public void Write(string path, byte[] bytes)
        {
            var httpclient = this.httpClientFactory.CreateClient(this.remoteUrl);
            string url = path.Replace(Path.DirectorySeparatorChar, '/');
            this.logger.LogInformation($"Write: url: {url}");
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new ByteArrayContent(bytes),
            };

            var response = httpclient.Send(requestMessage);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new UgitException("failed to write the object");
            }
        }

        public byte[] Read(string path)
        {
            var httpClient = this.httpClientFactory.CreateClient(this.remoteUrl);
            string url = path.Replace(Path.DirectorySeparatorChar, '/');
            this.logger.LogInformation($"Read, url: {url}");
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            var response = httpClient.Send(requestMessage);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsByteArrayAsync().Result;
            }

            throw new UgitException("failed to get the object");
        }

        public void Delete(string path)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new System.NotImplementedException();
        }

        public void EmptyCurrentDirectory()
        {
            throw new System.NotImplementedException();
        }

        public string GitDirFullPath => this.remoteUrl;
        public string GitDir { get; }
        public Tree Index { get; set; }
        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public string HashObject(byte[] data, string type = "blob")
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            var httpClient = this.httpClientFactory.CreateClient(this.remoteUrl);
            string url = this.remoteUrl + $"/{Constants.Objects}/{oid}/expect/{expected}";
            this.logger.LogInformation($"GetObject, url: {url}");
            var requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            var response = httpClient.Send(requestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Content.ReadAsByteArrayAsync().Result;
            }

            throw new UgitException("failed to get the object");

        }

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            var httpclient = this.httpClientFactory.CreateClient(this.remoteUrl);
            string url = this.remoteUrl + $"/refs/{@ref}?deref={deref.ToString()}";
            this.logger.LogInformation($"UpdateRef: {url}");
            string body = JsonSerializer.Serialize(value);
            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = new StringContent(body, Encoding.UTF8),
                RequestUri = new Uri(url)
            };
            httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = httpclient.Send(requestMessage);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new UgitException("failed to update the ref");
            }
        }

        public RefValue GetRef(string @ref, bool deref = true)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            var httpclient = this.httpClientFactory.CreateClient(this.remoteUrl);
            string url = this.remoteUrl + $"/{prefix}?deref={deref.ToString()}";
            url = url.Replace(Path.DirectorySeparatorChar, '/');
            this.logger.LogInformation($"Get all Refs: {url}");
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri =  new Uri(url)
            };
            httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = httpclient.Send(httpRequestMessage);
            string body = response.Content.ReadAsStringAsync().Result;
            if (!string.IsNullOrWhiteSpace(body))
            {
                var refs = JsonSerializer.Deserialize<Dictionary<string, RefValue>>(body);
                if (refs != null)
                {
                    return refs.Select(kv => (kv.Key, kv.Value));
                }
            }

            throw new UgitException($"Failed to fetch the remote repository: {remoteUrl}");

        }

        public void DeleteRef(string @ref, bool deref = true)
        {
            throw new System.NotImplementedException();
        }

        public string GetOid(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool IsIgnore(string path)
        {
            throw new System.NotImplementedException();
        }

        public bool ObjectExist(string oid)
        {
            throw new System.NotImplementedException();
        }

        public Config Config { get; set; }
    }
}