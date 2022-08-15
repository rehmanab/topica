using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHttpClientService
    {
        void AddHeader(string key, string value);
        void ClearHeaders();
        
        Task<string> GetAsync(string url);
        Task<T> GetAsync<T>(string url);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent content, CancellationToken ct = default);
        Task<HttpResponseMessage> PostAsync<TSource>(string url, TSource source, CancellationToken ct = default);
        Task<TResult> PostAsync<TSource, TResult>(string url, TSource source, CancellationToken ct = default);
        Task<HttpResponseMessage> PutAsync(string url, HttpContent content, CancellationToken ct = default);
        Task<HttpResponseMessage> PutAsync<TSource>(string url, TSource source, CancellationToken ct = default);
        Task<TResult> PutAsync<TSource, TResult>(string url, TSource source, CancellationToken ct = default);
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent content);
        Task<TResult> PatchAsync<TSource, TResult>(string url, TSource source);
        Task<HttpResponseMessage> DeleteAsync(string url);
    }
}