using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterOptionsCanvas : MonoBehaviour {
    public Text textMusicStatus;
    public Text textMeshOcclusionStatus;
    public Text textMeshWireStatus;

    public static MasterOptionsCanvas instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        UpdateStatii();
    }
 
    void UpdateStatii() {
        if (textMeshOcclusionStatus != null && MLMeshParent.instance != null) {
            textMeshOcclusionStatus.text = "Occlusion Mesh: " +
                (MLMeshParent.instance.defaultDisplayMode == MLMeshParent.DisplayMode.Occlusion ? "On" : "Off");
        }

        if (textMeshWireStatus != null && MLMeshParent.instance != null)
        {
            textMeshWireStatus.text = "Wire Mesh: " + (MLMeshParent.instance.wiresEnabled ? "On" : "Off");
        }
    }

    private void OnDestroy()
    {
        if (instance == this) {
            instance = null;
        }
    }

    public void ToggleMeshes()
    {
        if (MLMeshParent.instance != null)
        {
            MLMeshParent.instance.ToggleMesh();
            UpdateStatii();
        }
    }

    public void ToggleWires()
    {
        if (MLMeshParent.instance != null) {
            SetWiresEnabled(!MLMeshParent.instance.wiresEnabled);
        }
    }

    public void SetWiresEnabled(bool wiresEnabled)
    {
        if (MLMeshParent.instance != null)
        {
            MLMeshParent.instance.SetWiresEnabled(wiresEnabled);
            UpdateStatii();
        }
    }
}
