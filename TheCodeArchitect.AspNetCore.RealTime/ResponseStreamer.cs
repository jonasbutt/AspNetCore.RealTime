using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TheCodeArchitect.AspNetCore.RealTime
{
    public interface IResponseStreamer
    {
        Task StreamResponse(Func<Func<string, Task>, CancellationToken, Task> stream);
    }

    public class ResponseStreamer : IResponseStreamer
    {
        private const string EventStreamContentType = "text/event-stream";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResponseStreamer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task StreamResponse(Func<Func<string, Task>, CancellationToken, Task> stream)
        {
            var response = _httpContextAccessor.HttpContext.Response;
            var requestCancellationToken = _httpContextAccessor.HttpContext.RequestAborted;
            var eventStreamRequested =
                _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Accept", out var acceptHeader)
                && acceptHeader == EventStreamContentType;
            try
            {
                response.Headers.Add("Content-Type", EventStreamContentType);
                await stream(
                    async message =>
                    {
                        var output = eventStreamRequested ? $"data: {message}\r\r" : $"{message}{Environment.NewLine}";
                        await response.WriteAsync(output, requestCancellationToken);
                        await response.Body.FlushAsync(requestCancellationToken);
                    },
                    requestCancellationToken);

            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
