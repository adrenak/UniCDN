using System;
using UniRx.Async;
using System.Net;

namespace Adrenak.UniCDN {
	public static class Utils {
		public static UniTask<bool> CheckURL(string url) {
			var source = new UniTaskCompletionSource<bool>();
			CheckURL(url,
				result => source.TrySetResult(result)
			);
			return source.Task;
		}

		public static void CheckURL(string url, Action<bool> callback) {
			WebRequest request = WebRequest.Create(url);
			request.Timeout = 1000;
			request.Method = "HEAD";

			try {
				var response = request.GetResponseAsync();
				callback?.Invoke(true);
			}
			catch(Exception e) {
				callback?.Invoke(false);
			}
		}
	}
}
