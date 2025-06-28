using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Topica.Contracts;

namespace Topica.Services
{
    public class HttpClientService(HttpClient httpClient) : IHttpClientService
    {
        public void AddHeader(string key, string value) => httpClient.DefaultRequestHeaders.Add(key, value);
        public void ClearHeaders() => httpClient.DefaultRequestHeaders.Clear();

        public async Task<string> GetAsync(string url, CancellationToken cancellationToken)
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            using var content = response.Content;
            var result = await content.ReadAsStringAsync();

            return result;
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            var result = await GetAsync(url, cancellationToken);

            return JsonConvert.DeserializeObject<T>(result);
        }
        
        public async Task<HttpResponseMessage> GetHttpResponseMessageAsync(string url, CancellationToken cancellationToken)
        {
            return await httpClient.GetAsync(url, cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent? content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(new Uri(url), content, cancellationToken);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PostAsync<TSource>(string url, TSource source, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(source);
            var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<TResult> PostAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent? content, CancellationToken cancellationToken)
        {
            var response = await httpClient.PutAsync(new Uri(url), content, cancellationToken);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PutAsync<TSource>(string url, TSource source, CancellationToken cancellationToken)
        {
            var response = await httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<TResult> PutAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken)
        {
            var response = await httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content, CancellationToken cancellationToken)
        {
            return await httpClient.PatchAsync(url, content, cancellationToken);
        }

        public async Task<TResult> PatchAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken)
        {
            var response = await httpClient.PatchAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken)
        {
            return await httpClient.DeleteAsync(url, cancellationToken);
        }
    }
}