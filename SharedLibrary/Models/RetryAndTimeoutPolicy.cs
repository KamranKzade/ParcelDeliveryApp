using Polly;
using Microsoft.Extensions.Logging;

namespace SharedLibrary.Models;

public class RetryAndTimeoutPolicy
{
	private readonly ILogger _logger;

	public RetryAndTimeoutPolicy(ILogger logger)
	{
		_logger = logger;
	}

	public IAsyncPolicy GetRetryAndTimeoutPolicy()
	{
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.Or<Exception>()
			.RetryAsync(3, onRetry: (exception, retryCount) =>
			{
				_logger.LogWarning($"Retry count: {retryCount}");
			});

		var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

		return Policy.WrapAsync(retryPolicy, timeoutPolicy);
	}
}
