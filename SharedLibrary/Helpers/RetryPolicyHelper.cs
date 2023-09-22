using Polly;
using Serilog;
using Polly.Retry;

namespace SharedLibrary.Helpers;

public static class RetryPolicyHelper
{
	public static RetryPolicy GetRetryPolicy()
	{
		var policy = Policy.Handle<HttpRequestException>()
			.RetryAsync(3, (exception, retryCount, context) =>
			{
				// Her yeniden deneme öncesinde yapılacak işlemleri burada gerçekleştirebilirsiniz
				Log.Warning($"HTTP isteği başarısız oldu. Yeniden deneme #{retryCount}. Hata: {exception.Message}");
			});

		return policy;
	}
}
