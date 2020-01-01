﻿using System.Collections;
using System.IO;

using UnityEngine;

namespace AD
{

    /// <summary>
    /// 读取字节，调用WWW, 会自动识别Product/Bundles/Platform目录和StreamingAssets路径
    /// </summary>
    public class HotBytesLoader : AbstractResourceLoader
    {
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// 异步模式中使用了WWWLoader
        /// </summary>
        private WWWLoader _wwwLoader;

        private LoaderMode _loaderMode;

        public static HotBytesLoader Load(string path, LoaderMode loaderMode)
        {
            var newLoader = AutoNew<HotBytesLoader>(path, null, false, loaderMode);
            return newLoader;
        }

        private string _fullUrl;

        private IEnumerator CoLoad(string url)
        {
            var getResPathType = ResPath.GetResourceFullPath(url, _loaderMode == LoaderMode.Async, out _fullUrl);
            if (getResPathType == ResourceModule.GetResourceFullPathType.Invalid)
            {
                if (Debug.isDebugBuild)
                    Log.Error("[HotBytesLoader]Error Path: {0}", url);
                OnFinish(null);
                yield break;
            }

            if (_loaderMode == LoaderMode.Sync)
            {
                // 存在应用内的，StreamingAssets内的，同步读取；否则去PersitentDataPath
                if (getResPathType == ResourceModule.GetResourceFullPathType.InApp)
                {
                    if (Application.isEditor) // Editor mode : 读取Product配置目录
                    {
                        var loadSyncPath = Path.Combine(ResPath.ProductPathWithoutFileProtocol, url);
                        Bytes = ResourceModule.ReadAllBytes(loadSyncPath);
                    }
                    else // product mode: read streamingAssetsPath
                    {
                        Bytes = ResourceModule.LoadSyncFromStreamingAssets(url);
                    }
                }
                else
                {
                    Bytes = ResourceModule.ReadAllBytes(_fullUrl);
                }
            }
            else
            {

                _wwwLoader = WWWLoader.Load(_fullUrl);
                while (!_wwwLoader.IsCompleted)
                {
                    Progress = _wwwLoader.Progress;
                    yield return null;
                }

                if (!_wwwLoader.IsSuccess)
                {
                    //if (AssetBundlerLoaderErrorEvent != null)
                    //{
                    //    AssetBundlerLoaderErrorEvent(this);
                    //}
                    Log.Error("[HotBytesLoader]Error Load WWW: {0}", url);
                    OnFinish(null);
                    yield break;
                }

                Bytes = _wwwLoader.Www.downloadHandler.data;

            }

            OnFinish(Bytes);
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            if (_wwwLoader != null)
            {
                _wwwLoader.Release();
            }
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            _loaderMode = (LoaderMode)args[0];
            ResourceModule.Ins.StartCoroutine(CoLoad(url));

        }

    }

}
