using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneChangeManager : MonoBehaviour
{
    private float fadeDuration = 2;
    public Color fadeColor;
    [SerializeField] private Renderer rend;
    [SerializeField] private GameObject textPrefab;
    public void ChangeScene(bool win) {
        if(win) {
            StartCoroutine(FadeScreen(0, 1, "WinScene"));
        } else {
            StartCoroutine(FadeScreen(0, 1, "LoseScene"));
        }
    }
    private IEnumerator FadeScreen(float alphaIn ,float aplhaOut, string SceneToLoad) {
        float timer = 0;
        while(timer < fadeDuration) {
            Color newColor = fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, aplhaOut, timer / fadeDuration);
            rend.material.SetColor("_Color",newColor);
            timer += Time.deltaTime;
            yield return null;
        }
        Color newColor2 = fadeColor;
        newColor2.a = aplhaOut;
        rend.material.SetColor("_Color",newColor2);
        if(SceneToLoad != null) {
            SceneManager.LoadScene(SceneToLoad);
            yield return new WaitForSeconds(1);
            StartCoroutine(FadeScreen(1, 0, null));
        } else {
            
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
