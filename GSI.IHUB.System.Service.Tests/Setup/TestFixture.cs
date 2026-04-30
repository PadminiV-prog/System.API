using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Text;
using GSI.IHUB.System.Service.Contracts;

namespace GSI.IHUB.System.Service.Tests.Setup;

public class TestFixture
{
    public Mock<ILogger<GSI.IHUB.System.Service.Functions.ProcessRequestFunction>> LoggerMock { get; } = new();
    public Mock<ISysService> SysServiceMock { get; } = new();
    public Mock<IValidationService> ValidationServiceMock { get; } = new();
    public Mock<ITransformAdapterFactory> TransformAdapterFactoryMock { get; } = new();
    public Mock<ITransformAdapter> TransformAdapterMock { get; } = new();

    public FunctionContext CreateFunctionContext(string? correlationId = "test-correlation")
    {
        var contextMock = new Mock<FunctionContext>();
        var items = new Dictionary<object, object>();

        if (correlationId is not null)
        {
            items[GSI.IHUB.System.Service.Model.ApplicationConstants.CorrelationIdHeaderKey] = correlationId;
        }

        contextMock.SetupProperty(c => c.Items, items);
        return contextMock.Object;
    }

    public HttpRequestData CreateHttpRequestData(FunctionContext context, string body)
    {
        return new TestHttpRequestData(context, body);
    }

    private sealed class TestHttpRequestData : HttpRequestData
    {
        private readonly HttpHeadersCollection _headers = new();

        public TestHttpRequestData(FunctionContext functionContext, string body)
            : base(functionContext)
        {
            Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            Url = new Uri("https://localhost/system/process");
        }

        public override Stream Body { get; }
        public override HttpHeadersCollection Headers => _headers;
        public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();
        public override Uri Url { get; }
        public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();
        public override string Method => "POST";

        public override HttpResponseData CreateResponse() => throw new NotImplementedException();
    }
}
