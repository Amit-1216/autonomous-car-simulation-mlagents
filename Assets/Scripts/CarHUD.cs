using UnityEngine;

public class CarHUD : MonoBehaviour
{
    [Header("References")]
    public CarController carController;

    [Header("HUD Style")]
    public int fontSize = 18;
    public Color textColor = Color.white;
    public bool showBackground = true;
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.45f);

    [Header("Position")]
    public int paddingX = 16;
    public int paddingY = 16;

    [Header("Settings")]
    public bool useKMH = true;

    // Lap and reward are set externally
    private int lapCount = 0;
    private float totalReward = 0f;

    private GUIStyle labelStyle;
    private GUIStyle boxStyle;
    private bool stylesInitialized = false;

    // =========================
    // EXTERNAL SETTERS
    // =========================
    public void AddLap() => lapCount++;
    public void ResetLaps() => lapCount = 0;
    public void SetReward(float r) => totalReward = r;
    public void AddReward(float r) => totalReward += r;
    public void ResetReward() => totalReward = 0f;

    // =========================
    // GUI
    // =========================
    private void InitStyles()
    {
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            richText = true
        };
        labelStyle.normal.textColor = textColor;

        boxStyle = new GUIStyle(GUI.skin.box);
        Texture2D bg = new Texture2D(1, 1);
        bg.SetPixel(0, 0, backgroundColor);
        bg.Apply();
        boxStyle.normal.background = bg;

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!stylesInitialized) InitStyles();
        if (carController == null) return;

        // ?? Gather values ??????????????????????????????
        float speedMS = carController.GetComponent<Rigidbody>().linearVelocity.magnitude;
        float speed = useKMH ? speedMS * 3.6f : speedMS * 2.237f;
        string unit = useKMH ? "km/h" : "mph";
        float steer = carController.frontLeftWheel.steerAngle;

        // ?? Build lines ????????????????????????????????
        string line1 = $"Speed:    <color=#FFE55C>{Mathf.RoundToInt(speed)} {unit}</color>";
        string line2 = $"Steering: <color=#FFE55C>{steer:F2}�</color>";
        string line3 = $"Reward:   <color=#FFE55C>{totalReward:F1}</color>";
        string line4 = $"Lap:      <color=#FFE55C>{lapCount}</color>";

        string fullText = $"{line1}\n{line2}\n{line3}\n{line4}";

        // ?? Measure size ???????????????????????????????
        GUIContent content = new GUIContent(fullText);
        Vector2 size = labelStyle.CalcSize(content);
        // CalcSize doesn't handle multiline well � use fixed height
        float lineH = fontSize + 6f;
        float boxW = size.x + 20f;
        float boxH = lineH * 4 + 16f;

        Rect boxRect = new Rect(paddingX, paddingY, boxW, boxH);
        Rect labelRect = new Rect(paddingX + 10, paddingY + 8, boxW, boxH);

        // ?? Draw ???????????????????????????????????????
        if (showBackground)
            GUI.Box(boxRect, GUIContent.none, boxStyle);

        GUI.Label(labelRect, fullText, labelStyle);
    }
}