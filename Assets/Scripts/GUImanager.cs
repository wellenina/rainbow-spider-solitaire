using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUImanager : MonoBehaviour
{
    // MOVES
    [SerializeField] private GameObject movesGUI;
    private TextMeshProUGUI movesText;
    private int moves;

    // TIME
    [SerializeField] private GameObject timeGUI;
    private TextMeshProUGUI timeText;
    private bool hasTimeStarted;
    private float totalTime = 0;
    private float minutes;
    private float seconds;
    private string secondsString;

    // Start is called before the first frame update
    void Start()
    {
        movesText = movesGUI.GetComponent<TextMeshProUGUI>();
        moves = 0;
        timeText = timeGUI.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTimeStarted)
        {
            UpdateTime();
        }
    }

    public void IncrementMoves()
    {
        moves++;
        movesText.text = moves.ToString();
        if (moves == 1)
        {
            StartTime();
        }
    }

    public void StartTime()
    {
        hasTimeStarted = true;
    }

    void UpdateTime()
    {
        totalTime += Time.deltaTime;
	    minutes = Mathf.Floor(totalTime / 60.0f);
	    seconds = Mathf.Floor(totalTime % 60);
        secondsString = seconds > 9 ? seconds.ToString() : "0" + seconds.ToString();
	    timeText.text = minutes.ToString() + ":" + secondsString;
    }

    public void PauseResume()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        InputManager.DisableInput(!InputManager.inputDisabled);
    }
}
