using System.Net;

namespace InovaGAB.API.Tests.TestSupport;

// substitui a chamada de rede real ao provedor de IA nos testes
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

    public FakeHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        _responder = responder;
    }

    public static FakeHttpMessageHandler ReturningJson(
        HttpStatusCode statusCode,
        string jsonBody)
    {
        return new FakeHttpMessageHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonBody)
            }));
    }

    public static FakeHttpMessageHandler Delaying(TimeSpan delay)
    {
        return new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _responder(request, cancellationToken);
    }
}
