/***************************************************************************************
* Name: AssetBundleMgr.cs
* Function:AssetBundle���ع���;
* 
* Version     Date                Name                            Change Content
* ����������������������������������������������������������������������������������������������������������������������������������������������������������������
* V1.0.0    20170905    http://blog.csdn.net/husheng0
* 
* Copyright (c). All rights reserved.
* 
***************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AssetBundleMgr : SingletonManager<AssetBundleMgr>
{

    #region Field

    /// <summary>
    /// ���س�����AssetBundle����;
    /// </summary>
    private Dictionary<string, AssetBundle> assetBundleCache = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// ���س�����AssetBundle���ü���;
    /// </summary>
    private Dictionary<string, int> assetBundleReference = new Dictionary<string, int>();

    /// <summary>
    /// ������ϵAssetBundle;
    /// </summary>
    private AssetBundle mainAssetBundle;

    /// <summary>
    /// AssetBundleManifest
    /// </summary>
    private AssetBundleManifest manifest;

    /// <summary>
    /// ������ϵAssetBundle;
    /// </summary>
    private AssetBundle MainAssetBundle
    {
        get
        {
            if (null == mainAssetBundle)
            {
                mainAssetBundle = AssetBundle.LoadFromFile(FilePathUtil.assetBundlePath + "AssetBundle");
            }
            if (mainAssetBundle == null)
            {
                Debug.LogError(string.Format("[AssetBundleMgr]Load AssetBundle {0} failure!", FilePathUtil.assetBundlePath + "AssetBundle"));
            }
            return mainAssetBundle;
        }
    }

    /// <summary>
    /// AssetBundleManifest
    /// </summary>
    private AssetBundleManifest Manifest
    {
        get
        {
            if (null == manifest && MainAssetBundle != null)
            {
                manifest = MainAssetBundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
            }
            if (manifest == null)
            {
                Debug.LogError(string.Format("[AssetBundleMgr]Load AssetBundleManifest {0} failure!", FilePathUtil.assetBundlePath + "AssetBundle"));
            }
            return manifest;
        }
    }

    /// <summary>
    /// �����첽�����е�AssetBundle;
    /// </summary>
    public HashSet<string> assetBundleLoading = new HashSet<string>();

    #endregion

    #region Function

    /// <summary>
    /// AssetBundle�Ƿ����ڼ���;
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool IsAssetBundleLoading(string path)
    {
        return assetBundleLoading.Contains(path);
    }

    #endregion

    #region AssetBundle Load

    /// <summary>
    /// AssetBundleͬ������LoadFromFile;
    /// </summary>
    /// <param name="path">AssetBundle�ļ�·��</param>
    /// <returns>AssetBundle</returns>
    private AssetBundle LoadSingleAssetBundleSync(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        AssetBundle assetBundle = null;
        if (!assetBundleCache.ContainsKey(path))
        {
            try
            {
                assetBundle = AssetBundle.LoadFromFile(path);
                if (null == assetBundle)
                {
                    Debug.LogError(string.Format("[AssetBundleMgr]Load AssetBundle {0} failure!", path));
                }
                else
                {
                    assetBundleCache[path] = assetBundle;
                    assetBundleReference[path] = 1;
                    Debug.Log(string.Format("[AssetBundleMgr]Load AssetBundle {0} Success!", path));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
        else
        {
            assetBundle = assetBundleCache[path];
            assetBundleReference[path]++;
        }
        return assetBundle;
    }

    /// <summary>
    /// AssetBundle�첽����LoadFromFileAsync,www�첽�������Ĵ���LoadFromFileAsync;
    /// </summary>
    /// <param name="path">��Դ·��</param>
    /// <param name="action">AssetBundle�ص�</param>
    /// <param name="progress">progress�ص�</param>
    /// <returns></returns>
    private IEnumerator LoadSingleAssetBundleAsync(string path, Action<AssetBundle> action, Action<float> progress)
    {
        if (string.IsNullOrEmpty(path)) yield break;

        AssetBundle assetBundle = null;
        while (IsAssetBundleLoading(path))
        {
            yield return null;
        }
        if (!assetBundleCache.ContainsKey(path))
        {
            //��ʼ����;
            assetBundleLoading.Add(path);
            AssetBundleCreateRequest assetBundleReq = AssetBundle.LoadFromFileAsync(path);
            //���ؽ���;
            while (assetBundleReq.progress < 0.99)
            {
                if (null != progress)
                    progress(assetBundleReq.progress);
                yield return null;
            }

            while (!assetBundleReq.isDone)
            {
                yield return null;
            }
            assetBundle = assetBundleReq.assetBundle;
            if (assetBundle == null)
            {
                Debug.Log(string.Format("[AssetBundleMgr]Load AssetBundle {0} failure!", path));
            }
            else
            {
                assetBundleCache[path] = assetBundle;
                assetBundleReference[path] = 1;
                Debug.Log(string.Format("[AssetBundleMgr]Load AssetBundle {0} Success!", path));
            }
            //�������;
            assetBundleLoading.Remove(path);
        }
        else
        {
            assetBundle = assetBundleCache[path];
            assetBundleReference[path]++;
        }
        if (action != null) action(assetBundle);
    }

    /// <summary>
    /// AssetBundleͬ������;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    /// <returns>AssetBundle</returns>
    public AssetBundle LoadAssetBundleSync(AssetType type, string assetName)
    {
        if (type == AssetType.Non || string.IsNullOrEmpty(assetName)) return null;

        string assetBundlePath = FilePathUtil.GetAssetBundlePath(type, assetName);
        if (assetBundlePath == null) return null;
        string assetBundleName = FilePathUtil.GetAssetBundleFileName(type, assetName);

        AssetBundle assetBundle = LoadSingleAssetBundleSync(assetBundlePath);
        if (assetBundle == null) return null;

        //����AssetBundleName;
        string[] DependentAssetBundle = Manifest.GetAllDependencies(assetBundleName);
        foreach (string tempAssetBundle in DependentAssetBundle)
        {
            if (tempAssetBundle == FilePathUtil.GetAssetBundleFileName(AssetType.Shader, "Shader")) continue;
            string tempPtah = FilePathUtil.assetBundlePath + tempAssetBundle;
            LoadSingleAssetBundleSync(tempPtah);
        }
        return assetBundle;
    }

    /// <summary>
    /// AssetBundle�첽����;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    /// <param name="action">AssetBundle�ص�</param>
    /// <param name="progress">progress�ص�</param>
    /// <returns></returns>
    public IEnumerator LoadAssetBundleAsync(AssetType type, string assetName, Action<AssetBundle> action, Action<float> progress)
    {
        if (type == AssetType.Non || string.IsNullOrEmpty(assetName)) yield break;
        string assetBundlePath = FilePathUtil.GetAssetBundlePath(type, assetName);
        if (assetBundlePath == null) yield break;
        string assetBundleName = FilePathUtil.GetAssetBundleFileName(type, assetName);
        //�ȼ���������AssetBundle;
        string[] DependentAssetBundle = Manifest.GetAllDependencies(assetBundleName);
        foreach (string tempAssetBundle in DependentAssetBundle)
        {
            if (tempAssetBundle == FilePathUtil.GetAssetBundleFileName(AssetType.Shader, "Shader")) continue;
            string tempPtah = FilePathUtil.assetBundlePath + tempAssetBundle;
            IEnumerator itor = LoadSingleAssetBundleAsync(tempPtah, null, null);
            while (itor.MoveNext())
            {
                yield return null;
            }
        }
        //����Ŀ��AssetBundle;
        IEnumerator itorTarget = LoadSingleAssetBundleAsync(assetBundlePath, action, progress);
        while (itorTarget.MoveNext())
        {
            yield return null;
        }
    }

    /// <summary>
    /// ����Shader AssetBundle;
    /// </summary>
    /// <returns>AssetBundle</returns>
    public AssetBundle LoadShaderAssetBundle()
    {
        string path = FilePathUtil.GetAssetBundlePath(AssetType.Shader, "Shader");
        return LoadSingleAssetBundleSync(path);
    }

    #endregion

    #region AssetBundle Unload

    /// <summary>
    /// ж��AssetBundle��Դ;
    /// </summary>
    /// <param name="path">��Դ·��</param>
    /// <param name="flag">true or false</param>
    private void UnloadAsset(string path, bool flag)
    {
        int Count = 0;
        if (assetBundleReference.TryGetValue(path, out Count))
        {
            Count--;
            if (Count == 0)
            {
                assetBundleReference.Remove(path);
                AssetBundle bundle = assetBundleCache[path];
                if (bundle != null) bundle.Unload(flag);
                assetBundleCache.Remove(path);
                Debug.Log(string.Format("[AssetBundleMgr]Unload {0} AssetBundle {0} Success!", flag, path));
            }
            else
            {
                assetBundleReference[path] = Count;
            }
        }
    }

    /// <summary>
    /// ͨ����ԴAssetBundleж�ط���[Unload(true)];
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    public void UnloadAsset(AssetType type, string assetName)
    {
        if (type == AssetType.Non || type == AssetType.Shader || type == AssetType.Scripts || string.IsNullOrEmpty(assetName))
            return;

        string assetBundleName = FilePathUtil.GetAssetBundleFileName(type, assetName);

        string[] DependentAssetBundle = Manifest.GetAllDependencies(assetBundleName);
        foreach (string tempAssetBundle in DependentAssetBundle)
        {
            if (tempAssetBundle == FilePathUtil.GetAssetBundleFileName(AssetType.Shader, "Shader")) continue;
            string tempPtah = FilePathUtil.assetBundlePath + tempAssetBundle;
            UnloadAsset(tempPtah, true);
        }
        string assetBundlePath = FilePathUtil.GetAssetBundlePath(type, assetName);
        if (assetBundlePath != null)
        {
            UnloadAsset(assetBundlePath, true);
        }
    }

    /// <summary>
    /// AssetBundle ����ж�ط���[Unload(false)],ʹ����ԴΪһ���ʼ����ȫ�ֱ��治�����ٵ���Դ,��:Shader;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    public void UnloadMirroring(AssetType type, string assetName)
    {
        if (type == AssetType.Non || type == AssetType.Scripts || string.IsNullOrEmpty(assetName))
            return;
        string assetBundlePath = FilePathUtil.GetAssetBundlePath(type, assetName);
        if (assetBundlePath != null)
        {
            UnloadAsset(assetBundlePath, false);
        }
    }

    #endregion

}