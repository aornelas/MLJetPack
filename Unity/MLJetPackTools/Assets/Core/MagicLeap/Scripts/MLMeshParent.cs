using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLMeshParent : MonoBehaviour
{
    public static MLMeshParent instance;

    public Material materialOcclusion;
    public Material materialWireframe;

    public enum DisplayMode {
        None,
        Occlusion
    }

    public DisplayMode defaultDisplayMode = DisplayMode.Occlusion;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        if (instance == this) {
            instance = null;
        }
    }

    public Material[] GetMaterials(bool withWireframe) {
        switch (defaultDisplayMode) {
            case DisplayMode.None:
                if (withWireframe)
                {
                    return new Material[] { materialWireframe };
                }
                break;

            case DisplayMode.Occlusion:
                if (withWireframe) {
                    return new Material[] { materialOcclusion, materialWireframe };
                } else {
                    return new Material[] { materialOcclusion };
                }
        }
        return new Material[0];
    }


    public bool wiresEnabled = false;

    // Use this for initialization
    public void SetWiresEnabled(bool shouldBeEnabled)
    {
        wiresEnabled = shouldBeEnabled;
        UpdateMaterials();
    }

    public void UpdateMaterials() {
        Material[] mats = GetMaterials(wiresEnabled);
        bool shouldCollide = defaultDisplayMode == DisplayMode.Occlusion;
        foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (mats.Length > 0) {
                mr.materials = mats;
                mr.enabled = true;
            }
            else {
                mr.enabled = false;
            }
            MeshCollider c = mr.gameObject.GetComponent<MeshCollider>();
            if (c != null)
            {
                c.enabled = shouldCollide;
            }
        }
    }

    public void ToggleMesh()
    {
        switch(defaultDisplayMode) {
            case DisplayMode.None:
                SetMeshDisplayMode(DisplayMode.Occlusion);
                break;

            case DisplayMode.Occlusion:
                SetMeshDisplayMode(DisplayMode.None);
                break;
        }
    }

    public void SetMeshDisplayMode(DisplayMode displayMode) {
        defaultDisplayMode = displayMode;
        UpdateMaterials();
    }
}
