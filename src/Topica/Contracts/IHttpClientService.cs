using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Topica.Contracts
{
    public interface IHttpClientService
    {
        void AddHeader(string key, string value);
        void ClearHeaders();
        Task<string> GetAsync(string url, CancellationToken cancellationToken);
        Task<T> GetAsync<T>(string url, CancellationToken cancellationToken);
        Task<HttpResponseMessage> GetHttpResponseMessageAsync(string url, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync(string url, HttpContent? content, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PostAsync<TSource>(string url, TSource source, CancellationToken cancellationToken);
        Task<TResult> PostAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PutAsync(string url, HttpContent? content, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PutAsync<TSource>(string url, TSource source, CancellationToken cancellationToken);
        Task<TResult> PutAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken);
        Task<HttpResponseMessage> PatchAsync(string url, HttpContent content, CancellationToken cancellationToken);
        Task<TResult> PatchAsync<TSource, TResult>(string url, TSource source, CancellationToken cancellationToken);
        Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken);
    }
}