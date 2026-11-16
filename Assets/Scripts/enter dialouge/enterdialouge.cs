using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class enterdialouge : MonoBehaviour
{
    public RectTransform dialouge;
    public FarsiTypewriter dialougeFtw;

    private SpriteRenderer _npcSr;
    private GameObject _promptObj;
    private SpriteRenderer _promptSr;
    private bool _playerInRange = false;

    private float _promptScaleMultiplier = 1f;
    private Tween _scaleTween;
    private int _lastTappedFrame = -1;

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

                // Keep scale normalized against NPC flipping/scaling, with scale multiplier for punch effect
                Vector3 parentLossy = transform.lossyScale;
                float invX = parentLossy.x != 0 ? 0.45f / Mathf.Abs(parentLossy.x) : 0.45f;
                float invY = parentLossy.y != 0 ? 0.45f / Mathf.Abs(parentLossy.y) : 0.45f;
                _promptObj.transform.localScale = new Vector3(invX, invY, 1f) * _promptScaleMultiplier;

                // World position above NPC head with gentle sine wave bobbing
                float topY = _npcSr != null ? _npcSr.bounds.max.y + 0.35f : transform.position.y + 1.4f;
                float bob = Mathf.Sin(Time.time * 4.5f) * 0.08f;
                _promptObj.transform.position = new Vector3(transform.position.x, topY + bob, 0f);

                // Smooth fade in
                Color c = _promptSr.color;
                c.a = Mathf.MoveTowards(c.a, 1f, Time.deltaTime * 6f);
                _promptSr.color = c;
            }

            // Check touch on mobile or click on PC for this overhead prompt / NPC
            CheckTapInteraction();
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

    private void CheckTapInteraction()
    {
        if (!_playerInRange || IsDialogueActive || _promptObj == null || !_promptObj.activeSelf)
            return;

        if (_promptSr != null && _promptSr.color.a < 0.2f)
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Mobile Touch Input (supports multi-touch)
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                UnityEngine.Touch touch = Input.GetTouch(i);
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    if (IsPointerOverUI(touch.fingerId)) continue;

                    Vector3 worldPos = cam.ScreenToWorldPoint(touch.position);
                    worldPos.z = 0f;

                    if (IsTouchHittingPromptOrNPC(worldPos))
                    {
                        OnPromptTapped();
                        return;
                    }
                }
            }
        }

        // 2. Mouse Click (Desktop / Unity Editor)
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI(-1))
            {
                Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0f;

                if (IsTouchHittingPromptOrNPC(worldPos))
                {
                    OnPromptTapped();
                    return;
                }
            }
        }

#if ENABLE_INPUT_SYSTEM
        // 3. New Input System Touchscreen
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                if (t.press.wasPressedThisFrame)
                {
                    int fingerId = t.touchId.ReadValue();
                    if (IsPointerOverUI(fingerId)) continue;

                    Vector2 screenPos = t.position.ReadValue();
                    Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
                    worldPos.z = 0f;

                    if (IsTouchHittingPromptOrNPC(worldPos))
                    {
                        OnPromptTapped();
                        return;
                    }
                }
            }
        }

        // 4. New Input System Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverUI(-1))
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
                worldPos.z = 0f;

                if (IsTouchHittingPromptOrNPC(worldPos))
                {
                    OnPromptTapped();
                    return;
                }
            }
        }
#endif
    }

    private bool IsPointerOverUI(int fingerId)
    {
        if (EventSystem.current == null) return false;
        if (fingerId >= 0)
        {
            return EventSystem.current.IsPointerOverGameObject(fingerId);
        }
        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsTouchHittingPromptOrNPC(Vector3 worldPos)
    {
        // 1. Direct hit on the overhead E prompt (generous radius for mobile touch)
        if (_promptObj != null)
        {
            Vector2 promptPos = _promptObj.transform.position;
            if (Vector2.Distance(worldPos, promptPos) <= 0.95f)
            {
                return true;
            }
        }

        // 2. Direct hit on NPC sprite bounds
        if (_npcSr != null)
        {
            Bounds b = _npcSr.bounds;
            b.Expand(0.35f);
            if (b.Contains(worldPos))
            {
                return true;
            }
        }
        else
        {
            if (Vector2.Distance(worldPos, transform.position) <= 0.95f)
            {
                return true;
            }
        }

        return false;
    }

    private void OnPromptTapped()
    {
        if (Time.frameCount == _lastTappedFrame) return;
        _lastTappedFrame = Time.frameCount;

        PunchOverheadPrompt();
        TriggerInteraction();
    }

    private void PunchOverheadPrompt()
    {
        if (_scaleTween != null) _scaleTween.Kill();
        _promptScaleMultiplier = 1.35f;
        _scaleTween = DOTween.To(() => _promptScaleMultiplier, x => _promptScaleMultiplier = x, 1f, 0.25f)
            .SetEase(Ease.OutBack);
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
        if (_scaleTween != null)
        {
            _scaleTween.Kill();
            _scaleTween = null;
        }
        _promptScaleMultiplier = 1f;
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
