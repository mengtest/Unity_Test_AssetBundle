using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;

public delegate void dlg_OnAssetBundleDownLoadOver();
/// <summary>
/// ����AssetBundle
/// </summary>
public class LoadAssetBundle : SingletonManager<LoadAssetBundle>
{
    public override void Init()
    {

    }

    //��ͬƽ̨��StreamingAssets��·������
    public static readonly string PathURL =
#if UNITY_ANDROID
        "jar:file://" + Application.dataPath + "!/assets/";
#elif UNITY_IPHONE
        Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        "file://" + Application.dataPath + "/StreamingAssets/";
#else
        string.Empty;  
#endif

    //5.0�汾���ʱ��ѡ����Ҫ����Ķ���Ȼ���������½�����,ͬ��/���ö༯Ŀ¼,����Ŀ��Ǻ�׺(��׺����Ҫ)
    //���ʱ���Ŀ���ļ���,����Ŀ���ļ�������Ϊ"WJJ",��ô������"WJJ"��"WJJ.manifest"�����ļ�
    //����WJJ.manifest�ļ�û����,ֻ����������,WJJ��һ��assetbundle��,��������������ļ��е�������Ϣ
    //�����ȼ����������,Ȼ���ȡ��������ϵ���𲽼���
    //��һ�����ز����浽Application.persistentDataPath
    //ע����GetDirectDependencies�ݹ�,��Ҫ��GetAllDependencies,��Ϊ�Ѿ��������Ӷ����ֻ��������,�ظ�������
    //���÷�ֱ�ӻ�ȡ��Ҫ��GetAllDependencies,Ȼ�������    

    /// <summary>
    /// ������Դ�����ذ�������������
    /// </summary>
    /// <param name="AssetsHost">��Ŀ¼��ַ</param>
    /// <param name="RootAssetsName"></param>
    /// <param name="AssetName"></param>
    /// <param name="savePath"></param>
    public void DownLoadAssets2LocalWithDependencies(string AssetsHost, string RootAssetsName, string AssetName, string savePath, dlg_OnAssetBundleDownLoadOver OnDownloadOver = null)
    {
        //StartCoroutine(DownLoadAssetsWithDependencies2Local(AssetsHost, RootAssetsName, AssetName, savePath, OnDownloadOver));
    }

