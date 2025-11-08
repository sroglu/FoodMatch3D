using Game.Constants;
using Game.Data;
using Game.Data.ModelData;
using Game.DataStores;
using mehmetsrl.DataStore;
using mehmetsrl.MVC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null)
                _instance = value;
            else
                Destroy(value);
        }
    }
    
    #region Properties
    
    private DataStoreManager _dataStoreManager;

    //Pages
    private GamePageController gamePage;
    private DashboardPageController dashboardPage;
    #endregion

    private void Awake()
    {
        Instance = this;
        
        _dataStoreManager = new DataStoreManager();
        var levelData = Resources.Load<LevelData>(nameof(LevelData));
        var gameData = Resources.Load<GameData>(nameof(GameData));
        
        GameDataStore.Initialize();
        GameDataStore.Instance.SetLevelData(levelData);
        GameDataStore.Instance.SetGameData(gameData);
    }


    private void Start()
    {
        InitPages();

        dashboardPage.ShowView();
    }

    private void InitPages()
    {
        var playerData = new PlayerData()
        {
            Name = PlayerPrefs.GetString(PlayerPrefsKeys.PlayerNameKey, "New Player"),
            CurrentLevelId = new LevelId(PlayerPrefs.GetInt(PlayerPrefsKeys.PlayerLevelKey, 0)),
            Coins = (uint)PlayerPrefs.GetInt(PlayerPrefsKeys.PlayerCoinsKey, 0),
            SoundOn = PlayerPrefs.GetInt(PlayerPrefsKeys.SoundVolumeKey, 1) == 1,
            MusicOn = PlayerPrefs.GetInt(PlayerPrefsKeys.MusicVolumeKey, 1) == 1,
            IsTutorialCompleted = PlayerPrefs.GetInt(PlayerPrefsKeys.IsTutorialCompletedKey, 0) == 1
        };
        dashboardPage = new DashboardPageController(new DashboardPageModel(new DashboardPageData(playerData)));
        
        
    }
    
    public void LoadLevel(LevelId levelId)
    {
        var level = GameDataStore.Instance.GetLevelDataById(levelId);
        gamePage = new GamePageController(new GamePageModel(new GamePageData(level)));
        dashboardPage.HideView();
        gamePage.ShowView();
        gamePage.LoadLevel();
        gamePage.View.UpdateView();
    }
    
    public void ReturnToDashboard()
    {
        dashboardPage.ShowView();
        gamePage.HideView();
        gamePage.Dispose();
    }



    void OnApplicationQuit()
    {
        _dataStoreManager.Dispose();
        InstanceManager.Instance.Dispose();
        ViewManager.Instance.Dispose();
    }

}
