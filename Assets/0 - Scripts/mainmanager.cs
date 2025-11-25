using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Data.Common;
using System.Numerics;

public class mainmanager : MonoBehaviour
{
        public Camera playerCamera;
        public GameObject FPSController;

        // all the scripts
        private animationmanager animationManager;
        private soundmanager soundManager;
        private datamanager dataManager;
        private uimanager uiManager;
        private trialmanager trialManager;
        private trackingmanager trackingManager;
        private targetmanager targetManager;

        private GameObject activeOrbObj;
        private float startRT;
        private float endRT;
        private float currRT;
        // private float mouse_x;
        // private float mouse_y;
        private bool canCastSpell = false; // assure that the player can only cast a spell once per click
        private bool eventTriggered = false;
        private UnityEngine.Vector2 mouseDelta;
        private bool targetAppeared = false;


        void Start()
        {
                // get the scriptmanagers
                animationManager = GetComponent<animationmanager>();
                soundManager = GetComponent<soundmanager>();
                dataManager = GetComponent<datamanager>();
                uiManager = GetComponent<uimanager>();
                trialManager = GetComponent<trialmanager>();
                targetManager = GetComponent<targetmanager>();
                trackingManager = GetComponent<trackingmanager>();


                // register to events
                trialManager.OnTrialStarted += HandleTrialStart;
                trialManager.OnTrialEnded += HandleTrialEnd;
                trialManager.OnRoundEnded += HandleRoundEnd;
                trialManager.OnGameEnded += HandleGameEnd;


                // setup for the game
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                // QualitySettings.vSyncCount = 1; 
                // Application.targetFrameRate = 60;

        }

        // called from the start screen after password check
        public void StartGame()
        {
                FPSController.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                canCastSpell = true;
                trackingManager.SetInitData(dataManager.GetID(), trialManager.version);
                trialManager.StartPractice();
        }

        void Update()
        {
                if (!FPSController.activeSelf) return; // no FPSController = start screen

                mouseDelta = FPSController.GetComponentInChildren<FirstPersonLook>().updateFPSController(); // rotate camera

                // trackingManager.updateMouseTracking(); // update mouse tracking

                if (Input.GetMouseButtonDown(0) && !trialManager.isRoundFinished) // clicks only counts if no start or pause screen
                {
                        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

                        if (Physics.Raycast(ray, out RaycastHit hit) && canCastSpell)
                        {
                                animationManager.PlayAnimation();
                                soundManager.PlayShootSound();

                                if (hit.collider.CompareTag("orb"))
                                {
                                        if (!trialManager.isTrialRunning && hit.collider.name == "middleOrb") // middle target hit, trial starts
                                        {
                                                // sets up the trial and calls start-event
                                                trialManager.PrepareTrial();
                                                eventTriggered = true;
                                                trackingManager.updateMouseTracking(mouseDelta, trackingmanager.EventTrigger.middleTargetClick);
                                        }
                                        else if (trialManager.isTrialRunning && (hit.collider.name == "rightOrb" || trialManager.isTrialRunning && hit.collider.name == "leftOrb")) // side target hit, trial stops
                                        {
                                                StopTrial(true); // hit success
                                        }
                                }
                                else if (trialManager.isTrialRunning && !hit.collider.CompareTag("orb")) // hit no traget, but only counts if trial is running
                                {
                                        StopTrial(false); // hit fail
                                }
                        }
                }
                if (targetAppeared)
                {
                        trackingManager.updateMouseTracking(mouseDelta, trackingmanager.EventTrigger.targetAppeared);
                        targetAppeared = false;
                }
                else if (!eventTriggered)
                {
                        trackingManager.updateMouseTracking(mouseDelta); // update mouse tracking without triggered Event    
                }
                eventTriggered = false;

        }


        // called with first hit after middle target was hit
        // hitSuccess = true if the player hit the side target
        void StopTrial(bool hitSuccess)
        {
                endRT = Time.time;
                currRT = endRT - startRT;

                string activeOrb = trialManager.GetCurrentOrbName();
                float delay = trialManager.GetDelay(activeOrb);


                eventTriggered = true;
                if (hitSuccess)
                {
                        trackingManager.updateMouseTracking(mouseDelta, trackingmanager.EventTrigger.targetClick);
                }
                else
                {
                        trackingManager.updateMouseTracking(mouseDelta, trackingmanager.EventTrigger.failedClick);
                }

                trackingManager.currentPhase = trackingmanager.TrackingPhase.endClick; // set tracking phase to end click
                trackingManager.isTrackingMouseData = false; // stop mouse tracking

                dataManager.AddTrialToData(
                        trialManager.currentRound,
                        trialManager.currentTrial,
                        trackingManager.GetMousePosition().x,
                        trackingManager.GetMousePosition().y, // TODO 
                        activeOrb,
                        delay,
                        startRT,
                        endRT,
                        currRT,
                        hitSuccess ? 1 : 0);

                // Debug.Log($"Hit: {hitSuccess}, RT: {currRT}, Delay: {delay}");

                uiManager.UpdateInGameUI(true, currRT, trialManager.currentTrial, trialManager.currentMaxTrials);

                StartCoroutine(HideOrbWithDelay(delay));
        }

        // called in StopTrial() with first hit after middle target
        public IEnumerator HideOrbWithDelay(float delay)
        {
                canCastSpell = false;
                yield return new WaitForSeconds(delay);
                targetManager.HideAllOrbs();
                soundManager.PlayHitSound();

                // then wait for stimulus time?
                yield return new WaitForSeconds(trialManager.stimulusTime);

                trialManager.StopTrial();

                canCastSpell = true;

                if (trialManager.currentTrial < trialManager.currentMaxTrials)
                {
                        targetManager.ShowMiddleOrb();
                }

        }

        // gets called with trialmanager.StartTrial()
        // happens when middle target is hit

        void HandleTrialStart(string activeOrb)
        {
                // NOTE: tracking test
                canCastSpell = false;
                trackingManager.isTrackingMouseData = true; // start mouse tracking

                targetManager.HideAllOrbs();
                soundManager.PlayHitSound();
                StartCoroutine(PreTargetStimulusTime(activeOrb));
                
        }

        public IEnumerator PreTargetStimulusTime(string activeOrb)
        {
                trialManager.isWaitingPhase = true;             

                trackingManager.currentPhase = trackingmanager.TrackingPhase.waitingForTarget; // set tracking phase to waiting for target

                yield return new WaitForSeconds(trialManager.stimulusTime);


                canCastSpell = true;

                targetManager.ShowTargetOrb(activeOrb);
                trialManager.isWaitingPhase = false;

                targetAppeared = true;
                startRT = Time.time;
        }

        // gets called with hiding of side target
        private void HandleTrialEnd(string activeOrb)
        {
                // here happens nothing right now
                // maybe handle mouse tracking here

        }

        // called when last trial of each round is finished
        // except after last trial of last round => HandleGameEnded()
        private void HandleRoundEnd()
        {
                trackingManager.SendDataToJS();
                uiManager.PauseGame();
                if (trialManager.currentRound > 8) // halfway
                {
                        dataManager.SendData(); // send mid-game data to JS
                }
        }

        // called after last trial of the last round
        private void HandleGameEnd()
        {
                FPSController.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                trackingManager.SendDataToJS();
                dataManager.SendSecondData(); // save data and start JS-functions
        }

        public void ResetRound()
        {
                canCastSpell = true;
        }

}


