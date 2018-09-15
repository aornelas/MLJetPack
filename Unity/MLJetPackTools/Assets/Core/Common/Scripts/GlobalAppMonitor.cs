using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using MLJetPack.AlignedXR;

public class GlobalAppMonitor : MonoBehaviour
{
    public static GlobalAppMonitor instance = null;

    [SerializeField]
    private Camera _theMainCamera;

    private static Camera _mainCamera;
    public static Camera mainCamera
    {
        get
        {
            if (instance != null && _mainCamera == null)
            {
                instance._theMainCamera = _mainCamera = Camera.main;
            }
            if (instance == null)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple GlobalAppMonitors. Instance = " + instance.gameObject + " while destroying " + name);
            Destroy(gameObject);
            return;
        }
    }
}
