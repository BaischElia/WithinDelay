using UnityEngine;
using System.Collections.Generic;

public class trialmanager : MonoBehaviour
{
        // important stuff for the trial
        public int maxTrials = 60;
        public int maxRounds = 8;
        public int practiceTrials = 10;
        public float longDelay = 0.6f;
        public float shortDelay = 0f;
        public float stimulusTime = 0.4f;

        // current state of things of the trial for other scripts to access
        public string currentPhase = "waitingForTarget";
        public bool isTrialRunning = false;
        public bool isWaitingPhase = false;
        public bool isTrackingRunning = false;
        public bool isRoundFinished = false;
        public List<bool> delayOrder;

        // current count of things
        public int currentTrial = 0;
        public int currentRound = 0;
        public int currentMaxTrials;
        
        // properties of the current trial
        private bool currentTrialIsDelayed;
        private int currentTrialPosition;

        // event stuff
        public delegate void TrialStartedDelegate(int position, bool isDelayed);
        public event TrialStartedDelegate OnTrialStarted;
        public delegate void TrialEndedDelegate(int position, bool isDelayed);
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
                currentTrial = 0;
                ResetTrialState();
                targetManager.ShowMiddleOrb();
        }

        private void ResetTrialState()
        {
                isTrialRunning = false;
                isRoundFinished = false;
                currentPhase = "waitingForTarget";
        }

        // creates a randomized trial order
        public void GenerateTrialOrder()
        {
                // list with 30 delayed and 30 not-delayed trials (not shuffled)
                delayOrder = new List<bool>();
                for (int i = 0; i < currentMaxTrials / 2; i++)
                {
                        delayOrder.Add(true); // delayed
                        delayOrder.Add(false); // not delayed
                }

                // shuffle trial order
                for (int i = delayOrder.Count - 1; i > 0; i--)
                {
                        int randomIndex = UnityEngine.Random.Range(0, i + 1);
                        (delayOrder[randomIndex], delayOrder[i]) = (delayOrder[i], delayOrder[randomIndex]);
                }
        }

        // called in mainmanager with hit of middle target
        public void PrepareTrial()
        {

                if (currentTrial >= currentMaxTrials) return; // maximum trials in round reached

                isTrialRunning = true;

                currentTrialIsDelayed = delayOrder[currentTrial];
                currentTrialPosition = UnityEngine.Random.Range(0, 12); // 0-11 for 12 positions

                currentTrial++;

                OnTrialStarted?.Invoke(currentTrialPosition, currentTrialIsDelayed); // start trial with next target
                
        }

        // called in mainmanager after first hit in running trial
        // after disappearing of side target (see HideOrbWithDelay())
        public void StopTrial()
        {
                isTrialRunning = false;
                
                // starts end-trial-event
                OnTrialEnded?.Invoke(currentTrialPosition, currentTrialIsDelayed);

                currentPhase = "waitingForTarget";

                // check if round and/or game is finished
                if (currentTrial >= currentMaxTrials)
                {
                        isRoundFinished = true;
                        currentRound++;
                        if (currentRound >= maxRounds)
                        {
                                OnGameEnded?.Invoke(); // game is finished
                        }
                        else
                        {
                                OnRoundEnded?.Invoke(); // round is finished
                        }
                }
        }

        // returns the delay time for the current trial
        public float GetCurrentDelay()
        {
                return currentTrialIsDelayed ? longDelay : shortDelay;
        }

        public bool IsCurrentTrialDelayed()
        {
                return currentTrialIsDelayed;
        }
        
        public int GetCurrentTrialPosition()
        {
                return currentTrialPosition;
        }

        public void UpdateTrackingPhase(string phase)
        {
                currentPhase = phase;
        }
}