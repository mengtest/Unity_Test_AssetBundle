using UnityEngine;
using UnityEditor;

/// <summary>
/// ��������ǰ׺�Զ�������ͼƬ�ĸ�ʽ�Լ�TAG������
/// </summary>
public class ImageImporter : AssetPostprocessor
{
    /// <summary>
    /// ͼƬ����֮ǰ���ã�������ͼƬ�ĸ�ʽ��spritePackingTag��assetBundleName����Ϣ
    /// </summary>
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;
        string path = importer.assetPath;
        string[] pathArray = importer.assetPath.Split('/');
        if (pathArray.Length <= 2)
        {
            Debug.LogError("��ȡ·����ʧ��");
            return;
        }
        string imageName = pathArray[pathArray.Length - 1];
        string packTag = pathArray[pathArray.Length - 2];

        if (imageName.StartsWith("UI_"))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            //����spritePackingTag
            importer.spritePackingTag = packTag;
            //����assetBundleName
            importer.assetBundleName = packTag;
        }
    }
}