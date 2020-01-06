﻿using System.Collections;
using System.IO;
using UnityEngine;

#if UNITY_5 || UNITY_2017_1_OR_NEWER

namespace AD
{
    public class SceneLoader : AbstractResourceLoader
    {
        private AssetFileLoader _assetFileBridge;
        private LoaderMode _mode;
        private string _url;
        private string _sceneName;

        private string _loadedSceneName;
        public override float Progress
        {
            get { return _assetFileBridge.Progress; }
        }

        public string SceneName
        {
            get { return _sceneName; }
        }

        public static bool isLoadSceneAdditive = false;
        private static SceneLoader preSceneLoader;

        public static void UnloadPreScene()
        {
            if (preSceneLoader != null)
            {
//                preSceneLoader.FullClear = true;
//                preSceneLoader._assetFileBridge.FullClear = true;
//                preSceneLoader.Release();
                preSceneLoader._loadedSceneName = string.Empty;
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(preSceneLoader.SceneName);
                preSceneLoader = null;
            }
        }

        public static SceneLoader Load(string url, System.Action<bool> callback = null,
            LoaderMode mode = LoaderMode.Async)
        {
            UnloadPreScene();
            LoaderDelgate newCallback = null;
            if (callback != null)
            {
                newCallback = (isOk, obj) => callback(isOk);
            }
            var loader = AutoNew<SceneLoader>(url, newCallback, true, mode);
            preSceneLoader = loader;
            return preSceneLoader;
        }

        protected override void Init(string url, params object[] args)
        {
            base.Init(url, args);

            _mode = (LoaderMode)args[0];
            _url = url;
            _sceneName = Path.GetFileNameWithoutExtension(_url);
            ResourceModule.Ins.StartCoroutine(Start());
        }

        IEnumerator Start()
        {
            _assetFileBridge = AssetFileLoader.Load(_url, (bool isOk, UnityEngine.Object obj) => { },
                _mode);

            while (!_assetFileBridge.IsCompleted)
            {
                yield return null;
            }
            if (_assetFileBridge.IsError)
            {
                Debugger.Error("[SceneLoader]Load SceneLoader Failed(Error) when Finished: {0}", _url);
                _assetFileBridge.Release();
                OnFinish(null);
                yield break;
            }

            // load scene
            Debugger.Assert(_assetFileBridge.Asset);
            _loadedSceneName = _sceneName;
            if (_mode == LoaderMode.Sync)
                UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName,
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);

            else
            {
                var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                while (!op.isDone)
                {
                    yield return null;
                }
            }

            if (Application.isEditor)
                ResourceModule.Ins.StartCoroutine(EditorLoadSceneBugFix(null));

            OnFinish(_assetFileBridge);

        }


        /// <summary>
        ///     编辑器模式下，场景加载完毕，刷新所有material的shader确保显示正确， unity b.u.g
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private static IEnumerator EditorLoadSceneBugFix(AsyncOperation op)
        {
            if (op != null)
            {
                while (!op.isDone)
                    yield return null;
            }
            yield return null; // one more frame

            RefreshAllMaterialsShaders();
        }

        /// <summary>
        /// 编辑器模式下，对全部GameObject刷新一下Material
        /// </summary>
        private static void RefreshAllMaterialsShaders()
        {
            foreach (var renderer in GameObject.FindObjectsOfType<Renderer>())
            {
                AssetFileLoader.RefreshMaterialsShaders(renderer);
            }
        }


        protected override void DoDispose()
        {
            base.DoDispose();
            _assetFileBridge.Release();
            if(_loadedSceneName == _sceneName)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_sceneName);
            }
        }
        protected override void OnReadyDisposed()
        {
            base.OnReadyDisposed();
            _assetFileBridge.ForceDispose();
        }
    }
}

#endif