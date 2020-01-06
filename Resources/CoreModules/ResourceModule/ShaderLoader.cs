﻿using System.Collections;
using UnityEngine;

namespace AD
{
    /// <summary>
    /// Shader加载器
    /// </summary>
    public class ShaderLoader : AbstractResourceLoader
    {
        public delegate void ShaderLoaderDelegate(bool isOk, Shader shader);

        public Shader ShaderAsset
        {
            get { return ResultObject as Shader; }
        }

        public static ShaderLoader Load(string path, ShaderLoaderDelegate callback = null)
        {
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk, obj as Shader);
            }
            return AutoNew<ShaderLoader>(path, newCallback);
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);
            ResourceModule.Ins.StartCoroutine(CoLoadShader());
        }

        private IEnumerator CoLoadShader()
        {
            var loader = AssetBundleLoader.Load(Url);
            while (!loader.IsCompleted)
            {
                Progress = loader.Progress;
                yield return null;
            }

            var shader = loader.Bundle.LoadAsset<Shader>("");
            Debug.Assert(shader);

            Desc = shader.name;

            if (Application.isEditor)
                ResourceLoadedAssetDebugger.Create("Shader", Url, shader);

            loader.Release();

            OnFinish(shader);
        }


        protected override void DoDispose()
        {
            base.DoDispose();

            GameObject.Destroy(ShaderAsset);
        }
    }
}