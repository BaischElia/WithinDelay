using System;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;

public class trackingmanager : MonoBehaviour
{
        public LayerMask trackingLayer;
        private float mouseX;
        private float mouseY;

        public TrackingPhase currentPhase = TrackingPhase.startClick;
        private trialmanager trialManager;
        public bool isTrackingMouseData = false;

        private StringBuilder trackingDataBuilder = new StringBuilder();
        private string trackingDataHeader = "id, round, trial, version, timestamp, frame, relativeTime, pointerX, pointerY, mouseDX, mouseDY, phase, event;";

        public enum TrackingPhase
        {
                startClick,
                waitingForTarget,
                activeTracking,
                onTarget,
                endClick
        }

        public enum EventTrigger
        {
                noEvent,
                middleTargetClick,
                targetAppeared,
                targetClick,
                failedClick
        }

        private string pID;
        private string pVersion;
        private float startTrialTime = 0f;
        private float relativeTime = 0f;
        private float maxCountingTime = 4f; // we exclude trials that take longer than 4 seconds

        [DllImport("__Internal")]
        private static extern void receiveTrackingData(string data);

        void Start()
        {
                trackingLayer = LayerMask.GetMask("Tracker");
                trialManager = GetComponent<trialmanager>();
                ResetTrackingData();
        }

        public void updateMouseTracking(Vector2 mouseDelta, EventTrigger trigger = EventTrigger.noEvent)
        {
                if (!isTrackingMouseData) return;

                if (trigger == EventTrigger.targetAppeared) // start of trial as start of max seconds
                {
                        startTrialTime = Time.time;
                        relativeTime = 0f; // reset relative time
                }

                relativeTime += Time.deltaTime; // update relative time

                if (relativeTime >= maxCountingTime) // if trial takes longer than 4s we stop tracking
                {
                        // Debug.Log("STOP");
                        return;
                } 

                RaycastHit[] hits;
                hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 100f, trackingLayer);

                bool orbHit = false;

                foreach (RaycastHit hit in hits)
                {
                        // Debug.Log("Hit: " + hit.collider.name);
                        if (hit.collider.CompareTag("orb"))
                        {
                                orbHit = true;
                                currentPhase = TrackingPhase.onTarget;
                        }
                        else
                        {
                                Vector3 hitPosition = hit.point;
                                mouseX = hitPosition.x;
                                mouseY = hitPosition.y;
                        }
                }

                if (!orbHit && !trialManager.isWaitingPhase)
                {
                        currentPhase = TrackingPhase.activeTracking;
                }
                else if (trialManager.isWaitingPhase)
                {
                        currentPhase = TrackingPhase.waitingForTarget;
                }

                AddTrackingData(trialManager.currentRound, trialManager.currentTrial, mouseDelta, trigger);

        }

        public string GetTrackingData()
        {
                return trackingDataBuilder.ToString();
        }

        public void ResetTrackingData()
        {
                trackingDataBuilder.Clear();
                // trackingDataBuilder.Append(trackingDataHeader);
        }


        private void AddTrackingData(int round, int trial, Vector2 mouseDelta, EventTrigger trigger)
        {
                // "id, round, trial, version, frame, time, pointerX, pointerY, deltaX, deltaY, phase;";

                string lastTrackingEntry =
                        $"{pID}," +
                        $"{round}," +
                        $"{trial}," +
                        $"{pVersion}," +
                        $"{DateTime.Now:HH:mm:ss.fff}," +
                        $"{Time.frameCount}," +
                        $"{relativeTime:F3}," +
                        $"{mouseX:F3}," +
                        $"{mouseY:F3}," +
                        $"{mouseDelta.x:F3}," +
                        $"{mouseDelta.y:F3}," +
                        $"{currentPhase}," +
                        $"{trigger};";

                trackingDataBuilder.AppendLine(lastTrackingEntry); // Keep history if needed
                // Debug.Log($"Last Entry: {lastTrackingEntry}"); // Log only the latest
        }

        public void SetInitData(string id, string version)
        {
                pID = id;
                pVersion = version;
        }

        public Vector2 GetMousePosition()
        {
                return new Vector2(mouseX, mouseY);
        }

        public void SendDataToJS()
        {
                // Javascript function call to send data
#if UNITY_WEBGL && !UNITY_EDITOR
        receiveTrackingData(trackingDataBuilder.ToString());
#endif
                ResetTrackingData();
        }

}