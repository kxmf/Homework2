using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Homework2.Api.Interceptors;

public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var methodName = context.Method;
        logger.LogInformation("[gRPC Request] Starting {Method} with payload: {@Request}", methodName, request);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await continuation(request, context);
            stopwatch.Stop();

            logger.LogInformation("[gRPC Response] Finished {Method} in {Elapsed}ms with response: {@Response}",
                methodName, stopwatch.ElapsedMilliseconds, response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex, "[gRPC Error] Method {Method} failed after {Elapsed}ms: {Message}",
                methodName, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
