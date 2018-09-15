using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicLeapDebug : MonoBehaviour {

    public static MagicLeapDebug instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        UpdateMeshText();
    }

    private void OnDestroy()
    {
        if (instance == this) {
            instance = null;
        }
    }

    public Text textOutput;

    // Use this for initialization
    public static void Log(string text) {
        if (instance != null && !string.IsNullOrEmpty(text)) {
            instance.textOutput.text = text + "\n" + instance.textOutput.text;
        }
        Debug.Log(text);
	}

    public enum DebugMode {
        DisplayOnly,
        Calibrate
    }

    public Text textMeshToggle;
    public GameObject meshParent;
    public void ToggleMesh()
    {
        SetMeshActive(!meshParent.activeSelf);
    }

    public GameObject meshParentReal;
    void SetMeshActive(bool meshToBeActive) {
        if (textMeshToggle != null && meshParent != null)
        {
            meshParent.SetActive(meshToBeActive);
            meshParentReal.SetActive(meshToBeActive);
            UpdateMeshText();
        }
    }


    public void ToggleCalibration()
    {
        SetCalibrationActive(!calibrationActive);
    }

    public bool calibrationActive = false;
    void SetCalibrationActive(bool isActive)
    {
        SetMeshActive(true);
        calibrationActive = isActive;
        if (textCalibrationButton != null) {
            textCalibrationButton.text = isActive ? "Stop Calibration" : "Start Calibration";
        }
    }

    public LayerMask layerMaskCalibration;
    public Text textCalibrationButton;
    private void Update()
    {
        if (calibrationActive) {

            if (MLJetPack.Interactions.MagicLeapController.instance != null) {
                if (MLJetPack.Interactions.MagicLeapController.instance.ActionButtonUp(
                    (int)MLJetPack.Interactions.MagicLeapController.Control.Trigger))
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(MLJetPack.Interactions.MagicLeapController.instance.transform.position,
                                                     MLJetPack.Interactions.MagicLeapController.instance.transform.forward,
                                        out hitInfo, 5, layerMaskCalibration))
                    {
                        Vector3 position = hitInfo.point;
                        Vector3 forward = GlobalAppMonitor.mainCamera.transform.forward;
                        forward.y = 0;
                        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                        MLJetPack.AlignedXR.AlignedReferencePoint.instance.
                                SetReferencePointByPositionAndRotation(position, rotation);
                    } else {
                        Vector3 position = MLJetPack.Interactions.MagicLeapController.instance.transform.position;
                        Vector3 forward = GlobalAppMonitor.mainCamera.transform.forward;
                        forward.y = 0;
                        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                        MLJetPack.AlignedXR.AlignedReferencePoint.instance.
                                SetReferencePointByPositionAndRotation(position, rotation);
                    }
                    //else
                    //{
                        //Debug.Log("no raycast hit");
                    //}
                }
                //else
                //{
                //    Debug.Log("action button upis null");
                //}
            }
            //else
            //{
            //    Debug.Log("magic leap controller instance is null");
            //}
        }
    }

    void UpdateMeshText()
    {
        if (textMeshToggle != null && meshParent != null) {
            textMeshToggle.text = meshParent.activeSelf ? "Toggle Off Mesh" : "Toggle On Mesh";
        }
    }

    public Material meshMaterial;
}
