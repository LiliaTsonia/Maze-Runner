using Common.Enums;
using Common.EventsSystem;
using Common.ServiceLocator;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MazeExit : MonoBehaviour
{
    [SerializeField]
    private BoxCollider2D _collider;

    private EventManager _eventManager;

    public void Init(Transform parent, ref int num, ref Vector2 size)
    {
        _eventManager = ServiceLocator.LocateService<EventManager>();

        name = $"Exit_{num}";
        transform.SetParent(parent);
        _collider.size = size;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        _eventManager.GetEvent(EGameEvent.Win).Invoke();
    }

    public void Hide()
    {
#if UNITY_EDITOR
        if (this != null)
        {
            DestroyImmediate(gameObject);
        }
#else
        Destroy(gameObject);
#endif
    }
}
