/***************************************************************************************
* Name: FilePathUtil.cs
* Function:·��������;
* 
* Version     Date                Name                            Change Content
* ����������������������������������������������������������������������������������������������������������������������������������������������������������������
* V1.0.0    20170901    http://blog.csdn.net/husheng0
* 
* Copyright (c). All rights reserved.
* 
***************************************************************************************/

using UnityEngine;
using System.Collections;

public static class FilePathUtil
{
    /// <summary>
    /// AssetBundle���·��;
    /// </summary>
    public static string assetBundlePath =

#if UNITY_IOS && !UNITY_EDITOR    //unity5.x UNITY_IPHONE����UNITY_IOS
	Application.persistentDataPath;
#elif UNITY_ANDROID && !UNITY_EDITOR
    Application.persistentDataPath;
#else
    "Assets/../AssetBundle/";
#endif

    /// <summary>
    /// ��Ҫ�������Դ���ڵ�Ŀ¼;
    /// </summary>
    public static string resPath = "Assets/AssetBundleSrc/";

    /// <summary>
    /// ��·���µ���Դ�������,��Ҫ��Ϊ�˷���ʹ����Դ,��ͼ��,����,������ı�����ͼ�ȵ�;
    /// </summary>
    public static string singleResPath = "Assets/AssetBundleSrc/SingleAssetBundleSrc";

    /// <summary>
    /// ��ȡAssetBundle�ļ�������;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    /// <returns>AssetBundle��Դ����</returns>
    public static string GetAssetBundleFileName(AssetType type, string assetName)
    {
        string assetBundleName = null;

        if (type == AssetType.Non || string.IsNullOrEmpty(assetName)) return assetBundleName;
        //AssetBundle�����ֲ�֧�ִ�д;
        //AssetBundle���������ʽΪ[assetType.assetName.assetbundle],����ʱͬ����Դ������������ͬ,һ��ͬһ�ļ����²����ظ�,ÿ��
        //�ļ����µ���Դ��������ͬ��ǰ׺,��ͬ�ļ�����,��Դǰ׺��ͬ;
        assetBundleName = (type.ToString() + "." + assetName + ".assetbundle").ToLower();
        return assetBundleName;
    }

    /// <summary>
    /// ��ȡAssetBundle�ļ�����·��;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    /// <returns>AssetBundle��Դ·��</returns>
    public static string GetAssetBundlePath(AssetType type, string assetName)
    {
        string assetBundleName = GetAssetBundleFileName(type, assetName);
        if (string.IsNullOrEmpty(assetBundleName)) return null;
        return assetBundlePath + assetBundleName;
    }

    /// <summary>
    /// ��ȡResource�ļ�����·��;
    /// </summary>
    /// <param name="type">��Դ����</param>
    /// <param name="assetName">��Դ����</param>
    /// <returns>Resource��Դ·��;</returns>
    public static string GetResourcePath(AssetType type, string assetName)
    {
        if (type == AssetType.Non || type == AssetType.Scripts || string.IsNullOrEmpty(assetName)) return null;
        string assetPath = null;
        switch (type)
        {
            case AssetType.Prefab: assetPath = "Prefab/"; break;
            default:
                assetPath = type.ToString() + "/";
                break;
        }
        assetPath = assetPath + assetName;
        return assetPath;
    }
}