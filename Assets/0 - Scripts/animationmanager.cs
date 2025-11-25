using UnityEngine;

public class animationmanager : MonoBehaviour
{
    public GameObject playerHands;
    public void PlayAnimation() {
        playerHands.GetComponent<Animator>().SetTrigger("SpellCast");
    }
}
