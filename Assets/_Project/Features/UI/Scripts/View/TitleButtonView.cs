using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public sealed class TitleButtonView : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    private static readonly int Selected = Animator.StringToHash("Selected");
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnSelect(BaseEventData eventData) => _animator.SetBool(Selected, true);

    public void OnDeselect(BaseEventData eventData) => _animator.SetBool(Selected, false);

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }
}
