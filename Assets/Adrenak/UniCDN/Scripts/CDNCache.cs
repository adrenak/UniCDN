using System;
using System.IO;
using System.Text;
using UniRx.Async;
using System.Threading.Tasks;
using UnityEngine;

namespace Adrenak.UniCDN {
	public class CDNCache {
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
				var fullPath = config.rootDir.AppendPath(subPath);
				var fileName = Path.GetFileName(fullPath);

				var versionFileName = await config.GetVersionFileName(fileName);
				var versionFilePath = fullPath.Replace(fileName, versionFileName);

				if (!File.Exists(versionFilePath))
					onSuccess?.Invoke(string.Empty);

				using (var reader = File.OpenText(versionFilePath)) {
					var fileText = await reader.ReadToEndAsync();

					reader.Close();
					onSuccess?.Invoke(fileText);
				}
			}
			catch (Exception e) {
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
				var fullPath = config.rootDir.AppendPath(subPath);
				var fileName = Path.GetFileName(fullPath);

				var versionFileName = await config.GetVersionFileName(fileName);
				var versionFilePath = fullPath.Replace(fileName, versionFileName);

				File.WriteAllText(versionFilePath, version);
			}
			catch (Exception e) {
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

		public async void GetRemoteVersion(string remoteSubPath, Action<string> onSuccess, Action<Exception> onFailure) {
			try {
				var fullPath = config.rootDir.AppendPath(remoteSubPath);
				var fileName = Path.GetFileName(fullPath);
				var versionFileName = await config.GetVersionFileName(fileName);
				var versionSubPath = remoteSubPath.Replace(fileName, versionFileName);

				var versionFileURL = await m_Downloader.GetURL(versionSubPath);
				var bytes = await m_Downloader.Download(versionFileURL);
				var version = Encoding.UTF8.GetString(bytes);
				onSuccess?.Invoke(version);
			}
			catch (Exception e) {
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
					onSuccess?.Invoke(false);
					return;
				}

				onSuccess?.Invoke(File.Exists(config.rootDir.AppendPath(localSubPath)));
			}
			catch (Exception e) {
				onFailure?.Invoke(e);
			}
		}
		#endregion

		// ================================================
		// LOCAL FILE
		// ================================================
		#region LOCAL_FILE
		// READ
		public UniTask<object[]> ReadLocalFile(string localSubPath) {
			var source = new UniTaskCompletionSource<object[]>();
			ReadLocalFile(localSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}
		
		public async void ReadLocalFile(string localSubPath, Action<object[]> onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = config.rootDir.AppendPath(localSubPath);
				var fileName = Path.GetFileName(filePath);
				var versionFileName = await config.GetVersionFileName(fileName);
				var versionFilePath = filePath.Replace(fileName, versionFileName);

				using(var versionFileStream = File.OpenRead(versionFilePath))
				using (var fileStream = File.OpenRead(filePath)) {
					var fileBuffer = new byte[fileStream.Length];
					var versionFileBuffer = new byte[versionFileStream.Length];

					var fileRead = fileStream.ReadAsync(fileBuffer, 0, fileBuffer.Length);
					var versionFileRead = versionFileStream.ReadAsync(versionFileBuffer, 0, versionFileBuffer.Length);

					await Task.WhenAll(fileRead, versionFileRead);
					var version = Encoding.UTF8.GetString(versionFileBuffer);

					versionFileStream.Close();
					fileStream.Close();
					onSuccess?.Invoke(new object[] { fileBuffer, version });
				}
			}
			catch (Exception e) {
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
				var filePath = config.rootDir.AppendPath(subPath);
				var fileName = Path.GetFileName(filePath);
				var fileDir = filePath.Replace(fileName, string.Empty);
				Directory.CreateDirectory(fileDir);

				var versionFileName = await config.GetVersionFileName(fileName);
				var versionFilePath = filePath.Replace(fileName, versionFileName);

				using (var fileStream = File.OpenWrite(filePath))
				using (var versionFileStream = File.OpenWrite(versionFilePath)) {
					var fileWrite = fileStream.WriteAsync(bytes, 0, bytes.Length);

					var versionBytes = Encoding.UTF8.GetBytes(version);
					var versionFileWrite = versionFileStream.WriteAsync(versionBytes, 0, versionBytes.Length);

					await Task.WhenAll(fileWrite, versionFileWrite);
					fileStream.Close();
					versionFileStream.Close();
					onSuccess?.Invoke();
				}
			}
			catch(Exception e) {
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

		public async void DeleteLocalFile(string localSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			try {
				var filePath = config.rootDir.AppendPath(localSubPath);
				var fileName = Path.GetFileName(filePath);
				var versionFileName = await config.GetVersionFileName(fileName);
				var versionFilePath = filePath.Replace(fileName, versionFileName);

				if (!File.Exists(filePath)) {
					onSuccess?.Invoke(false);
					return;
				}

				if (!File.Exists(versionFilePath)) {
					onSuccess?.Invoke(false);
					return;
				}

				var fileDeletion = FileX.DeleteAsync(filePath);
				var versionFileDeletion = FileX.DeleteAsync(versionFilePath);
				await Task.WhenAll(fileDeletion, versionFileDeletion);
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
		public UniTask<bool> Update(string commonSubPath) {
			return Update(commonSubPath, commonSubPath);
		}

		public UniTask<bool> Update(string localSubPath, string remoteSubPath) {
			var source = new UniTaskCompletionSource<bool>();
			Update(localSubPath, remoteSubPath,
				result => source.TrySetResult(result),
				error => source.TrySetException(error)
			);
			return source.Task;
		}

		public void Update(string commonSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			Update(commonSubPath, commonSubPath, onSuccess, onFailure);
		}

		public async void Update(string localSubPath, string remoteSubPath, Action<bool> onSuccess, Action<Exception> onFailure) {
			try {

				var isUpToDate = await IsUpToDate(localSubPath);
				if (isUpToDate) {
					onSuccess?.Invoke(false);
					return;
				}
				
				// Download the new file
				var url = await m_Downloader.GetURL(remoteSubPath);
				var bytes = await m_Downloader.Download(url);

				// Delete the existing files
				await DeleteLocalFile(localSubPath);

				// Write the new files
				var remoteVersion = await GetRemoteVersion(remoteSubPath);
				await WriteLocalFile(localSubPath, bytes, remoteVersion);
				onSuccess?.Invoke(true);
			}
			catch (Exception e) {
				onFailure?.Invoke(e);
			}
		}
		#endregion
	}
}
