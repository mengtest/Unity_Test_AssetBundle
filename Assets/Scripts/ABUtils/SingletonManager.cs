/***************************************************************************************
* Name: SingletonManager.cs
* Function:����;
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

/// <summary>
/// �����������ʵ�֣����̳�Monobehaviour
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonManager<T> where T : class, new()
{

    private static T instance = null;

    public static T Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new T(); //���ù��캯��;
            }
            return instance;
        }
    }
    /// <summary>
    /// SingletonManager���캯��
    /// </summary>
    protected SingletonManager()
    {
        if (null != instance)
            Debug.Log("This " + (typeof(T)).ToString() + " Singleton Instance is not null��");
        Init();
    }
    public virtual void Init() { }

    public virtual void OnDestroy()
    {
        instance = null;
    }
}