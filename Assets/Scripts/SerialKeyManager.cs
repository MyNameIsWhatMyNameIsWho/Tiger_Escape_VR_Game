using System.Collections;
using System.Collections.Generic;
using NavKeypad;
using TMPro;
using UnityEngine;

public class SerialKeyManager : MonoBehaviour
{
    private Keypad keypad;
    [SerializeField] private GameObject NumberPresenterPrefab;
    private string finalKey = "";
    private List<int> numbers = new List<int>();
    private int finalKeyCurrentIndex = 0;
    void generateFinalKey() {
        for(int i = 0; i < 3; i++) {
            finalKey += numbers[Random.Range(0, numbers.Count)];
        }
        Debug.Log("key: " + finalKey);
    }
    public void showPartOfFinalKey(Vector3 position, Quaternion rotation) {
        GameObject instance = Instantiate(NumberPresenterPrefab, position, rotation);
        instance.gameObject.SetActive(false);
        instance.transform.Rotate(new Vector3(0, 180, 0));
        TextMeshPro textMeshPro = instance.GetComponent<TextMeshPro>();
        if(textMeshPro) {
            textMeshPro.text = finalKey[finalKeyCurrentIndex].ToString();
            if(finalKeyCurrentIndex < 3) {
                finalKeyCurrentIndex += 1;
            }
        }
        StartCoroutine(AnimateNumberPresenter(instance, position));
    }
    private IEnumerator AnimateNumberPresenter(GameObject instance, Vector3 targetPosition) {
        float duration = 1f;
        float elapsed = 0f;

        Vector3 startPos = targetPosition + new Vector3(0, -0.4f, 0);
        Vector3 endPos = targetPosition + new Vector3(0, 0.05f, 0);
        instance.gameObject.SetActive(true);

        TMPro.TextMeshPro textMeshPro = instance.GetComponentInChildren<TMPro.TextMeshPro>();
        if (textMeshPro != null)
        {
            Color originalColor = textMeshPro.color;
            Color fadedColor = originalColor;
            fadedColor.a = 0;
            textMeshPro.color = fadedColor;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            instance.transform.position = Vector3.Lerp(startPos, endPos, t);

            if (textMeshPro != null)
            {
                Color currentColor = textMeshPro.color;
                currentColor.a = Mathf.Lerp(0, 1, t);
                textMeshPro.color = currentColor;
            }

            yield return null;
        }

        if (textMeshPro != null)
        {
            Color finalColor = textMeshPro.color;
            finalColor.a = 1;
            textMeshPro.color = finalColor;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        keypad = FindObjectOfType<Keypad>();
        if(!keypad) {
            Debug.LogError("keypad not found");
        }
        for(int i = 0; i < 10; i++) {
            numbers.Add(i);
        }
        generateFinalKey();
        if(int.TryParse(finalKey, out int value)) {
            keypad.keypadCombo = value;
        } else {
            Debug.LogError("could not parse keypad combo");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}