using UnityEngine;

public class ChangingSprite : MonoBehaviour
{
    public Sprite boy_Sprite;
    public RuntimeAnimatorController boy_AnimatorController;
    public GameObject Player;

    private void Start()
    {
        if (SelectionPlayer.instance != null && SelectionPlayer.instance.PlayerID == 1)
        {
            if (Player != null)
            {
                // Change sprite
                SpriteRenderer sr = Player.GetComponent<SpriteRenderer>();
                if (sr != null && boy_Sprite != null)
                {
                    sr.sprite = boy_Sprite;
                }

                // Assign new animator controller
                Animator playerAnimator = Player.GetComponent<Animator>();
                if (playerAnimator != null && boy_AnimatorController != null)
                {
                    playerAnimator.runtimeAnimatorController = boy_AnimatorController;
                }
            }
        }
    }
}