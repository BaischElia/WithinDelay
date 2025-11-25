using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class uimanager : MonoBehaviour
{
    public GameObject inGameUI;
    public TMP_Text roundTxt;
    public TMP_Text scoreText;
    public TMP_Text trialsLeftText;
    public GameObject pauseUI;
    public TMP_Text pauseCountdownText;
    public TMP_Text accRoundText;
    public TMP_Text rtRoundText;
    public TMP_Text roundRoundText; // der text der in der Pause die Runde anzeigt, profi am werk
    public GameObject extendedPauseText;
    public GameObject confirmPreExitUI;
    public float pauseDuration = 30f;
    private int scorePoints = 0;
    private float accRound = 0f;
    private float highAccRound = 0f;
    private float rtRound = 0f;
    private float highRtRound = 4f;
    private bool accBetter = false;
    private bool rtBetter = false;
    private float maxRTForPoints = 4f;
    private Color redColor = new Color(0.89f, 0.20f, 0.1f);
    private Color greenColor = new Color(0.03f, 0.63f, 0.27f);
    private bool pauseKeyPressed = false;
    private int currRound = 0;
    private int earliestExtendedPauseRound = 8;
    private int maxRounds = 16;
    private float pauseTimeRemaining;


    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        maxRounds = GetComponent<trialmanager>().maxRounds;

    }

    public void StartGameUI()
    {
        inGameUI.SetActive(true);
        pauseUI.SetActive(false);
        // trialsLeftText.text = GetComponent<hitmanager>().currentMaxTrials.ToString();
        trialsLeftText.text = GetComponent<trialmanager>().currentMaxTrials.ToString();
        // currRound = GetComponent<trialmanager>().currentRound;

        // if (currRound == earliestPreExitRound)
        // {
        //     preExitTxt.SetActive(true);
        // }
    }


    void Update()
    {
        scoreText.text = "Points: " + scorePoints.ToString();

    }

    public void PauseGame()
    {
        inGameUI.SetActive(false);

        currRound = GetComponent<trialmanager>().currentRound - 1; // -1 because the round is already incremented for next round


        // get stats
        UpdateStats();
        UpdatePauseUI();

        pauseUI.SetActive(true);
        pauseKeyPressed = false;

        StartCoroutine(StartPauseCountdown());
    }

    public void NextRoundStart()
    {
        inGameUI.SetActive(true);
        pauseUI.SetActive(false);
        roundTxt.text = " ";
        GetComponent<datamanager>().ResetRoundStats();
        // GetComponent<hitmanager>().resetRound();
        GetComponent<trialmanager>().StartNewRound();
        trialsLeftText.text = GetComponent<trialmanager>().currentMaxTrials.ToString();
    }

    private IEnumerator StartPauseCountdown()
    {
        pauseTimeRemaining = pauseDuration;
        while (pauseTimeRemaining > 0)
        {
            pauseCountdownText.text = "next round starts in " + Mathf.Ceil(pauseTimeRemaining).ToString() + " s";
            yield return new WaitForSeconds(1f);
            pauseTimeRemaining -= 1f;
        }
        NextRoundStart();
    }

    private void UpdatePauseUI()
    {
        // Debug.Log("Upadte Pause UI");
        accRoundText.text = (accRound * 100).ToString("F2") + "%";
        rtRoundText.text = rtRound.ToString("F2") + "s";


        if (currRound != 0)
        {
            roundRoundText.text = "Round " + currRound.ToString() + "/" + maxRounds + " finished";
        }
        else
        {
            roundRoundText.text = "Practice round finished";
            scorePoints = 0;
        }



        if (accBetter)
        {
            accRoundText.color = greenColor;
        }
        else
        {
            accRoundText.color = redColor;
        }

        if (rtBetter)
        {
            rtRoundText.color = greenColor;
        }
        else
        {
            rtRoundText.color = redColor;
        }
    }

    private void UpdateStats()
    {
        // get stats
        int roundSuccessHitList = GetComponent<datamanager>().GetRoundSuccessHits();
        List<float> roundRTList = GetComponent<datamanager>().GetRoundRTs();

        accRound = (float)roundSuccessHitList / roundRTList.Count;

        // calculate sum for average RT
        float rtSum = 0f;
        foreach (float rt in roundRTList)
        {
            rtSum += rt;
        }

        // calculate average RT
        rtRound = rtSum / roundRTList.Count;

        // Check if stats are better than before
        if (accRound >= highAccRound)
        {
            highAccRound = accRound;
            accBetter = true;
        }
        else
        {
            accBetter = false;
        }

        if (rtRound < highRtRound)
        {
            highRtRound = rtRound;
            rtBetter = true;
        }
        else
        {
            rtBetter = false;
        }
    }

    public void UpdateInGameUI(bool hit, float RT, int trial, int maxTrial)
    {
        if (hit && RT <= maxRTForPoints)
        {
            // Calculate points based on reaction time
            // asked GPT what a good way to calculate points based on reaction time would be
            // for user HUD, not essential for research question
            float normalizedTime = Mathf.Clamp01(1.0f - (RT / maxRTForPoints));
            int basePoints = 100;
            scorePoints += Mathf.RoundToInt(basePoints * normalizedTime);
        }

        trialsLeftText.text = (maxTrial - trial).ToString();

    }

}
