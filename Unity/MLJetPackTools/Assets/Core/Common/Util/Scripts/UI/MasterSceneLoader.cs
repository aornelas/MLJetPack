using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterSceneLoader : MonoBehaviour {
	public static MasterSceneLoader instance = null;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void OnDestroy () {
        if (instance == this) {
            instance = null;
        }
	}

    public void ActionLoadScene(string sceneName)
    {
        StartCoroutine(UnloadSceneAndLoadScene(sceneName));
    }

    IEnumerator UnloadSceneAndLoadScene(string sceneName) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        yield break;
    }

    private void Update()
    {
    }
}
