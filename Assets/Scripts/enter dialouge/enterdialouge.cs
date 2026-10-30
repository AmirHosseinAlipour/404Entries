using System;
using UnityEngine;

public class enterdialouge : MonoBehaviour
{
    public RectTransform dialouge;
    public FarsiTypewriter dialougeFtw;

    private SpriteRenderer _npcSr;
    private GameObject _promptObj;
    private SpriteRenderer _promptSr;
    private bool _playerInRange = false;

    public bool IsDialogueActive => dialouge != null && dialouge.gameObject.activeInHierarchy;

    private void Start()
    {
        _npcSr = GetComponent<SpriteRenderer>();
        if (_npcSr == null) _npcSr = GetComponentInChildren<SpriteRenderer>();

        if (dialouge == null)
        {
            RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
            foreach (var r in rects)
            {
                if (r != null && r.gameObject.name.ToLower().Contains("person"))
                {
                    dialouge = r;
                    break;
                }
            }
        }

        if (dialougeFtw == null && dialouge != null)
        {
            dialougeFtw = dialouge.GetComponentInChildren<FarsiTypewriter>(true);
        }

        if (dialouge != null && dialouge.gameObject.activeSelf)
        {
            dialouge.gameObject.SetActive(false);
        }

        CreateOverheadPrompt();
    }

    private void CreateOverheadPrompt()
    {
        if (_promptObj != null) return;

        _promptObj = new GameObject("Overhead_E_Prompt");
        _promptObj.transform.SetParent(transform, false);

        _promptSr = _promptObj.AddComponent<SpriteRenderer>();
        _promptSr.sprite = NPCInteractUI.EPromptSprite;

        if (_npcSr != null)
        {
            _promptSr.sortingLayerID = _npcSr.sortingLayerID;
            _promptSr.sortingOrder = _npcSr.sortingOrder + 10;
        }
        else
        {
            _promptSr.sortingOrder = 100;
        }

        // Normalize initial scale
        Vector3 parentLossy = transform.lossyScale;
        float invX = parentLossy.x != 0 ? 0.45f / Mathf.Abs(parentLossy.x) : 0.45f;
        float invY = parentLossy.y != 0 ? 0.45f / Mathf.Abs(parentLossy.y) : 0.45f;
        _promptObj.transform.localScale = new Vector3(invX, invY, 1f);

        Color c = _promptSr.color;
        c.a = 0f;
        _promptSr.color = c;
        _promptObj.SetActive(false);
    }

    private void Update()
    {
        if (GetComponent<DialogueSequenceTrigger>() != null) return;

        bool shouldShow = _playerInRange && !IsDialogueActive;

        if (shouldShow)
        {
            if (_promptObj != null)
            {
                if (!_promptObj.activeSelf)
                {
                    _promptObj.SetActive(true);
                }

                // Keep scale normalized against NPC flipping/scaling
                Vector3 parentLossy = transform.lossyScale;
                float invX = parentLossy.x != 0 ? 0.45f / Mathf.Abs(parentLossy.x) : 0.45f;
                float invY = parentLossy.y != 0 ? 0.45f / Mathf.Abs(parentLossy.y) : 0.45f;
                _promptObj.transform.localScale = new Vector3(invX, invY, 1f);

                // World position above NPC head with gentle sine wave bobbing
                float topY = _npcSr != null ? _npcSr.bounds.max.y + 0.35f : transform.position.y + 1.4f;
                float bob = Mathf.Sin(Time.time * 4.5f) * 0.08f;
                _promptObj.transform.position = new Vector3(transform.position.x, topY + bob, 0f);

                // Smooth fade in
                Color c = _promptSr.color;
                c.a = Mathf.MoveTowards(c.a, 1f, Time.deltaTime * 6f);
                _promptSr.color = c;
            }
        }
        else
        {
            if (_promptObj != null && _promptObj.activeSelf)
            {
                Color c = _promptSr.color;
                c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * 8f);
                _promptSr.color = c;

                if (c.a <= 0.01f)
                {
                    _promptObj.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (GetComponent<DialogueSequenceTrigger>() != null) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;
        NPCInteractUI.Register(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (GetComponent<DialogueSequenceTrigger>() != null) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
        NPCInteractUI.Unregister(this);

        if (IsDialogueActive)
        {
            CloseDialogue();
        }
    }

    public void TriggerInteraction()
    {
        if (IsDialogueActive)
        {
            CloseDialogue();
        }
        else
        {
            OpenDialogue();
        }
    }

    public void OpenDialogue()
    {
        if (dialouge == null) return;

        DialogueSequenceTrigger.EnsureDialogueSortingOrder(dialouge);
        UIAnimationManager.Instance.ShowDialogueWindow(dialouge, 0.5f, dialougeFtw);
    }

    public void CloseDialogue()
    {
        if (dialouge == null) return;

        UIAnimationManager.Instance.HideWindow(dialouge, 0.4f);
    }

    private void OnDisable()
    {
        _playerInRange = false;
        NPCInteractUI.Unregister(this);
        if (_promptObj != null)
        {
            _promptObj.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        _playerInRange = false;
        NPCInteractUI.Unregister(this);
    }
}
