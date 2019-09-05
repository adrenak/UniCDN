using System;
using UniRx.Async;

namespace Adrenak.UniCDN {
	public interface IDownloader {
		Provider GetProvider();

		void Init(object[] data, Action onSuccess, Action<Exception> onFailure);
		UniTask Init(object[] data);

		void GetURL(string key, Action<string> onSuccess, Action<Exception> onFailure);
		UniTask<string> GetURL(string key);

		void Download(string url, Action<byte[]> onSuccess, Action<Exception> onFailure);
		UniTask<byte[]> Download(string url);
	}
}