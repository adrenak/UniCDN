using System;
using Adrenak.Unex;

namespace WebSocketSharp {
	public static class WebSocketSharpExtensions {
		public static void ConnectAsync(this WebSocket socket, Action OnConnect, Action<Exception> OnError) {
			Action onDone = () => { };

			EventHandler onConnect = (sender, e) => {
				if (OnConnect != null) OnConnect();
				onDone();
			};

			EventHandler<CloseEventArgs> onClose = (sender, e) => {
				if (OnError != null) OnError(new Exception(e.Reason));
				onDone();
			};

			onDone = () => {
				socket.OnOpen -= onConnect;
				socket.OnClose -= onClose;
			};

			socket.OnOpen += onConnect;
			socket.OnClose += onClose;

			socket.ConnectAsync();
		}

		public static IPromise ConnectPromise(this WebSocket socket) {
			var promise = new Promise();
			socket.ConnectAsync(
				() => promise.Resolve(),
				exception => promise.Reject(exception)
			);
			return promise;
		}

		public static void CloseAsync(this WebSocket socket, Action OnDisconnect) {
			if (!socket.IsAlive)
				if (OnDisconnect != null) OnDisconnect();

			Action onDone = () => { };

			EventHandler<CloseEventArgs> onClose = (sender, e) => {
				OnDisconnect();
				onDone();
			};

			onDone = () => socket.OnClose -= onClose;
			socket.OnClose += onClose;
			socket.CloseAsync();
		}

		public static IPromise ClosePromise(this WebSocket socket) {
			var promise = new Promise();
			socket.CloseAsync(
				() => promise.Resolve()
			);
			return promise;
		}
	}
}
