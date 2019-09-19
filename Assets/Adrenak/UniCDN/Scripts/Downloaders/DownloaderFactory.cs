namespace Adrenak.UniCDN {
	public static class DownloaderFactory {
		public static IDownloader Create(Provider provider) {
			switch (provider) {
				case Provider.PlayerIO:
					return new PlayerIODownloader();
				default:
					return new NullDownloader();
			}
		}
	}
}