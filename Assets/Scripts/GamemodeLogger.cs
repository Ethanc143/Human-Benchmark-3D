using UnityEngine;
using TMPro;   // ← use TMPro instead of UnityEngine.UI

public class GamemodeLogger : MonoBehaviour
{
    // survives scene loads during play, resets when you stop
    public static int GamemodeIndex = 0;

    TMP_Dropdown dropdown;   // ← TMP_Dropdown instead of Dropdown

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null)
        {
            Debug.LogError("[GamemodeLogger] No TMP_Dropdown on this GameObject.");
            enabled = false;
            return;
        }

        // restore previous index (clamped to valid range)
        dropdown.value = Mathf.Clamp(
            GamemodeIndex,
            0,
            dropdown.options.Count - 1
        );

        // listen for changes
        dropdown.onValueChanged.AddListener(OnGamemodeChanged);
    }

    void OnGamemodeChanged(int idx)
    {
        GamemodeIndex = idx;
        Debug.Log($"[GamemodeLogger] Saved gamemode index: {idx}");
    }
}
