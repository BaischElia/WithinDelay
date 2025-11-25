using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data.Common;

public class hitmanager : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject FPSController;
    public GameObject orb_middle;
    public GameObject orb_right;
    public GameObject orb_left;
    public GameObject coordinates_plane;
    // public ParticleSystem shootParticle;
    public string version = "lld";
    public float longDelay = 0.6f;
    public float stimulusTime = 0.4f;
    public int maxTrials = 60;
    public int maxRounds = 8;
    public int practiceTrials = 10;
    //public bool tutorialDone = false;

    private float startRT;
    private float endRT;
    private float currRT;
    private int currentTrial = 0;
    public int currentRound = 0;
    private List<string> trialOrder;
    private string activeOrbStr;
    private bool trialRunning = false;
    private bool roundFinished = false;
    private GameObject activeOrbObj;
    public int currentMaxTrials = 0; // for shuffle and handling practice & experiment trials
    // private bool gameFinished = false;
    private bool canCastSpell = false;

    private float mouse_x = 0;
    private float mouse_y = 0;
    private Bounds targetBounds = new Bounds();
    private String currentPhase = "waitingForTarget";
    private int layerMaskTracker;
    private bool trackingStarted = false;
    private bool isWaitingPhase = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        //US encoding to correctly format floating numbers
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
        QualitySettings.vSyncCount = 0; // VSynch disabled for targetFrameRate to be enabled
        Application.targetFrameRate = 60;
        // StartGame();
        layerMaskTracker = LayerMask.GetMask("Tracker");

    }

    public void StartGame()
    {
        FPSController.SetActive(true);
        currentMaxTrials = practiceTrials;
        canCastSpell = true;
        GenerateTrialOrder();
        orb_middle.SetActive(true);

        Collider targetCollider = orb_middle.GetComponent<Collider>();

        if (targetCollider != null)
        {
            targetBounds = targetCollider.bounds;
        }
    }

    void Update()
    {
        if (FPSController.activeSelf)
        {

            FPSController.GetComponentInChildren<FirstPersonLook>().updateFPSController();

            // Debug.Log(Input.mousePositionDelta);

            if (Input.GetMouseButtonDown(0) && !roundFinished)
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;



                if (Physics.Raycast(ray, out hit) && canCastSpell)
                {
                    GetComponent<animationmanager>().PlayAnimation();
                    GetComponent<soundmanager>().PlayShootSound();

                    // only orbs should are considered
                    if (hit.collider.CompareTag("orb"))
                    {

                        if (!trialRunning && hit.collider.name == "middleOrb")
                        {
                            // trial starts

                            StartTrial();
                        }
                        else if (trialRunning && hit.collider.name == "rightOrb" || trialRunning && hit.collider.name == "leftOrb")
                        {
                            // in trial shown target is hit
                            StopTrial();
                        }
                    }
                    else if (trialRunning && !hit.collider.CompareTag("orb"))
                    {
                        // in trial something else was hit
                        TrialFailed();
                    }
                }
            }
        }
    }

    private void GenerateTrialOrder()
    {
        // list with 30 left and 30 right orbs (not shuffled)
        trialOrder = new List<string>();
        for (int i = 0; i < currentMaxTrials / 2; i++)
        {
            trialOrder.Add("left");
            trialOrder.Add("right");
        }

        // shuffle trial order
        for (int i = trialOrder.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (trialOrder[randomIndex], trialOrder[i]) = (trialOrder[i], trialOrder[randomIndex]);
        }
    }

    private void StartTrial()
    {

        // get current Orb in order
        activeOrbStr = trialOrder[currentTrial];
        currentTrial++; // set index for next round


        // hide middle target
        orb_middle.SetActive(false);
        GetComponent<soundmanager>().PlayHitSound();

        // show lateral orb
        if (activeOrbStr == "right")
        {
            activeOrbObj = orb_right;
        }
        else if (activeOrbStr == "left")
        {
            activeOrbObj = orb_left;
        }

        isWaitingPhase = true;
        trackingStarted = true;

        StartCoroutine(preTargetStimulusTime());
    }

    private void StopTrial()
    {
        // stop RT
        endRT = Time.time;
        currRT = endRT - startRT;

        bool isDelayedOrb = CheckIfDelayedOrb();


        GetComponent<datamanager>().AddTrialToData(currentRound, currentTrial, mouse_x, mouse_y, activeOrbStr, isDelayedOrb ? longDelay : 0, startRT, endRT, currRT, 1);

        // update Points
        GetComponent<uimanager>().UpdateInGameUI(true, currRT, currentTrial, currentMaxTrials);

        StartCoroutine(HideOrbWithDelay(isDelayedOrb));
    }

    private bool CheckIfDelayedOrb()
    {
        if (version == "lld" && activeOrbStr == "left")
        {
            // long
            return true;
        }
        else if (version == "rld" && activeOrbStr == "right")
        {
            // long
            return true;
        }
        return false;
    }

    private IEnumerator HideOrbWithDelay(bool isDelayedOrb)
    {
        canCastSpell = false;
        if (isDelayedOrb)
        {
            yield return new WaitForSeconds(longDelay);
        }
        activeOrbObj.SetActive(false);
        GetComponent<soundmanager>().PlayHitSound();
        yield return new WaitForSeconds(stimulusTime);
        orb_middle.SetActive(true);
        trialRunning = false;
        trackingStarted = false;
        CheckTrial();
        canCastSpell = true;
    }

    private void TrialFailed()
    {

        endRT = Time.time;
        currRT = endRT - startRT;

        bool isDelayedOrb = CheckIfDelayedOrb();

        GetComponent<datamanager>().AddTrialToData(currentRound, currentTrial, mouse_x, mouse_y, activeOrbStr, isDelayedOrb ? longDelay : 0, startRT, endRT, currRT, 0);

        // update Points
        GetComponent<uimanager>().UpdateInGameUI(false, currRT, currentTrial, currentMaxTrials);

        // hide Orb 
        StartCoroutine(HideOrbWithDelay(CheckIfDelayedOrb()));
        //CheckTrial();
    }

    private void CheckTrial()
    {
        if (currentTrial >= currentMaxTrials)
        {
            roundFinished = true;
            if (currentRound < maxRounds)
            {
                GetComponent<uimanager>().PauseGame();

            }
            else
            {
                Debug.Log("GAME DONE");
                GetComponent<datamanager>().SendData();
                GameFinished();
            }
        }
    }

    public void resetRound()
    {
        currentMaxTrials = maxTrials; // now experiment trials
        GenerateTrialOrder();
        trialRunning = false;
        roundFinished = false;
        currentTrial = 0;
        currentRound++;
        // update Points
        GetComponent<uimanager>().UpdateInGameUI(false, currRT, currentTrial, currentMaxTrials);
    }

    public IEnumerator preTargetStimulusTime()
    {
        yield return new WaitForSeconds(stimulusTime);

        // trial setup
        activeOrbObj.SetActive(true);
        trialRunning = true;

        isWaitingPhase = false;
        currentPhase = "targeting";

        startRT = Time.time;
    }


    public void GameFinished()
    {
        FPSController.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }

}
