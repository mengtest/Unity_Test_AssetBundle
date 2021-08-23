using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 
/// </summary>
namespace com.acorn.utils
{
    public class ABUtils : SingletonManager<ABUtils>
    {
        public T LoadResource<T>(string assetBundleName, string assetBundleGroupName,bool isDownloadPath) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetBundleGroupName))
            {
                return default(T);
            }

            //���ļ��л�ȡ
            AssetBundle assetbundle = AssetBundle.LoadFromFile(GetStreamingAssetsPath(isDownloadPath) + "/" + assetBundleGroupName);
            object obj = assetbundle.LoadAsset(assetBundleName, typeof(T));
            var one = obj as T;
            assetbundle.Unload(false);
            return one;
        }

        /// <summary>
        ///   //�ӷ��������ص�����
        /// </summary>
        /// <param name="AssetsHost">������·��</param>
        /// <param name="RootAssetsName">�������ļ�Ŀ¼·��</param>
        /// <param name="AssetName">������Դ����</param>
        /// <param name="saveLocalPath">���浽����·��,һ�����Application.persistentDataPath</param>
        /// <returns></returns>
        public IEnumerator DownLoadAssetsWithDependencies2Local(string AssetsHost, string RootAssetsName, string AssetName, OnDownloadFinish OnDownloadOver = null)
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

            string saveLocalPath = GetStreamingAssetsPath(true);
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
        /// ���ļ�ģ�ʹ���������
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="length"></param>
        void SaveAsset2LocalFile(string path, string name, byte[] info, int length)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
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
        }

        /// <summary>
        /// ɾ���ļ�
        /// </summary>
        /// <param name="path"></param>
        void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {
                Debug.Log("Catch exception At DeleteFile");
            }
        }

        public string GetStreamingAssetsPath(bool isDownloadPath)
        {
            string res;
#if UNITY_EDITOR
            res = Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID
            if (isDownloadPath)
            {
                res = Application.persistentDataPath + "/AssetBundle";
            }
            else
            {
                res = "jar:file://" + Application.dataPath + "!/assets";
            }
#elif UNITY_IPHONE
            res = Application.dataPath + "/Raw/";
#else
            res = string.Empty;
#endif
            return res;
        }

    }

    public delegate void OnDownloadFinish();
}