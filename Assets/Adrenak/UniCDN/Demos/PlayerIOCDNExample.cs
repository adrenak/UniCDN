using UnityEngine;
using System;
using Adrenak.UniCDN;

public class PlayerIOCDNExample : MonoBehaviour {
	CDNCache m_Cache = new CDNCache();

	async void Start() {
		var downloader = DownloaderBuilder.Build(Provider.PlayerIO);
		var config = new CDNCache.Config {
			rootDir = Application.persistentDataPath + "/PlayerIOCDNExample/",
			versionFileNomenclatureMethod = (fileName, onSuccess, onFailure) => 
				onSuccess?.Invoke(fileName + "_version.txt")
		};

		await m_Cache.Init(config, downloader);
		Debug.Log("CDN Cache initialized");
	}

	[ContextMenu("Update Test")]
	void UpdateFileTest() {
		UpdateFile("/files/largefile");
	}

	public async void UpdateFile(string fileName) {
		try {
			var result = await m_Cache.Update(fileName);
			if (result) {
				var version = await m_Cache.GetLocalVersion(fileName);
				Debug.Log(fileName + " updated to latest version" + version);
			}
			else
				Debug.Log(fileName + " is already up to date");
		}
		catch(Exception e) {
			Debug.Log(e);
		}
	}

	[ContextMenu("GetLocalVersion Test")]
	void GetLocalVersionTest() {
		GetLocalVersion("/files/largefile");
	}

	public async void GetLocalVersion(string fileName) {
		try {
			var version = await m_Cache.GetLocalVersion(fileName);
			Debug.Log(fileName + " version: " + version);
		}
		catch(Exception e) {
			Debug.LogError(e);
		}
	}

	[ContextMenu("GetRemoteVersion Test")]
	void GetRemoteVersionTest() {
		GetRemoteVersion("/files/largefile");
	}

	public async void GetRemoteVersion(string fileName) {
		try {
			var version = await m_Cache.GetRemoteVersion(fileName);
			Debug.Log(fileName + " version: " + version);
		}
		catch(Exception e) {
			Debug.LogError(e);
		}
	}
}
