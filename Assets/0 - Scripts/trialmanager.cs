using UnityEngine;
using System.Collections.Generic;

public class trialmanager : MonoBehaviour
{
        // important stuff for the trial
        public int maxTrials = 60;
        public int maxRounds = 8;
        public int practiceTrials = 10;
        public string version = "lld";
        public float longDelay = 0.6f;
        public float shortDelay = 0f;
        public float stimulusTime = 0.4f;

        // current state of things of the trial for other scripts to access
        public string currentPhase = "waitingForTarget";
        public bool isTrialRunning = false;
        public bool isWaitingPhase = false;
        public bool isTrackingRunning = false;
        public bool isRoundFinished = false;
        public List<string> trialOrder;

        // current count of things
        public int currentTrial = 0;
        public int currentRound = 0;
        public int currentMaxTrials;
        private string currentOrbStr;

        // event stuff
        public delegate void TrialStartedDelegate(string activeOrb);
        public event TrialStartedDelegate OnTrialStarted;
        public delegate void TrialEndedDelegate(string activeOrb);
        public event TrialEndedDelegate OnTrialEnded;
        public delegate void RoundEndedDelegate();
        public event RoundEndedDelegate OnRoundEnded;
        public delegate void GameEndedDelegate();
        public event GameEndedDelegate OnGameEnded;

        private targetmanager targetManager;
        private trackingmanager trackingManager;
        private fpscounter fpsCounter;

        public void Start()
        {
                // get the scriptmanagers
                targetManager = GetComponent<targetmanager>();
                trackingManager = GetComponent<trackingmanager>();
                fpsCounter = GetComponent<fpscounter>();
        }

        // sets trials for practice round
        public void StartPractice()
        {
                currentMaxTrials = practiceTrials;
                GenerateTrialOrder();
                currentTrial = 0;
                currentRound = 0;
                ResetTrialState();
                targetManager.ShowMiddleOrb();
                fpsCounter.StartFPSTest(); // start fps test for practice round for 15 seconds
        }

        // sets trials for experiment rounds
        public void StartNewRound()
        {
                currentMaxTrials = maxTrials; // 60 trials per round
                GenerateTrialOrder();
                // isTrialRunning = false;
                currentTrial = 0;
                // currentRound++;
                ResetTrialState();
                targetManager.ShowMiddleOrb();
        }

        private void ResetTrialState()
        {
                isTrialRunning = false;
                isRoundFinished = false;
                // isWaitingPhase = false;
                currentPhase = "waitingForTarget";
        }

        // creates a randomized trial order
        public void GenerateTrialOrder()
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

        // called in mainmanager with hit of middle target
        public void PrepareTrial()
        {

                if (currentTrial >= currentMaxTrials) return; // maximum trials in round reached

                isTrialRunning = true;
                // isWaitingPhase = false;
                // isTrackingRunning = true;
                // currentPhase = "activeTargeting";

                currentOrbStr = trialOrder[currentTrial];
                currentTrial++;

                OnTrialStarted?.Invoke(currentOrbStr); // start trial with next side target
                
        }

        // called in mainmanager after first in in running trial
        // after disappearing of side target (see HideOrbWithDelay())
        public void StopTrial()
        {
                isTrialRunning = false;
                
                // starts end-trial-event
                OnTrialEnded?.Invoke(currentOrbStr);

                // isTrackingRunning = false;
                currentPhase = "waitingForTarget";

                // string activeOrb = trialOrder[currentTrial];


                // check if round and/or game is finished
                if (currentTrial >= currentMaxTrials)
                {
                        isRoundFinished = true;
                        currentRound++;
                        if (currentRound > maxRounds)
                        {
                                OnGameEnded?.Invoke(); // game is finished
                        }
                        else
                        {
                                // currentRound++;
                                OnRoundEnded?.Invoke(); // round is finished
                        }
                }


        }

        // returns delay for the current orb
        // comment and decomment for whatever version (extented or persistence) you need
        public float GetDelay(string activeOrbStr)
        {
                // persistence version
                // if (currentRound > 8) return shortDelay; // short delay for all trials after 8 rounds
                // else if (version == "lld" && activeOrbStr == "left") return longDelay;
                // if (currentRound > 8) return shortDelay; // short delay for all trials after 8 rounds

                // extended version
                if (version == "lld" && activeOrbStr == "left") return longDelay;
                else if (version == "rld" && activeOrbStr == "right") return longDelay;
                return shortDelay; // short
        }

        public void UpdateTrackingPhase(string phase)
        {
                currentPhase = phase;
        }

        // returns current string of side orb
        public string GetCurrentOrbName()
        {

                return currentOrbStr;
        }
}