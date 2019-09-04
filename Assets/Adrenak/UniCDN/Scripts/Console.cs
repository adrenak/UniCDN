using UnityEngine;

namespace Adrenak.UniCDN {
	public static class Console {
		public static Logger Out { get; private set; }

		static Console() {
			Out = new Logger(Debug.unityLogger);
		}
	}
}