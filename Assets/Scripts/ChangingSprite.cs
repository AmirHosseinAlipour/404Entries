using UnityEngine;

public class ChangingSprite : MonoBehaviour
{
    public Sprite boy_Sprite;
    public RuntimeAnimatorController boy_AnimatorController;
    public GameObject Player;

    private void Start()
    {
        if (SelectionPlayer.instance.PlayerID == 1)
        {
            // Change sprite
            Player.GetComponent<SpriteRenderer>().sprite = boy_Sprite;

            // Assign new animator controller
            Animator playerAnimator = Player.GetComponent<Animator>();
            if (playerAnimator != null && boy_AnimatorController != null)
            {
                playerAnimator.runtimeAnimatorController = boy_AnimatorController;
            }
        }
    }
}