using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



public interface IWindow
{
    void CloseUI();
    bool GetActive();
    UnityEvent GetOnCancelled();
}

public class GUIWindowScripts : MonoBehaviour, IWindow, IPointerDownHandler, IPointerUpHandler
{
    Vector2 mousePos;
    bool selected;
    public bool playSoundOnClose = true;

    public UnityEvent OnCancelled = new UnityEvent();

    // does a rudimentary check for if the player moves too far from the position they were in before
    protected bool exitOnPlayerRange = false;

    public UnityEvent GetOnCancelled()
    {
        return OnCancelled;
    }

    public virtual void CloseUI()
    {
        if ((transform && transform.parent && transform.parent.gameObject.activeSelf) ? transform.parent.gameObject.activeSelf : false)
        {
            if (playSoundOnClose)
            {
                AudioManager.PlayClipByID("clip_back", true);
            }

            transform.parent.gameObject.SetActive(false);
        }
    }

    public virtual void Activate()
    {
        transform.parent.gameObject.SetActive(true);
        GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
        PlayerViewScript.SetCurrentWindow(this);
        if (exitOnPlayerRange && PlayerCore.Instance)
        {
            lastPos = PlayerCore.Instance.transform.position;
        }
    }

    public virtual void ToggleActive()
    {
        bool active = transform.parent.gameObject.activeSelf;
        if (active)
        {
            CloseUI();
        }
        else
        {
            transform.parent.gameObject.SetActive(true);
            GetComponentInParent<Canvas>().sortingOrder = ++PlayerViewScript.currentLayer; // move window to top
            PlayerViewScript.SetCurrentWindow(this);
        }
    }

    public virtual bool GetActive()
    {
        return transform && transform.parent ? transform.parent.gameObject.activeSelf : false;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        mousePos = (Vector2)Input.mousePosition - GetComponent<RectTransform>().anchoredPosition;
        selected = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        selected = false;
    }

    public static int ExitRange = 100;
    Vector3? lastPos = null;

    protected virtual void Update()
    {
        if (exitOnPlayerRange && lastPos.HasValue &&
            Vector3.SqrMagnitude(PlayerCore.Instance.transform.position - lastPos.Value) > ExitRange)
        {
            CloseUI();
        }

        if (selected)
        {
            GetComponent<RectTransform>().anchoredPosition = (Vector2)Input.mousePosition - mousePos;
        }
    }
}
