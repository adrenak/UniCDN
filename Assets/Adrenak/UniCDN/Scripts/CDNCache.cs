using System;
using System.IO;
using System.Text;
using UniRx.Async;
using System.Threading.Tasks;
using UnityEngine;

namespace Adrenak.UniCDN {
	public class CDNCache {
		const string TAG = "[CDN_CACHE]";

		public class Config {
			public string rootDir;

			// VERSION FILE NOMENCLATURE
			public delegate void VersionFileNomenclatureMethod(string fileName, Action<string> onSuccess, Action<Exception> onFailure);
			public VersionFileNomenclatureMethod versionFileNomenclatureMethod;

			public UniTask<string> GetVersionFileName(string fileName) {
				var source = new UniTaskCompletionSource<string>();
				versionFileNomenclatureMethod(fileName,
					result => source.TrySetResult(result),
					error => source.TrySetException(error)
				);
				return source.Task;
			}
		}

		public bool enableLogging;
		public Config config;
		public IDownloader m_Downloader { get; private set; }

		// ================================================
		// INITIALIZATION
		// ================================================
		public UniTask Init(Config config, IDownloader downloader) {
			this.config = config;
			Directory.CreateDirectory(config.rootDir);
			m_Downloader = downloader;
			return m_Downloader.Init(null);
		}

