using UnityEngine;
using UnityEngine.UI;

public class DialogueAnimator : MonoBehaviour
{
    public Animator boxAnim;
    public DialogueManager dm;

    public void OnTriggerEnter2D(Collider2D other)
    {
        boxAnim.SetBool("startOpen", true);
    }
    public void OnTriggerExit2D(Collider2D other)
    {
        boxAnim.SetBool("startOpen", false);
        dm.EndDialogue();
    }
}
