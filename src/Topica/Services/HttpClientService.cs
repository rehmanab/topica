using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Topica.Contracts;

namespace Topica.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void AddHeader(string key, string value) => _httpClient.DefaultRequestHeaders.Add(key, value);
        public void ClearHeaders() => _httpClient.DefaultRequestHeaders.Clear();

        public async Task<string> GetAsync(string url)
        {
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            using var content = response.Content;
            var result = await content.ReadAsStringAsync();

            return result;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            var result = await GetAsync(url);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync(new Uri(url), content, ct);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PostAsync<TSource>(string url, TSource source, CancellationToken ct = default)
        {
            var json = JsonConvert.SerializeObject(source);
            var response = await _httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), ct);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<TResult> PostAsync<TSource, TResult>(string url, TSource source, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), ct);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsync(new Uri(url), content, ct);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PutAsync<TSource>(string url, TSource source, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), ct);

            if (response.IsSuccessStatusCode) return response;

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<TResult> PutAsync<TSource, TResult>(string url, TSource source, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"), ct);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
        {
            return await _httpClient.PatchAsync(url, content);
        }

        public async Task<TResult> PatchAsync<TSource, TResult>(string url, TSource source)
        {
            var response = await _httpClient.PatchAsync(url, new StringContent(JsonConvert.SerializeObject(source), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<TResult>(await response.Content.ReadAsStringAsync());

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(error);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await _httpClient.DeleteAsync(url);
        }
    }
}