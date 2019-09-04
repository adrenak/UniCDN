namespace Adrenak.UniCDN {
	public static class DownloaderBuilder {
		public static IDownloader Build(Provider provider) {
			switch (provider) {
				case Provider.PlayerIO:
					return new PlayerIODownloader();
				default:
					return new NullDownloader();
			}
		}
	}
}