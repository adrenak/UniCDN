## UniCDN
A CDN adapter for Unity. _Under development_

## Intro
- Allows you to use content delivery networks using `async/await`  
- Provides `CDNCache`, a module that manages file versions and can be used to update files only when the CDN has a new one  

## Documentation
### Downloaders
- `Provider` enumerates all the CDN provides available to UniCDN. Add new values as required.
- `IDownloader` acts as the interface for classes from downloading from different CDN providers
- `DownloaderFactory` creates concrete implementations of `IDownloader`
- The following methods are provided by `IDownloader`, and hence, any implementation of the interface:
    - `Init` for initializing any SDKs, authenticating connections, obtaining tokens. In other words, whatever a provider needs before it can be used.
    - `GetURL` for obtaining the CDN download URL using a file name
    - `Download` to start the download process using a URL. This method is a simple download web request using `RestSharp`, so any URL can be used regardless of it being associated with a CDN or not
    - ___Note___ The above methods are also available as `UniTask` for async/await style use. `UniRx.Async` (or `UniTask`) library is used for this.

### CDNCache
`CDNCache` can be used to keep track of file versions. This is used to make sure you only download a file if required, keeping the CDN traffic low.  
Currently, `CDNCache` assumes that next to any file on the CDN, another file containing the version of the file is included.  

The `CDNCache.Config` allows you to set the require parameters for this feature to operate. Namely:  
* `rootDir` the directory on the local disk where files and folders will be created and tracked using version files  
* `versionFileNomenclatureMethod` for setting the naming convention. For example, the version file name could be `abc.zyx_version.txt` where `abc.xyz` is the name of the file that it is associated with.  

`CDNCache` provides the following methods:  
- `Init` where it requires the `Config` and the concrete `IDownloader` instance it should use  
- `GetLocalVersion` to fetch the version of a file present locally  
- `SetLocalVersion` to assign the version for a local file  
- `GetRemoteVersion` to fetch the version of a file on the CDN  
- `IsUpToDate` to check if the version of a local file is the same as the one on the CDN  
- `ReadLocalFile` to retreive the `byte[]` of a local file as well as the version associated with it in a `ReadLocalFileResult` object  
- `WriteLocalFile` to write a `byte` array to a local file as well as the version  
- `DeleteLocalFile` to delete a file as well as the version file associated with it  
- `UpdateFile` to download and write a file and update the version if the CDN has a different version  
- ___Note___ `CDNCache` provides an `enableLogging` option  
  
### Adding new providers
Presently the project supports PlayerIO GameFS CDN. If you want to add your own, follow these steps:
- Add a new value in `Provider` enum
- Implement `IDownloader` for your platform of choice

If you have trouble adding support for a new service, or UniCDN does not provide the means to configure or use it, please open an issue.

## Contact  
[@github](https://www.github.com/adrenak)  
[@www](http://www.vatsalambastha.com)  