    /// <summary>
    ///   //�ӷ��������ص�����
    /// </summary>
    /// <param name="AssetsHost">������·��</param>
    /// <param name="RootAssetsName">�������ļ�Ŀ¼·��</param>
    /// <param name="AssetName">������Դ����</param>
    /// <param name="saveLocalPath">���浽����·��,һ�����Application.persistentDataPath</param>
    /// <returns></returns>
    IEnumerator DownLoadAssetsWithDependencies2Local(string AssetsHost, string RootAssetsName, string AssetName, string saveLocalPath, dlg_OnAssetBundleDownLoadOver OnDownloadOver = null)
    {
        WWW ServerManifestWWW = null;        //���ڴ洢������ϵ�� AssetBundle
        AssetBundle LocalManifestAssetBundle = null;    //���ڴ洢������ϵ�� AssetBundle
        AssetBundleManifest assetBundleManifestServer = null;  //������ �ܵ�������ϵ    
        AssetBundleManifest assetBundleManifestLocal = null;   //���� �ܵ�������ϵ

        if (RootAssetsName != "")    //��������Ϊ�յ�ʱ��ȥ������������
        {
            ServerManifestWWW = new WWW(AssetsHost + "/" + RootAssetsName);

            Debug.Log("___��ǰ�����������ļ�~\n");

            yield return ServerManifestWWW;
            if (ServerManifestWWW.isDone)
            {
                //�����ܵ������ļ�
                assetBundleManifestServer = ServerManifestWWW.assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                Debug.Log("___��ǰ�����������ļ�~\n");
            }
            else
            {
                throw new Exception("�������ļ�����ʧ��~~~\n");
            }
        }

        //��ȡ��Ҫ�������������������
        string[] AllDependencies = new string[0];
        if (assetBundleManifestServer != null)
        {
            //�������ƻ�ȡ������
            AllDependencies = assetBundleManifestServer.GetAllDependencies(AssetName);
        }

        //���ض��� ����ȡÿ����Դ��Hashֵ
        Dictionary<string, Hash128> dicDownloadInfos = new Dictionary<string, Hash128>();
        for (int i = AllDependencies.Length - 1; i >= 0; i--)
        {
            dicDownloadInfos.Add(AllDependencies[i], assetBundleManifestServer.GetAssetBundleHash(AllDependencies[i]));
        }
        dicDownloadInfos.Add(AssetName, assetBundleManifestServer.GetAssetBundleHash(AssetName));
        if (assetBundleManifestServer != null)   //�����ļ���Ϊ�յĻ����������ļ�
        {
            Debug.Log("Hash:" + assetBundleManifestServer.GetHashCode());
            dicDownloadInfos.Add(RootAssetsName, new Hash128(0, 0, 0, 0));
        }

        //ж�ص�,�޷�ͬʱ���ض�������ļ�
        ServerManifestWWW.assetBundle.Unload(true);

        if (File.Exists(saveLocalPath + "/" + RootAssetsName))
        {
            LocalManifestAssetBundle = AssetBundle.LoadFromFile(saveLocalPath + "/" + RootAssetsName);
            assetBundleManifestLocal = LocalManifestAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        foreach (var item in dicDownloadInfos)
        {
            if (!CheckLocalFileNeedUpdate(item.Key, item.Value, RootAssetsName, saveLocalPath, assetBundleManifestLocal))
            {
                Debug.Log("��������:" + item.Key);
                continue;
            }
            else
            {
                DeleteFile(saveLocalPath + "/" + item.Key);
            }

            //ֱ�Ӽ������е�������ͺ���
            WWW wwwAsset = new WWW(AssetsHost + "/" + item.Key);
            //��ȡ���ؽ���
            while (!wwwAsset.isDone)
            {
                Debug.Log(string.Format("���� {0} : {1:N1}%", item.Key, (wwwAsset.progress * 100)));
                //yield return new WaitForSeconds(0.2f);
            }
            //���浽����
            SaveAsset2LocalFile(saveLocalPath, item.Key, wwwAsset.bytes, wwwAsset.bytes.Length);

        }

        if (LocalManifestAssetBundle != null)
        {
            LocalManifestAssetBundle.Unload(true);
        }

        if (OnDownloadOver != null)
        {
            OnDownloadOver();
        }
    }

    /// <summary>
    /// ��Ȿ���ļ��Ƿ�����Ѿ��Ƿ�������
    /// </summary>
    /// <param name="AssetName"></param>
    /// <param name="RootAssetsName"></param>
    /// <param name="localPath"></param>
    /// <param name="serverAssetManifestfest"></param>
    /// <param name="CheckCount"></param>
    /// <returns></returns>
    bool CheckLocalFileNeedUpdate(string AssetName, Hash128 hash128Server, string RootAssetsName, string localPath, AssetBundleManifest assetBundleManifestLocal)
    {
        Hash128 hash128Local;
        bool isNeedUpdate = false;
        if (!File.Exists(localPath + "/" + AssetName))
        {
            return true;   //���ز�����,��һ������
        }

        if (!File.Exists(localPath + "/" + RootAssetsName))   //������������Ϣ������ʱ,����
        {
            isNeedUpdate = true;
        }
        else   //�ܵ�������Ϣ�������ļ��Ѵ���  �Աȱ��غͷ����������ļ���Hashֵ
        {
            if (hash128Server == new Hash128(0, 0, 0, 0))
            {
                return true;  //��֤ÿ�ζ������������ļ�
            }
            hash128Local = assetBundleManifestLocal.GetAssetBundleHash(AssetName);
            //�Աȱ�����������ϵ�AssetBundleHash  �汾��һ�¾�����
            if (hash128Local != hash128Server)
            {
                isNeedUpdate = true;
            }
        }
        return isNeedUpdate;
    }

    /// <summary>
    /// �ǵݹ�ʽ����ָ��AB,������������,������Ŀ��GameObject
    /// </summary>
    /// <param name="RootAssetsName"></param>
    /// <param name="AssetName"></param>
    /// <param name="LocalPath"></param>
    public GameObject GetLoadAssetFromLocalFile(string RootAssetsName, string AssetName, string PrefabName, string LocalPath)
    {
        AssetBundle assetBundle = AssetBundle.LoadFromFile(LocalPath + "/" + RootAssetsName);
        AssetBundleManifest assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string[] AllDependencies = assetBundleManifest.GetAllDependencies(AssetName);

        for (int i = AllDependencies.Length - 1; i >= 0; i--)
        {
            AssetBundle assetBundleDependencies = AssetBundle.LoadFromFile(LocalPath + "/" + AllDependencies[i]);
            assetBundleDependencies.LoadAllAssets();
        }

        AssetBundle assetTarget = AssetBundle.LoadFromFile(LocalPath + "/" + AssetName);
        return assetTarget.LoadAsset<GameObject>(PrefabName);
    }

    /// <summary>
    /// �ݹ���ر�������������
    /// </summary>
    /// <param name="RootAssetsName"></param>
    /// <param name="AssetName"></param>
    /// <param name="LocalPath"></param>
    AssetBundleManifest assetBundleManifestLocalLoad;   //�ݹ����ʱ����
    public void RecursionLoadAssetFromLocalFile(string RootAssetsName, string AssetName, string LocalPath, int RecursionCounter)
    {
        if (RecursionCounter++ == 0)
        {
            //���ر���Manifest��ȡ������
            assetBundleManifestLocalLoad = AssetBundle.LoadFromFile(LocalPath + "/" + RootAssetsName).LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        //��ǰAssetName����������
        string[] AllDependencies = assetBundleManifestLocalLoad.GetDirectDependencies(AssetName);

        for (int i = 0; i < AllDependencies.Length; i++)
        {
            RecursionLoadAssetFromLocalFile(RootAssetsName, AllDependencies[i], LocalPath, RecursionCounter);
        }

        AssetBundle assetBundle = AssetBundle.LoadFromFile(LocalPath + "/" + AssetName);
        assetBundle.LoadAllAssets();
    }

    /// <summary>
    /// ���ļ�ģ�ʹ���������
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="info"></param>
    /// <param name="length"></param>
    void SaveAsset2LocalFile(string path, string name, byte[] info, int length)
    {
        Stream sw = null;
        FileInfo fileInfo = new FileInfo(path + "/" + name);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }

        //������ļ��������򴴽�
        sw = fileInfo.Create();
        //д��
        sw.Write(info, 0, length);

        sw.Flush();
        //�ر���
        sw.Close();
        //������
        sw.Dispose();

        Debug.Log(name + "�ɹ����浽����~");
    }

    /// <summary>
    /// ɾ���ļ�
    /// </summary>
    /// <param name="path"></param>
    void DeleteFile(string path)
    {
        File.Delete(path);
    }
}