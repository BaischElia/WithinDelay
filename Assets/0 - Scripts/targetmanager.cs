using UnityEngine;

public class targetmanager : MonoBehaviour
{
        public GameObject orb_middle;
        public GameObject orb_right;
        public GameObject orb_left;

        private GameObject activeOrbObj;
        private string activeOrbStr;

        public string getActiveOrbString() => activeOrbStr;
        public GameObject getActiveOrbObject() => activeOrbObj;

        // private trialmanager trialManager;

        void Start()
        {
        //   trialManager = GetComponent<trialmanager>();
        //   trialManager.OnTrialStarted += HandleTrialStart;      
        }

        public void ShowMiddleOrb()
        {
                orb_middle.SetActive(true);
                activeOrbObj = orb_middle;
                activeOrbStr = "middle";
        }

        public void ShowTargetOrb(string side)
        {
                activeOrbStr = side;
                if (side == "right")
                {
                        orb_right.SetActive(true);
                        activeOrbObj = orb_right;
                }
                else if (side == "left")
                {
                        orb_left.SetActive(true);
                        activeOrbObj = orb_left;
                }
        }

        public void HideAllOrbs()
        {
                orb_middle.SetActive(false);
                orb_right.SetActive(false);
                orb_left.SetActive(false);
        }

        void HandleTrialStart(string side)
        {
                // HideMiddleOrb();
                ShowTargetOrb(side);
        }


}