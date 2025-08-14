using System.Runtime.CompilerServices;
using UnityEngine;

public class ObjectResizer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float h = 0f;
    private float baseOffset = .1f;
    private float debounce = .2f;
    private float currDebounce = 0f;
    void Start()
    {
        SetHeight();
    }

    private void Update()
    {
        currDebounce += Time.deltaTime;
    }

    private void OnMouseDown()
    {
        ToggleHeight();
    }

    public void ToggleHeight()
    {
        if (currDebounce >= debounce)
        {
            currDebounce = 0f;
            h = (h + 1) % 4;
            SetHeight();
        }
    }

    private void SetHeight()
    {
        float height = Mathf.Max(h, baseOffset);
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
        transform.position = new Vector3(transform.position.x, height / 2, transform.position.z);
    }
}
