using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    public string sceneName;
    public float waitTime = 0f;

    public void Change()
    {
        Debug.Log("SceneChange");
        StartCoroutine(WaitingAndChange(sceneName));
    }

    IEnumerator WaitingAndChange(string targetScene)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        SceneManager.LoadSceneAsync(targetScene);
    }
}