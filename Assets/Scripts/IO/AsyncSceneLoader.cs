using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncSceneLoader : MonoBehaviour
{
    public static AsyncSceneLoader instance;
    
    /* void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
    } */

    private void Start()
    {
        loadingScreen.SetActive(false);
        instance = this;
    }

    public Slider progress;
    public GameObject loadingScreen;
    
    public void LoadScene(string scene) {
        loadingScreen.SetActive(true);
        progress.value = 0;
        
        StartCoroutine(LoadLevelASync(scene));
    }
    
    IEnumerator LoadLevelASync(string scene) {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene);
        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            progress.value = progressValue;
            yield return null;
        }
    }

}