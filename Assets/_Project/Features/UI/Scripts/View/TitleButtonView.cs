using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public sealed class TitleButtonView : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    private static readonly int SelectedParameter = Animator.StringToHash("Selected");
    private Animator _animator;

    public event Action<TitleButtonView> Selected;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _animator.SetBool(SelectedParameter, true);
        Selected?.Invoke(this);
    }

    public void OnDeselect(BaseEventData eventData) => _animator.SetBool(SelectedParameter, false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }
}
