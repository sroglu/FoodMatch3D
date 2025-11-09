using Game;
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
    
    //Scene References
    [SerializeField] private Transform _levelObjectSpawnPoint;
    
    //DataStores
    private DataStoreManager _dataStoreManager;

    //Pages
    private GamePageController gamePage;
    private DashboardPageController dashboardPage;
    #endregion

    private void Awake()
    {
        Instance = this;
        
        _dataStoreManager = new DataStoreManager();
        var gameData = Resources.Load<GameData>(nameof(GameData));
        
        GameDataStore.Initialize();
        GameDataStore.Instance.SetGameData(gameData);
    }

    private void Start()
    {
        InitPages();

        dashboardPage.ShowView();
    }

    private void InitPages()
    {
        dashboardPage = new DashboardPageController(new DashboardPageModel(new DashboardPageData()));
        if (GameDataStore.Instance.CurrentLevelId == 0)
        {
            //Player is new, start from level 1
            //Tutorial can be handled here if needed
            LoadLevel(new LevelId(1));
        }
        
    }
    
    private void LoadLevel(LevelId levelId)
    {
        var level = LevelUtils.LoadLevel(levelId);
        gamePage = new GamePageController(new GamePageModel(new GamePageData(level)));
        dashboardPage.HideView();
        gamePage.ShowView();
        gamePage.LoadLevel();
        gamePage.View.UpdateView();
    }
    
    public void CompleteLevel(bool isSuccess)
    {
        if (isSuccess)
        {
            GameDataStore.Instance.UpdatePlayerDataOnLevelComplete();
        }
        
        ReturnToDashboard();
    }
    private void ReturnToDashboard()
    {
        dashboardPage.View.UpdateView();
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
