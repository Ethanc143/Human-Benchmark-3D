using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    //UI
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI strikesText;
    [SerializeField] private Button submitButton;

    //timers
    [SerializeField] private float timeLimit = 5f;
    [SerializeField] private float displayTime = 2.5f;

    //game parameters
    private int level = 0;
    private int strikes = 0;

    //object managmenet
    [SerializeField] private GameObject plane;
    [SerializeField] private LineDrawer lineDrawer;
    private int sideLength = 0;
    private GameObject[,] blocks;
    private float[,] heights;
    private List<GameObject> extras;
    //object management
    private GameObject[,] adjustables;
    [SerializeField] private GameObject adjustableBlock;
    
    void Start()
    {
        submitButton.gameObject.SetActive(false);
        IncreaseLevel();
        StartCoroutine(TimerPhase());
    }

    private IEnumerator TimerPhase()
    {
        GenerateScene();
        strikesText.text = $"You are on level {level}. You have {strikes}/3 strikes.";
        float remaining = (level == 1) ? timeLimit * 2 : timeLimit;
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
        submitButton.gameObject.SetActive(true);
    }

    public void OnSubmitButtonPressed()
    {
        StartCoroutine(Submit());
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
            strikesText.text = (strikes >= 3) ? $"You have {strikes}/3 strikes. Test complete." : $"Incorrect. Retrying level {level}. You have {strikes}/3 strikes.";
        }
        submitButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(displayTime);
        for(int i = 0;i < currLength;i++)
        {
            for(int j = 0;j < currLength;j++)
            {
                Destroy(adjustables[i, j]);
            }
        }
        if (strikes == 3)
        {
            mainText.text = $"You were able to reach level {level}, exceeding {GetPercentile()} percent of players.";
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

    private bool IsCorrect()
    {
        bool correct = true;
        for(int i = 0;i < sideLength;i++)
        {
            for(int j = 0;j < sideLength;j++)
            {
                if (heights[i, j] != adjustables[i, j].transform.localScale.y)
                {
                    Renderer renderer = adjustables[i, j].GetComponent<Renderer>();
                    renderer.material.color = Color.red;
                    correct = false;
                } 
            }
        }
        return correct;
    }

    private float GetPercentile()
    {
        return 50f;
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
            block = GameObject.CreatePrimitive(PrimitiveType.Cube);

        }
        block.transform.localScale = new Vector3(scale, h, scale);
        block.transform.position = new Vector3(scale * (x + .5f), h / 2, scale * (z + .5f));
        Renderer renderer = block.GetComponent<Renderer>();
        renderer.material.color = (((x + z) % 2) == 0) ? Color.black : Color.white;
        return block;
    }
}
