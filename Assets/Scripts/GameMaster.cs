using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    //UI
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI strikesText;
    [SerializeField] private Button submitButton;
    private bool isSubmitting = false;

    //timers
    [SerializeField] private float timeLimit = 5f;
    [SerializeField] private float displayTime = 2.5f;
    private float remaining = 0f;

    //game parameters
    private int level = 0;
    private int strikes = 0;

    //object managmenet
    [SerializeField] private GameObject plane;
    [SerializeField] private LineDrawer lineDrawer;
    [SerializeField] private GameObject cube;
    private int sideLength = 0;
    private GameObject[,] blocks;
    private float[,] heights;
    private List<GameObject> extras;

    //object management(adjustable version)
    private GameObject[,] adjustables;
    [SerializeField] private GameObject adjustableBlock;

    //object mangement(feedback)
    private List<GameObject> feedbackBlocks;

    //saving 
    private const string DATABASE_URL = "https://human-benchmark-database-default-rtdb.firebaseio.com/"; 
    private List<int> scores = new List<int>();
    private bool baby = false;
    [SerializeField] private string dataName = "scores";

    void Start()
    {
        IncreaseLevel();
        StartCoroutine(TimerPhase());
    }

    private IEnumerator TimerPhase()
    {
        submitButton.gameObject.SetActive(true);
        isSubmitting = false;
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Skip";
        GenerateScene();
        strikesText.text = $"You are on level {level}. You have {strikes}/3 strikes.";
        remaining = (level == 1) ? timeLimit * 2 : timeLimit;
        while (remaining >= 0f)
        {
            mainText.text = $"You have {remaining:F0} seconds to memorize the scene.";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }
        PrepareSubmission();
    }

    private void GenerateScene()
    {
        //draw gridlines
        lineDrawer.DrawGrid(sideLength);
        //sample spawned blocks
        int maxPairs = (sideLength) * (sideLength);
        blocks = new GameObject[sideLength, sideLength];
        heights = new float[sideLength, sideLength];
        //generate random set
        List<(int, int)> all = new List<(int, int)>(maxPairs);
        for (int a = 0; a < sideLength; a++)
            for (int b = 0; b < sideLength; b++)
                all.Add((a, b));
        //sample from random set
        System.Random random = new System.Random();
        for (int i = 0;i < level;i++)
        {
            int idx = UnityEngine.Random.Range(0, all.Count);
            var (x, y) = all[idx];
            int height = UnityEngine.Random.Range(1, 4);
            blocks[x, y] = SpawnBlock(x, y, height, false);
            heights[x, y] = height;
            all.RemoveAt(idx);
        }
        //fill in checkerboard
        extras = new List<GameObject>();
        for(int i = 0;i < all.Count;i++)
        {
            var (x, y) = all[i];
            heights[x, y] = 0.1f;
            extras.Add(SpawnBlock(x, y, .1f, false));
        }
    }

    private void PrepareSubmission()
    {
        //delete blocks
        mainText.text = "Please accurately recreate the previous scene and press submit.";
        for(int i = 0;i < sideLength;i++)
        {
            for(int j = 0;j < sideLength;j++)
            {
                if (blocks[i, j] != null)
                {
                    Destroy(blocks[i, j]);
                    blocks[i, j] = null;
                }
            }
        }
        //destroy extras
        for(int i = extras.Count - 1;i >= 0;i--) {
            Destroy(extras[i]);
            extras.RemoveAt(i);
        }
        //make adjustables
        adjustables = new GameObject[sideLength, sideLength];
        for(int i = 0;i < sideLength;i++)
        {
            for(int j = 0;j < sideLength;j++)
            {
                adjustables[i, j] = SpawnBlock(i, j, UnityEngine.Random.Range(1, 4), true);
            }
        }
        //enable handles
        isSubmitting = true;
        submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Submit";
    }

    public void OnSubmitButtonPressed()
    {
        if (isSubmitting)
        {
            StartCoroutine(Submit());
        }
        else {
            remaining = 0f;
        }
    }

    private IEnumerator Submit() {
        int currLength = sideLength;
        if(IsCorrect())
        {
            mainText.text = $"Correct! Moving onto level {level + 1}.";
            IncreaseLevel();
        } else
        {
            strikes++;
            if(level == 1)
            {
                baby = true;
            }
            strikesText.text = (strikes >= 3) ? $"You have {strikes}/3 strikes. Test complete." : $"Incorrect. Retrying level {level}. You have {strikes}/3 strikes.";
        }
        for (int i = 0; i < currLength; i++)
        {
            for (int j = 0; j < currLength; j++)
            {
                Destroy(adjustables[i, j]);
            }
        }
        submitButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(displayTime);
        Clean(feedbackBlocks);
        if (strikes == 3)
        {
            Finish();
        } else
        {
            StartCoroutine(TimerPhase());
        }
    }

    private void IncreaseLevel()
    {
        level += 1;
        sideLength = (int) Mathf.Ceil(Mathf.Sqrt((2.0f * level)));
    }

    private void Finish()
    {
        StartCoroutine(GetScores());
        if(!baby)
        {
            StartCoroutine(LogScores());
        }
    }

    private bool IsCorrect()
    {
        bool correct = true;
        feedbackBlocks = new List<GameObject>();
        for (int i = 0;i < sideLength;i++)
        {
            for(int j = 0;j < sideLength;j++)
            {
                float setHeight = adjustables[i, j].transform.localScale.y;
                Destroy(adjustables[i, j]);
                if (setHeight != heights[i, j])
                {
                    GameObject prev = SpawnBlock(i, j, setHeight, false);
                    Renderer renderer = prev.GetComponent<Renderer>();
                    Color color1 = renderer.material.color;
                    renderer.material.color = new Color(color1.r, color1.g, color1.b, 0.5f);

                    GameObject actual = SpawnBlock(i, j, heights[i, j], false);
                    Renderer renderer2 = actual.GetComponent<Renderer>();
                    Color color2 = Color.red;
                    renderer2.material.color = new Color(color2.r, color2.g, color2.b, 0.5f);
                    actual.transform.localScale = actual.transform.localScale * 1.001f;

                    correct = false;
                    feedbackBlocks.Add(prev);
                    feedbackBlocks.Add(actual);
                } else
                {
                    feedbackBlocks.Add(SpawnBlock(i, j, setHeight, false));
                }
            }
        }
        return correct;
    }

    private float GetPercentile() { 
        for(int i = 0;i < scores.Count;i++)
        {
            if (scores[i] >= level)
            {
                return (float)i / scores.Count * 100f;
            } 
        }
        return 100f;
    }

    public GameObject SpawnBlock(int x, int z, float h, bool adjustable)
    {
        float scale = 10f / sideLength;
        GameObject block;
        if(adjustable)
        {
            block = Instantiate(adjustableBlock);
            h = 0f;
        } else
        {
            block = Instantiate(cube);

        }
        block.transform.localScale = new Vector3(scale, h, scale);
        block.transform.position = new Vector3(scale * (x + .5f), h / 2, scale * (z + .5f));
        Renderer renderer = block.GetComponent<Renderer>(); 
        renderer.material.color = (((x + z) % 2) == 0) ? Color.black : Color.white;
        return block;
    }

    private IEnumerator LogScores()
    {
        string json = "{\"score\": " + level + "}";
        string url = $"{DATABASE_URL}/{dataName}.json";
        using var req = new UnityWebRequest(url, "POST");
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogWarning("⚠️ Upload failed: " + req.error);
    }

    private IEnumerator GetScores()
    {
        using var req = UnityWebRequest.Get($"{DATABASE_URL}/{dataName}.json");
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("⚠️ Download failed: " + req.error);
        }
        string raw = req.downloadHandler.text;
        if(raw == null)
        {
            mainText.text = $"You were able to reach level {level}, and you were the first one to get their score logged!";
        }
        var scores = new List<int>();
        var matches = Regex.Matches(raw, "\"score\"\\s*:\\s*(\\d+)");
        foreach (Match m in matches)
        {
            scores.Add(int.Parse(m.Groups[1].Value));
        }
        scores.Sort();
        this.scores = scores;
        mainText.text = $"You were able to reach level {level}, exceeding {GetPercentile()} percent of players.";
    }

    private void Clean(List<GameObject> gos)
    {
        if(gos == null) { return; }
        for (int i = gos.Count - 1;i >= 0;i--)
        {
            Destroy(gos[i]);
            gos.RemoveAt(i);
        }
    }
}
