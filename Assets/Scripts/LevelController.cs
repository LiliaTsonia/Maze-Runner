using Common.Enums;
using Common.EventsSystem;
using Common.SaveSystem;
using Common.SceneManagement;
using Common.ServiceLocator;
using Game.UI;
using Game.UI.PopUp;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    private const float UPDATE_INTERVAL = 0.1f;

    [SerializeField] 
    private MazeCreator mazeCreator;
    
    [SerializeField] 
    private PlayerController _playerController;
    
    [SerializeField] 
    private LevelUIController _levelUIController;

    [SerializeField] 
    private PausePopup _pausePopup;
    
    [SerializeField] 
    private WinnerPopup winnerPopup;
    
    private float _levelTime;
    private float _uiUpdateAccum;

    private EventManager _eventManager;
    private LevelHandler _levelHandler;
    private PopUpManager _popUpManager;
    private SceneLoader _sceneLoader;

    private void Awake()
    {
        _levelHandler = ServiceLocator.LocateService<LevelHandler>();
        _eventManager = ServiceLocator.LocateService<EventManager>();
        _eventManager.GetEvent(EGameEvent.Pause).Subscribe(PauseGame);
        _eventManager.GetEvent(EGameEvent.Win).Subscribe(CompleteLevel);
        _popUpManager = ServiceLocator.LocateService<PopUpManager>();
        _sceneLoader = ServiceLocator.LocateService<SceneLoader>();

        InitLevelData();
    }

    private void Start()
    {
        StartLevel();
    }
    
    private void InitLevelData()
    {
        mazeCreator.CreateMaze();
        var playerStartPosition = mazeCreator.GetMazeCenterPosition();
        _playerController.Init(ref playerStartPosition);
        mazeCreator.CreateExits();
        
        ResetLevelTime();
        _levelUIController.Init();
        var distance = _playerController.GetTotalDistance();
        _levelUIController.UpdateDistance(ref distance);
    }

    private void StartLevel()
    {
        _popUpManager.PopLast();
        _eventManager.GameStatus = EGameState.Playing;
        _levelUIController.EnableTimer();
    }

    private void StartNextLevel()
    {
        _popUpManager.PopLast();
        InitLevelData();
        StartLevel();
    }

    private void BackToMainMenu()
    {
        _popUpManager.PopLast();
        _sceneLoader.GoToMainMenu();
    }

    private void Update()
    {
        if (_eventManager.GameStatus != EGameState.Playing)
        {
            return;
        }

        CountTime();

        _playerController.UpdateInput();
    }

    private void FixedUpdate()
    {
        if (_eventManager.GameStatus != EGameState.Playing)
        {
            return;
        }

        _playerController.Move();
        _uiUpdateAccum += Time.fixedDeltaTime;

        if (_uiUpdateAccum >= UPDATE_INTERVAL)
        {
            _uiUpdateAccum = 0f;
            var distance = _playerController.GetTotalDistance();
            _levelUIController.UpdateDistance(ref distance);
        }
    }

    private void ResetLevelTime()
    {
        _levelTime = 0f;
    }

    private void CountTime()
    {
        _levelTime += Time.deltaTime;
        _levelUIController.UpdateTimeDisplay(ref _levelTime);
    }

    private void PauseGame()
    {
        _eventManager.GameStatus = EGameState.Pause;
        _playerController.StopMovement();

        var intent = new PausePopup.Intent(BackToMainMenu, UnpauseGame);
        _popUpManager.PushWithIntent<PausePopup, PausePopup.Intent>(_pausePopup, intent);
    }

    private void UnpauseGame()
    {
        _eventManager.GameStatus = EGameState.Playing;
    }

    private void CompleteLevel()
    {
        _eventManager.GameStatus = EGameState.Win;
        _playerController.StopMovement();
        _levelHandler.TryToSaveBestTime(_levelTime);

        var intent = new WinnerPopup.Intent(
            _levelHandler.GetBestTime(), 
            _levelTime, 
            BackToMainMenu, 
            StartNextLevel);

        _popUpManager.PushWithIntent<WinnerPopup, WinnerPopup.Intent>(winnerPopup, intent);
    }

    private void OnDestroy()
    {
        mazeCreator.ClearMaze();
        
        _eventManager.GetEvent(EGameEvent.Pause).Unsubscribe(PauseGame);
        _eventManager.GetEvent(EGameEvent.Win).Unsubscribe(CompleteLevel);
    }
}
