using System;
using UniRx.Async;

namespace Adrenak.UniCDN {
	public class NullDownloader : IDownloader {
		public void Download(string url, Action<byte[]> onSuccess, Action<Exception> onFailure) {
			throw new NotImplementedException();
		}

		public UniTask<byte[]> Download(string url) {
			throw new NotImplementedException();
		}

		public Provider GetProvider() {
			throw new NotImplementedException();
		}

		public void GetURL(string key, Action<string> onSuccess, Action<Exception> onFailure) {
			throw new NotImplementedException();
		}

		public UniTask<string> GetURL(string key) {
			throw new NotImplementedException();
		}

		public void Init(object[] data, Action onSuccess, Action<Exception> onFailure) {
			throw new NotImplementedException();
		}

		public UniTask Init(object[] data) {
			throw new NotImplementedException();
		}
	}
}
