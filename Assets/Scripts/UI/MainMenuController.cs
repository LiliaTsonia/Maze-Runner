using Common.SceneManagement;
using Common.ServiceLocator;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] 
    private Button _playButton;
    
    private Tweener _pulseSequence;
    
    private SceneLoader _sceneLoader;

    private void Start()
    {
        Init();
        _playButton.onClick.AddListener(OnPlayBtnClick);
        _pulseSequence = _playButton.transform.DOScale(1.1f, 1.1f)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void Init()
    {
        _sceneLoader = ServiceLocator.LocateService<SceneLoader>();
    }

    private void OnPlayBtnClick()
    {
        _sceneLoader.StartGameScene();
    }

    private void OnDestroy()
    {
        _playButton.onClick.RemoveListener(OnPlayBtnClick);
        
        _pulseSequence?.Kill();
        _pulseSequence = null;
    }
}
