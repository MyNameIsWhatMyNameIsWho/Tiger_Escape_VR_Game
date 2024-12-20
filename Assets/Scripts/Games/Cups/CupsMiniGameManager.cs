using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CupsMiniGameManager : MiniGame
{
    [SerializeField] private GameObject cup1, cup2, cup3, ball;
    SerialKeyManager serialKeyManager;
    private GameObject cupWithBall = null;
    private Rigidbody cup1Rigidbody, cup2Rigidbody, cup3Rigidbody;
    private List<GameObject> cupsList = new List<GameObject>();
    private List<Vector3> cupsLocation = new List<Vector3>();
    private float showSpeed = 0.1f;
    public int minigameRounds = 3;
    private int remainingRounds;
    public int maxShuffleCount = 10;
    public int minShuffleCount = 4;
    private int shuffleCount;
    public float shuffleMovementDuration = 4f;
    private int completedCoroutines = 0;
    private int totalCoroutines = 2;
    private int wins = 0;
    float ballYcoord = 0;
    private Vector3 keyTextPosition;
    private bool running = false;
    private bool completed = false;

    // Start is called before the first frame update
    void Start()
    {
        cup1.tag = "Untagged";
        cup2.tag = "Untagged";
        cup3.tag = "Untagged";

        keyTextPosition = cup2.transform.position;

        cup1Rigidbody = cup1.GetComponent<Rigidbody>();
        cup2Rigidbody = cup2.GetComponent<Rigidbody>();
        cup3Rigidbody = cup3.GetComponent<Rigidbody>();
        if(!cup1Rigidbody || !cup2Rigidbody || !cup3Rigidbody) {
            Debug.LogError("could not get cup rigidbody");
            return;
        }

        serialKeyManager = FindObjectOfType<SerialKeyManager>();
        if(!serialKeyManager) {
            Debug.LogError("could not get serialKeyManager");
            return;
        }

        cupsList.Add(cup1);
        cupsLocation.Add(cup1.transform.position);
        cupsList.Add(cup2);
        cupsLocation.Add(cup2.transform.position);
        cupsList.Add(cup3);
        cupsLocation.Add(cup3.transform.position);

        ballYcoord = cup1.transform.position.y - 0.074f;
    }

    public override void StartGame() {
        if(running || completed) {
            return;
        }
        running = true;
        wins = 0;
        remainingRounds = minigameRounds;
        shuffleCount = Random.Range(minShuffleCount, maxShuffleCount);
        int firstIndex = Random.Range(0, cupsList.Count);
        PutBallToCup(cupsList[firstIndex]);

        StartCoroutine(ShowCupsCoroutine());
    }
    public override void EndGame() {
        running = false;
        wins = 0;
        remainingRounds = minigameRounds;
        shuffleCount = Random.Range(minShuffleCount, maxShuffleCount);
        int firstIndex = Random.Range(0, cupsList.Count);
        PutBallToCup(cupsList[firstIndex]);
    }
    void PutBallToCup(GameObject cup) {
        Vector3 cupPos = cup.transform.position;
        cupPos.y = ballYcoord;
        ball.gameObject.transform.position = cupPos;
        cupWithBall = cup;
    }   
    IEnumerator ShowCupCoroutine(GameObject cup) {
        Rigidbody rigidbody = cup.GetComponent<Rigidbody>();
        if(rigidbody) {
            rigidbody.isKinematic = true;
            float maxY = cup.transform.position.y + 0.35f;
            while((maxY - cup.transform.position.y) > 0.008f) {
                cup.transform.position = Vector3.Lerp(cup.transform.position, new Vector3(cup.transform.position.x, maxY, cup.transform.position.z), 0.01f);
                yield return null;
            }

            yield return new WaitForSeconds(1);
            float minY = cup.transform.position.y - 0.35f;
            while((cup.transform.position.y - minY) > 0.008f) {
                cup.transform.position = Vector3.Lerp(cup.transform.position, new Vector3(cup.transform.position.x, minY, cup.transform.position.z), 0.01f);
                yield return null;
            }

            if(GameObject.ReferenceEquals(cup, cupWithBall)) {
                wins += 1;
                if(wins == minigameRounds) {
                    serialKeyManager.showPartOfFinalKey(keyTextPosition + new Vector3(0, 0.8f, 0), transform.rotation);
                    completed = true;
                }
            }

            remainingRounds -= 1;
            if(remainingRounds > 0) {
                shuffleCount = Random.Range(minShuffleCount, maxShuffleCount);
                ShuffleCups();
            } else {
                running = false;
            }
        }
    }
    IEnumerator ShowCupsCoroutine() {
        yield return new WaitForSeconds(3);
        cup1Rigidbody.isKinematic = true;
        cup2Rigidbody.isKinematic = true;
        cup3Rigidbody.isKinematic = true;
        float maxY = cup1.transform.position.y + 0.35f;
        while((maxY - cup1.transform.position.y) > 0.008f) {
            cup1.transform.position = Vector3.Lerp(cup1.transform.position, new Vector3(cup1.transform.position.x, maxY, cup1.transform.position.z), 0.01f);
            cup2.transform.position = Vector3.Lerp(cup2.transform.position, new Vector3(cup2.transform.position.x, maxY, cup2.transform.position.z), 0.01f);
            cup3.transform.position = Vector3.Lerp(cup3.transform.position, new Vector3(cup3.transform.position.x, maxY, cup3.transform.position.z), 0.01f);
            yield return null;
        }

        yield return new WaitForSeconds(2);
        float minY = cup1.transform.position.y - 0.35f;
        while((cup1.transform.position.y - minY) > 0.008f) {
            cup1.transform.position = Vector3.Lerp(cup1.transform.position, new Vector3(cup1.transform.position.x, minY, cup1.transform.position.z), 0.01f);
            cup2.transform.position = Vector3.Lerp(cup2.transform.position, new Vector3(cup2.transform.position.x, minY, cup2.transform.position.z), 0.01f);
            cup3.transform.position = Vector3.Lerp(cup3.transform.position, new Vector3(cup3.transform.position.x, minY, cup3.transform.position.z), 0.01f);
            yield return null;
        }
        ShuffleCups();
    }

    void ShuffleCups() {
        ball.transform.SetParent(cupWithBall.transform);

        int firstIndex = Random.Range(0, cupsList.Count);
        GameObject firstCup = cupsList[firstIndex];

        List<GameObject> remainingElements = new List<GameObject>(cupsList);
        remainingElements.RemoveAt(firstIndex);

        int secondIndex = Random.Range(0, remainingElements.Count);
        GameObject secondCup = remainingElements[secondIndex];

        Vector3 centerPointBack = ((firstCup.transform.position + secondCup.transform.position)/2) - new Vector3(0, 0, 0.3f);
        Vector3 cetnerPointFront = ((firstCup.transform.position + secondCup.transform.position)/2) + new Vector3(0, 0, 0.3f);

        completedCoroutines = 0;

        int direction = Random.Range(0, 1);
        if(direction == 0) {
            StartCoroutine(MoveAlongCurve(firstCup, firstCup.transform.position, centerPointBack, secondCup.transform.position));
            StartCoroutine(MoveAlongCurve(secondCup, secondCup.transform.position, cetnerPointFront, firstCup.transform.position));
        } else {
            StartCoroutine(MoveAlongCurve(firstCup, firstCup.transform.position, cetnerPointFront, secondCup.transform.position));
            StartCoroutine(MoveAlongCurve(secondCup, secondCup.transform.position, centerPointBack, firstCup.transform.position));
        }
        shuffleCount -= 1;
    }
    void makeCupsInteractible() {
        ball.transform.parent = null;
        cup1.tag = "Grabbable";
        cup2.tag = "Grabbable";
        cup3.tag = "Grabbable";
    }
    private IEnumerator MoveAlongCurve(GameObject o, Vector3 point0, Vector3 point1, Vector3 point2)
    {
        float time = 0f;

        while (time <= 1f)
        {
            Vector3 position = CalculateBezierPoint(time, point0, point1, point2);
            o.transform.position = position;

            time += Time.deltaTime * shuffleMovementDuration;

            yield return null;
        }
        o.transform.position = point2;
        completedCoroutines += 1;
        if(shuffleCount > 0 && completedCoroutines == totalCoroutines) {
            ShuffleCups();
        } else if(completedCoroutines == totalCoroutines) {
            makeCupsInteractible();
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
    }
    public void choseCup(GameObject chosenCup) {
        cup1.tag = "Untagged";
        cup2.tag = "Untagged";
        cup3.tag = "Untagged";
        Outline outline = chosenCup.GetComponent<Outline>();
        if(outline) {
            outline.enabled = false;
        }
        StartCoroutine(ShowCupCoroutine(chosenCup));
    }
}
