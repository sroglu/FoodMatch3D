using Game;
using Game.Data;
using Game.Data.ModelData;
using Game.DataStores;
using Game.Instances.PuzzleInstances;
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
    [SerializeField] private Transform _puzzleObjectHolder;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Canvas _worldCanvas;
    
    //Camera
    public Camera GameCamera { get; private set; }
    
    //DataStores
    private DataStoreManager _dataStoreManager;

    //Pages
    private GamePageController gamePage;
    private DashboardPageController dashboardPage;
    
    
    private static readonly GameObject[] _puzzleWalls = new GameObject[6];
    
    #endregion

    private void Awake()
    {
        Instance = this;
        
        GameCamera = Camera.main;
        
        _dataStoreManager = new DataStoreManager();
        var gameData = Resources.Load<GameData>(nameof(GameData));
        
        GameDataStore.Initialize();
        GameDataStore.Instance.SetGameData(gameData);

        InitPuzzleObjects();
    }

    private void Start()
    {
        InitPages();
    }

    #region Page Management

    
    private void InitPages()
    {
        dashboardPage = new DashboardPageController(new DashboardPageModel(new DashboardPageData()));
        if (GameDataStore.Instance.CurrentLevelId == 0)
        {
            //Player is new, start from level 1
            //Tutorial can be handled here if needed
            LoadLevel(new LevelId(1));
        }
        else
        {
            dashboardPage.ShowView();
        }
        
    }
    
    private void LoadLevel(LevelId levelId)
    {
        var level = LevelUtils.LoadLevel(levelId);

        foreach (var puzzleObject in level.PuzzleObjects)
        {
            Debug.Assert(puzzleObject.Quantity * GameData.MatchCountToClear == puzzleObject.Positions.Length);
            Debug.Assert(puzzleObject.Quantity * GameData.MatchCountToClear == puzzleObject.Rotations.Length);
            for (int i = 0; i < puzzleObject.Quantity * GameData.MatchCountToClear; i++)
            {
                SpawnPuzzleObject(puzzleObject.TypeId, puzzleObject.Positions[i], puzzleObject.Rotations[i]);
            }
        }

        gamePage = new GamePageController(new GamePageModel(new GamePageData(level)));
        dashboardPage.HideView();
        gamePage.ShowView();
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

    #endregion

    #region Puzzzle Object Management

    private void InitPuzzleObjects()
    {
        CreatePuzzleWalls();
    }
    
    private void CreatePuzzleWalls()
    {
        _puzzleObjectHolder.transform.position = Vector3.zero;
        _puzzleObjectHolder.transform.rotation = Quaternion.identity;
        
        LevelUtils.GetPuzzleViewAndWalls(GameData.BaseSize, GameData.TopOffset, GameData.CameraPositionOffset, 
            out var cameraFarClipPlane, out Vector3 cameraPosition, out var puzzleWallsDefs); 
        
        GameCamera.farClipPlane = cameraFarClipPlane;
        GameCamera.transform.position = cameraPosition;
        
        _worldCanvas.planeDistance = cameraFarClipPlane/2;
        
        for (int i = 0; i < puzzleWallsDefs.Length; i++)
        {
            var puzzleWallsDef = puzzleWallsDefs[i];
            GameObject wall = _puzzleWalls[i];

            if (wall == null)
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = LevelUtils.PuzzleWallNames[i];
                wall.transform.SetParent(_puzzleObjectHolder.transform, false);

                // remove renderer to make the wall invisible while keeping colliders
                var rend = wall.GetComponent<MeshRenderer>();
                if (rend != null)
                    Object.DestroyImmediate(rend);
            }
            else
            {
                if (wall.transform.parent != _puzzleObjectHolder.transform)
                    wall.transform.SetParent(_puzzleObjectHolder.transform, false);
            }

            wall.transform.localPosition = puzzleWallsDef.pos;
            wall.transform.localRotation = Quaternion.identity;
            wall.transform.localScale = puzzleWallsDef.scale;

            // ensure collider exists
            if (wall.GetComponent<Collider>() == null)
                wall.AddComponent<BoxCollider>();

            _puzzleWalls[i] = wall;
        }
    }
    
    private void SpawnPuzzleObject(uint typeId, Vector3 position, Quaternion rotation)
    {
        if (GameDataStore.Instance.GameData.TryGetPuzzleObjectViewData(typeId, out var puzzleObjectViewData))
        {
            var puzzleObject = InstanceManager.Instance.SpawnInstance<PuzzleObjectInstance>(puzzleObjectViewData.Prefab);
            puzzleObject.transform.SetParent(_puzzleObjectHolder, false);
            puzzleObject.Initialize(typeId, position, rotation, Vector3.one);
        }
        else
        {
            Debug.LogError($"Puzzle Object View Data not found for TypeId: {typeId}");
            return;
        }
    }

    #endregion


    void OnApplicationQuit()
    {
        _dataStoreManager.Dispose();
        InstanceManager.Instance.Dispose();
        ViewManager.Instance.Dispose();
    }

}