		// ================================================
		// LOCAL VERSION
		// ================================================
		#region LOCAL_VERSION
		public UniTask<string> GetLocalVersion(string subPath) {
			var source = new UniTaskCompletionSource<string>();
			GetLocalVersion(subPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void GetLocalVersion(string subPath, Action<string> onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(filePath);

				var verFileName = await GetVerFileNameFromFileName(fileName);
				var verFilePath = GetVerFilePath(filePath, verFileName);

				// If either file is not present, we delete both and return null
				if (!File.Exists(verFilePath) || !File.Exists(filePath)) {
					if (File.Exists(verFilePath)) await FileX.DeleteAsync(verFilePath);
					if (File.Exists(filePath)) await FileX.DeleteAsync(filePath);

					onSuccess?.Invoke(null);
				}

				using (var reader = File.OpenText(verFilePath)) {
					var fileText = await reader.ReadToEndAsync();

					reader.Close();
					Log("Fetched local version for " + subPath + ". Version is " + fileText);
					onSuccess?.Invoke(fileText);
				}
			}
			catch (Exception e) {
				LogError("Could not fetch local version for " + subPath + " --> " + e);
				onFailure?.Invoke(e);
			}
		}

		public UniTask SetLocalVersion(string subPath, string version) {
			var source = new UniTaskCompletionSource();
			SetLocalVersion(subPath, version,
				() => source.TrySetResult(),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void SetLocalVersion(string subPath, string version, Action onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(filePath);

				var verFileName = await GetVerFileNameFromFileName(fileName);
				var verFilePath = GetVerFilePath(filePath, verFileName);

				File.WriteAllText(verFilePath, version);
				Log("Version for " + subPath + " set to " + version);
				onSuccess?.Invoke();
			}
			catch (Exception e) {
				LogError("Could not set version for " + subPath + " --> " + e);
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// REMOTE VERSION
		// ================================================
		#region REMOTE_VERSION
		public UniTask<string> GetRemoteVersion(string remoteSubPath) {
			var source = new UniTaskCompletionSource<string>();
			GetRemoteVersion(remoteSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void GetRemoteVersion(string subPath, Action<string> onSuccess, Action<Exception> onFailure) {
			try {
				var fullPath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(fullPath);
				var verFileName = await GetVerFileNameFromFileName(fileName);
				var verSubPath = subPath.Replace(fileName, verFileName);

				var verFileURL = await m_Downloader.GetURL(verSubPath);
				var bytes = await m_Downloader.Download(verFileURL);
				var version = Encoding.UTF8.GetString(bytes);
				Log("Remote version for " + subPath + " is " + version);
				onSuccess?.Invoke(version);
			}
			catch (Exception e) {
				LogError("Could not fetch remote version for " + subPath);
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// IS UP TO DATE
		// ================================================
		#region IS_UP_TO_DATE
		public UniTask<bool> IsUpToDate(string commonSubPath) {
			return IsUpToDate(commonSubPath, commonSubPath);
		}

		public UniTask<bool> IsUpToDate(string localSubPath, string remoteSubPath) {
			var source = new UniTaskCompletionSource<bool>();
			IsUpToDate(localSubPath, remoteSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public void IsUpToDate(string commonSubPateh, Action<bool> onSuccess, Action<Exception> onFailure) {
			IsUpToDate(commonSubPateh, commonSubPateh, onSuccess, onFailure);
		}

		public async void IsUpToDate(string localSubPath, string remoteSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			try {
				var localVersion = await GetLocalVersion(localSubPath);
				var remoteVersion = await GetRemoteVersion(remoteSubPath);

				bool areVersionsEqual = localVersion.Equals(remoteVersion);
				if (!areVersionsEqual) {
					Log(localSubPath + " is not up to date. Local version is " + localVersion + ". Remote version is " + remoteVersion);
					onSuccess?.Invoke(false);
					return;
				}
				onSuccess?.Invoke(File.Exists(config.rootDir.AppendPath(localSubPath)));
			}
			catch (Exception e) {
				LogError("Could not perform consistency check. --> " + e);
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// LOCAL FILE
		// ================================================
		#region LOCAL_FILE
		// READ
		public class ReadLocalFileResult {
			public byte[] bytes;
			public string version;
		}

		public UniTask<ReadLocalFileResult> ReadLocalFile(string localSubPath) {
			var source = new UniTaskCompletionSource<ReadLocalFileResult>();
			ReadLocalFile(localSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void ReadLocalFile(string subPath, Action<ReadLocalFileResult> onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(filePath);
				var verFileName = await GetVerFileNameFromFileName(fileName);
				var verFilePath = GetVerFilePath(filePath, verFileName);

				using (var versionFileStream = File.OpenRead(verFilePath))
				using (var fileStream = File.OpenRead(filePath)) {
					var fileBuffer = new byte[fileStream.Length];
					var versionFileBuffer = new byte[versionFileStream.Length];

					var fileRead = fileStream.ReadAsync(fileBuffer, 0, fileBuffer.Length);
					var versionFileRead = versionFileStream.ReadAsync(versionFileBuffer, 0, versionFileBuffer.Length);

					await Task.WhenAll(fileRead, versionFileRead);
					var version = Encoding.UTF8.GetString(versionFileBuffer);

					versionFileStream.Close();
					fileStream.Close();
					Log("Finished reading from " + subPath + " and its version file");
					var result = new ReadLocalFileResult {
						bytes = fileBuffer,
						version = version
					};
					onSuccess?.Invoke(result);
				}
			}
			catch (Exception e) {
				LogError("Could not read file and/or version files for " + subPath);
				onFailure?.Invoke(e);
			}
		}

		// WRITE
		public UniTask WriteLocalFile(string subPath, byte[] bytes, string version) {
			var source = new UniTaskCompletionSource();
			WriteLocalFile(subPath, bytes, version,
				() => source.TrySetResult(),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void WriteLocalFile(string subPath, byte[] bytes, string version, Action onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(filePath);
				var fileDir = filePath.Replace(fileName, string.Empty);
				Directory.CreateDirectory(fileDir);

				var versionFileName = await GetVerFileNameFromFileName(fileName);
				var versionFilePath = GetVerFilePath(filePath, versionFileName);

				using (var fileStream = File.OpenWrite(filePath))
				using (var versionFileStream = File.OpenWrite(versionFilePath)) {
					var fileWrite = fileStream.WriteAsync(bytes, 0, bytes.Length);

					var versionBytes = Encoding.UTF8.GetBytes(version);
					var versionFileWrite = versionFileStream.WriteAsync(versionBytes, 0, versionBytes.Length);

					await Task.WhenAll(fileWrite, versionFileWrite);
					fileStream.Close();
					versionFileStream.Close();

					Log("Wrote to " + subPath + " and its version file");
					onSuccess?.Invoke();
				}
			}
			catch (Exception e) {
				LogError("Could not write to " + subPath + " and/or its version file");
				onFailure?.Invoke(e);
			}
		}

		// DELETE 
		public UniTask<bool> DeleteLocalFile(string subPath) {
			var source = new UniTaskCompletionSource<bool>();
			DeleteLocalFile(subPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public async void DeleteLocalFile(string subPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = GetFullFromSubPath(subPath);
				var fileName = Path.GetFileName(filePath);
				var verFileName = await GetVerFileNameFromFileName(fileName);
				var verFilePath = GetVerFilePath(filePath, verFileName);

				if (!File.Exists(filePath)) {
					onSuccess?.Invoke(false);
					return;
				}

				if (!File.Exists(verFilePath)) {
					onSuccess?.Invoke(false);
					return;
				}

				var fileDeletion = FileX.DeleteAsync(filePath);
				var versionFileDeletion = FileX.DeleteAsync(verFilePath);
				await Task.WhenAll(fileDeletion, versionFileDeletion);
				Log("Deleted file and version file for " + subPath);
				onSuccess?.Invoke(true);
			}
			catch (Exception e) {
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// UPDATE
		// ================================================
		#region UPDATE
		public UniTask<bool> UpdateFile(string commonSubPath) {
			return UpdateFile(commonSubPath, commonSubPath);
		}

		public UniTask<bool> UpdateFile(string localSubPath, string remoteSubPath) {
			var source = new UniTaskCompletionSource<bool>();
			UpdateFile(localSubPath, remoteSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public void UpdateFile(string commonSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			UpdateFile(commonSubPath, commonSubPath, onSuccess, onFailure);
		}

		public async void UpdateFile(string localSubPath, string remoteSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			try {
				// Check if the file is up to date
				var isUpToDate = await IsUpToDate(localSubPath);
				if (isUpToDate) {
					onSuccess?.Invoke(false);
					return;
				}

				// Download the new file
				var url = await m_Downloader.GetURL(remoteSubPath);
				Log("Downloading file from " + url);
				var bytes = await m_Downloader.Download(url);
				Log("Downloaded finished from " + url);

				// Delete the existing files
				await DeleteLocalFile(localSubPath);

				// Write the new files
				var remoteVersion = await GetRemoteVersion(remoteSubPath);
				await WriteLocalFile(localSubPath, bytes, remoteVersion);
				Log("Updated " + localSubPath);
				onSuccess?.Invoke(true);
			}
			catch (Exception e) {
				LogError("Error updating " + localSubPath + " --> " + e);
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// HELPERS
		// ================================================
		void Log(object msg) {
			if (enableLogging)
				Console.Out.Log(TAG, msg);
		}

		void LogError(object error) {
			if (enableLogging)
				Console.Out.LogError(TAG, error);
		}

		string GetFullFromSubPath(string subPath) {
			return config.rootDir.AppendPath(subPath);
		}

		UniTask<string> GetVerFileNameFromFileName(string fileName) {
			return config.GetVersionFileName(fileName);
		}

		string GetVerFilePath(string filePath, string versionFileName) {
			var fileName = Path.GetFileName(filePath);
			return filePath.Replace(fileName, versionFileName);
		}
	}
}
