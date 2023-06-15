using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class SuperCellButton : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerDownHandler
{
    const float DURATION = 0.125f;
    const float SCALE_DOWN = 0.85f;
    const float SCALE_UP = 1f;
    bool m_clickedFlag;
    Selectable m_uiSelectable;


    private void Awake()
    {
        m_isDestroyed = false;
        m_clickedFlag = false;
        m_uiSelectable = GetComponent<Selectable>();
    }

    private void OnDisable()
    {
        DOTween.Kill(gameObject, true);
    }

    private void OnEnable()
    {
        this.transform.localScale = Vector3.one;
    }

    public void DoAnimation()
    {
        if (m_isDestroyed) return;

        DOTween.Kill(gameObject, true);
        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * SCALE_DOWN, DURATION * 0.5f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(Vector3.one * SCALE_UP, DURATION * 1.25f).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(Vector3.one, DURATION * 0.75f).SetEase(Ease.OutQuad));
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_isDestroyed) return;
        if (m_uiSelectable != null && !m_uiSelectable.interactable) return;
        //Debug.LogError("OnPointerClick");
        m_clickedFlag = true;
        DoAnimation();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_isDestroyed) return;
        if (m_uiSelectable != null && !m_uiSelectable.interactable) return;

        DOTween.Kill(gameObject, true);
        transform.DOScale(Vector3.one * SCALE_DOWN, DURATION * 0.5f).SetEase(Ease.OutQuad);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_isDestroyed) return;
        //Debug.LogError("OnPointerExit");
        if (!m_clickedFlag)
        {
            DOTween.Kill(gameObject, true);
            this.transform.localScale = Vector3.one;
        }
        m_clickedFlag = false;
    }

    bool m_isDestroyed = false;
    void OnDestroy()
    {
        m_isDestroyed = true;
        DOTween.Kill(gameObject, true);
    }
}