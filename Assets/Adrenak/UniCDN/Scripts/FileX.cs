using System.IO;
using System.Threading.Tasks;
using System;

namespace Adrenak.UniCDN {
	public static class FileX {
		public static Task DeleteAsync(string fileName) {
			try {
				var info = new FileInfo(fileName);
				return Task.FromResult(info.DeleteAsync());
			}
			catch(Exception e) {
				throw e;
			}
		}
	}
}
