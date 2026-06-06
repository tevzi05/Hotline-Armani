using UnityEngine;

public class DialogueActivator : MonoBehaviour
{
    private GameObject dialogueMenu;
    private GameObject crosshair;

    void Start()
    {
        Transform dialogueTransform = transform.Find("DialogueBars");
        if (dialogueTransform != null)
        {
            dialogueMenu = dialogueTransform.gameObject;
            dialogueMenu.SetActive(false);
        }

        crosshair = GameObject.FindGameObjectWithTag("Crosshair");
    }


    // Âûçûâàåì, ÊÎÃÄÀ ÄÈÀËÎÃ ÍÀ×ÈÍÀÅÒÑß
    public void DialogueStart()
    {
        if (dialogueMenu != null) dialogueMenu.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);
        Cursor.visible = true;
    }

    // ÂÛÇÛÂÀÅÌ, ÊÎÃÄÀ ÄÈÀËÎÃ ÎÊÎÍ×ÅÍ
    public void DialogueEnd()
    {
        if (crosshair != null) crosshair.SetActive(true);
        Cursor.visible = false;
    }
}
