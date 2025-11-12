using System;
using System.Collections.Generic;
using Game;
using Game.Data;
using Game.Data.ModelData;
using Game.DataStores;
using Game.Instances.PuzzleInstances;
using mehmetsrl.DataStore;
using mehmetsrl.MVC;
using mehmetsrl.MVC.core;
using UnityEngine;
using Object = UnityEngine.Object;

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
    [SerializeField] private GameObject _splashScreenImage;
    
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
        
        _dataStoreManager = new DataStoreManager();
        var gameData = Resources.Load<GameData>(nameof(GameData));
        
        GameDataStore.Initialize();
        GameDataStore.Instance.SetGameData(gameData);
        GameDataStore.Instance.SetGameCamera(Camera.main);

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
        dashboardPage.HideView();
        gamePage = new GamePageController(new GamePageModel(new GamePageData()));
        gamePage.HideView();
        if (GameDataStore.Instance.CurrentLevelId == 0)
        {
            //Player is new, start from level 1
            //Tutorial can be handled here if needed
            LoadLevel(new LevelId(1));
        }
        else
        {
            dashboardPage.ShowView();
            _splashScreenImage.SetActive(false);
        }
        
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CompleteLevel(false);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            CompleteLevel(true);
        }
    }

    private void LoadLevel(LevelId levelId)
    {
        //var level = LevelUtils.LoadLevel(levelId);
        _splashScreenImage.SetActive(true);
        StartCoroutine(LevelUtils.LoadLevelAsync(levelId, OnLevelLoaded));
    }

    private void OnLevelLoaded(LevelData level)
    {
        foreach (var puzzleObject in level.PuzzleObjects)
        {
            Debug.Assert(puzzleObject.Quantity * GameData.MatchCountToClear == puzzleObject.Positions.Length);
            Debug.Assert(puzzleObject.Quantity * GameData.MatchCountToClear == puzzleObject.Rotations.Length);
            
            /*Debug.Log($"Spawning Puzzle Object TypeId: {puzzleObject.TypeId}, Quantity: {puzzleObject.Quantity} Positions Count: {puzzleObject.Positions.Length}" +
                      $" Rotations Count: {puzzleObject.Rotations.Length}");*/
            
            for (int i = 0; i < puzzleObject.Quantity * GameData.MatchCountToClear; i++)
            {
                SpawnPuzzleObject(puzzleObject.TypeId, puzzleObject.Positions[i], puzzleObject.Rotations[i]);
            }
        }
        
        gamePage.Update(new GamePageData(level));
        dashboardPage.HideView();
        gamePage.ShowView();
        gamePage.View.UpdateView();
        
        
        _splashScreenImage.SetActive(false);
    }


    public void LoadNextLevel()
    {
        var nextLevelId = new LevelId(GameDataStore.Instance.CurrentLevelId + 1);
        LoadLevel(nextLevelId);
    }
    
    public void CompleteLevel(bool isSuccess)
    {
        ClearPuzzleObjects();
        if (isSuccess)
        {
            //Tutorial is not implemented yet, so we skip tutorial completion check here and set it as completed directly
            if (GameDataStore.Instance.CurrentLevelId == 0)
            {
                GameDataStore.Instance.SetTutorialCompleted();
            }
            GameDataStore.Instance.UpdatePlayerDataOnLevelComplete();
        }
        
        ReturnToDashboard();
    }
    private void ReturnToDashboard()
    {
        dashboardPage.View.UpdateView();
        dashboardPage.ShowView();
        gamePage.HideView();
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
        
        GameDataStore.Instance.GameCamera.farClipPlane = cameraFarClipPlane;
        GameDataStore.Instance.GameCamera.transform.position = cameraPosition;
        
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
            puzzleObject.GetComponentInChildren<Rigidbody>().isKinematic = false;
        }
        else
        {
            Debug.LogError($"Puzzle Object View Data not found for TypeId: {typeId}");
            return;
        }
    }
    
    private void ClearPuzzleObjects()
    {
        foreach (var puzzleObject in _puzzleObjectHolder.GetComponentsInChildren<PuzzleObjectInstance>())
        {
            puzzleObject.Dispose();
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
