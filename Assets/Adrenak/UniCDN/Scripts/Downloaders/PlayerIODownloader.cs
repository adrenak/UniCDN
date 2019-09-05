using UniRx.Async;
using System.Collections.Generic;
using System;
using RestSharp;
using PlayerIOClient;
using Adrenak.Unex;

namespace Adrenak.UniCDN {
	public class PlayerIODownloader : IDownloader {
		const string k_GameID = "kiza-yddy2zxb9elupyzdynyua";
		const string k_Username = "cdn";
		const string k_Password = "cdn";

		Client PlayerIOClient;

		public Provider GetProvider() {
			return Provider.PlayerIO;
		}

		// ================================================
		// INITIALIZATION
		// ================================================
		public UniTask Init(object[] data) {
			var source = new UniTaskCompletionSource();
			Init(data,
				() => source.TrySetResult(),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public void Init(object[] data, Action onSuccess, Action<Exception> onFailure) {
			Dispatcher.Init();

			PlayerIO.Authenticate(
				k_GameID,
				"public",
				new Dictionary<string, string> {
					{"register", "false"},
					{"username", k_Username},
					{"password", k_Password}
				},
				null,
				delegate (Client client) {
					PlayerIOClient = client;
					onSuccess?.Invoke();
				},
				delegate (PlayerIOError error) {
					onFailure?.Invoke(new Exception(error.Message));
				}
			);
		}
		
		// ================================================
		// GET URL
		// ================================================
		/// <summary>
		/// Returns the download URL for the content of the given key
		/// using the right CDN provider
		/// </summary>
		public void GetURL(string key, Action<string> onSuccess, Action<Exception> onFailure) {
			if (!key.StartsWith("/"))
				key = "/" + key;
			var url = PlayerIOClient.GameFS.GetUrl(key);
			if (string.IsNullOrEmpty(url))
				onFailure?.Invoke(new Exception("URL is null of empty"));
			else
				onSuccess?.Invoke(url);
		}

		/// <summary>
		/// Returns the download URL for the content of the given key
		/// using the right CDN provider
		/// </summary>
		public UniTask<string> GetURL(string key) {
			var source = new UniTaskCompletionSource<string>();
			if (!key.StartsWith("/"))
				key = "/" + key;
			var url = PlayerIOClient.GameFS.GetUrl(key);
			if (string.IsNullOrEmpty(url))
				source.TrySetException(new Exception("URL is null or empty"));
			else
				source.TrySetResult(url);
			return source.Task;
		}

		// ================================================
		// DOWNLOAD
		// ================================================
		public void Download(string url, Action<byte[]> onSuccess, Action<Exception> onFailure) {
			var client = new RestClient();
			var request = new RestRequest(url);

			var handler = client.ExecuteAsync(request)
				.Then(response => {
					if (response.IsSuccess())
						Dispatcher.Enqueue(() => onSuccess?.Invoke(response.RawBytes));
					else
						Dispatcher.Enqueue(() => onFailure?.Invoke(response.GetException()));
				})
				.Catch(error => Dispatcher.Enqueue(() => onFailure?.Invoke(error)));
		}

		public UniTask<byte[]> Download(string url) {
			var task = new UniTaskCompletionSource<byte[]>();
			Download(url,
				response => task.TrySetResult(response),
				error => task.TrySetException(error)
			);
			return task.Task;
		}
	}
}
