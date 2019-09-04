using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using System;

namespace Adrenak.UniCDN {
	public static class Extensions {
		public static string AppendPath(this string path1, string path2) {
			var splits = path2.Split(new char[] { '/', '\\' });
			List<string> list = new List<string>() { path1 };
			list.AddRange(splits.ToList());

			var interm = Path.Combine(list.ToArray());
			interm = interm.Replace('/', Path.DirectorySeparatorChar);
			interm = interm.Replace('\\', Path.DirectorySeparatorChar);

			return interm;
		}

		public static Task DeleteAsync(this FileInfo info) {
			return Task.Factory.StartNew(() => info.Delete());
		}
	}
}
