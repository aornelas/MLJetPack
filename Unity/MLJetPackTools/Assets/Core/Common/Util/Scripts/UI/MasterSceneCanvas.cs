using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MasterSceneCanvas : MonoBehaviour {
    public GameObject prefabExperiences;
    public GameObject objectExperiences;

    public List<GameObject> panels = new List<GameObject>();

    public void SetActivePanel(int panelIndex) {
        for (int i = 0; i < panels.Count; i++) {
            panels[i].SetActive(i == panelIndex);
        }
    }

    public void ActionLoadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneAndLoadScene(sceneName));
    }

    IEnumerator UnloadSceneAndLoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        yield break;
    }

    public void ShowExperiencesMenu() {
        if (objectExperiences != null) {
            objectExperiences.SetActive(true);
        } else {
            objectExperiences = GameObject.Instantiate(prefabExperiences);
            objectExperiences.transform.SetParent(transform);
            objectExperiences.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, -1);
            objectExperiences.GetComponent<RectTransform>().localEulerAngles = Vector3.zero;
        }
    }

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha9)) {
            ShowExperiencesMenu();
        }
    }

    public void ToggleMeshes() {
        MLMeshParent.instance.ToggleMesh();
    }

    public void SetWiresEnabled(bool wiresEnabled)
    {
        MLMeshParent.instance.SetWiresEnabled(wiresEnabled);
    }
}
