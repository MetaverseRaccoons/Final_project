using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject obj;
    public bool notActiveAtStart;
    public bool onSelect;
    public bool onPointerEnter;

    void Start()
    {
        obj.SetActive(!notActiveAtStart);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (obj != null && onSelect)
        {
            obj.SetActive(false);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (obj != null && onSelect)
        {
            obj.SetActive(true);
        }
    }  

    public void SetActive()
    {
        obj.SetActive(!obj.activeInHierarchy);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (obj != null && onPointerEnter)
        {
            obj.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (obj != null && onPointerEnter)
        {
            obj.SetActive(false);
        }
    }
}
