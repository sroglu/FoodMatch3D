using System;
using System.Collections.Generic;
using Game.Data;
using Game.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Editor
{
    public class LevelEditor : EditorWindow
    {
        private static EditorWindow _window;
        private static Scene _scene;
        private static Camera _sceneCamera;
        private static GameObject _gameLevelRoot;
        private static GameObject _puzzleObjectHolder;
        private static readonly GameObject[] _puzzleWalls = new GameObject[6];

        private static readonly string[] _puzzleWallNames = new[]
        {
            "PuzzleWall_Top",
            "PuzzleWall_Bottom",
            "PuzzleWall_Side1",
            "PuzzleWall_Side2",
            "PuzzleWall_Side3",
            "PuzzleWall_Side4"
        };

        /// <summary>
        /// These variables can be stored GameData by a preset id, but we need odin serializer for that, and it's overkill for now
        /// </summary>
        private Vector3 _objectSpawnRoot;

        private float _waitDurationAfterThrow = 1f;
        private float _throwSpeed = 5f;
        private Vector3 coneAxisToThrow = Vector3.down;
        private float maxAngleDegToThrow = 30f;

        private LevelData _currentLevelData;
        private GameData _gameData;

        private bool _enteringPlayModeToThrow = false;
        private bool _throwingInProgress = false;

        private Vector3 _baseSize = new Vector3(.9f, 1.6f, 1f);
        private float _topOffset = 1f;
        private float _cameraEdgeOffset = 1.2f;
        private Vector2 _cameraPositionOffset = new Vector2(0f, 0.3f);


        private static Dictionary<PuzzleObject, List<Rigidbody>> _placedPuzzleObjects = new();

        private Vector2 _mainScrollPos;


        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            _window = GetWindow<LevelEditor>("Level Editor");
            _scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/LevelEditorScene.unity");
            ClearScene();
        }

        private static void ClearScene()
        {
            _gameLevelRoot = GameObject.Find("GameLevelRoot");
            if (_gameLevelRoot == null)
            {
                _gameLevelRoot = new GameObject("GameLevelRoot");
            }

            _puzzleObjectHolder = GameObject.Find("PuzzleObjectHolder");
            if (_puzzleObjectHolder == null)
            {
                _puzzleObjectHolder = new GameObject("PuzzleObjectHolder");
            }

            for (int i = _gameLevelRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = _gameLevelRoot.transform.GetChild(i).gameObject;
                Object.DestroyImmediate(child);
            }

            for (int i = 0; i < _puzzleWalls.Length; i++)
            {
                var existing = _puzzleObjectHolder.transform.Find(_puzzleWallNames[i]);
                if (!existing)
                {
                    Debug.LogError($"PuzzleObject {_puzzleWallNames[i]} not found");
                    return;
                }

                _puzzleWalls[i] = existing != null ? existing.gameObject : null;
            }

            _sceneCamera = Camera.main;
            _placedPuzzleObjects.Clear();
            SceneView.RepaintAll();
        }


        private void UpdatePuzzleHolder(Vector3 baseSize, float topOffset)
        {
            // calculate geometry
            float halfWidth = baseSize.x * 0.5f;
            float halfDepth = baseSize.y * 0.5f;
            // Use the larger base dimension as a baseline for height so a square base behaves like the example.
            float height = baseSize.z + topOffset;
            float centerY = topOffset - height * 0.5f;
            const float thin = 0.1f;
            const float bottomThickness = 0.5f;


            _sceneCamera.farClipPlane = height;
            _sceneCamera.orthographicSize = (Math.Max(baseSize.x, baseSize.y) + _cameraEdgeOffset) / 2f;
            _sceneCamera.transform.position = new Vector3(_cameraPositionOffset.x, topOffset, _cameraPositionOffset.y);

            var defs = new (Vector3 pos, Vector3 scale)[]
            {
                // Top
                (new Vector3(0f, topOffset, 0f), new Vector3(baseSize.x, thin, baseSize.y)),
                // Bottom
                (new Vector3(0f, topOffset - height - bottomThickness / 2, 0f),
                    new Vector3(baseSize.x, bottomThickness, baseSize.y)),
                // Side front (positive Z)
                (new Vector3(0f, centerY, halfDepth + thin / 2), new Vector3(baseSize.x, height, thin)),
                // Side back (negative Z)
                (new Vector3(0f, centerY, -halfDepth - thin / 2), new Vector3(baseSize.x, height, thin)),
                // Side right (positive X)
                (new Vector3(halfWidth + thin / 2, centerY, 0f), new Vector3(thin, height, baseSize.y)),
                // Side left (negative X)
                (new Vector3(-halfWidth - thin / 2, centerY, 0f), new Vector3(thin, height, baseSize.y))
            };

            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                GameObject wall = _puzzleWalls[i];

                if (wall == null)
                {
                    wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.name = _puzzleWallNames[i];
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

                wall.transform.localPosition = def.pos;
                wall.transform.localRotation = Quaternion.identity;
                wall.transform.localScale = def.scale;

                // ensure collider exists
                if (wall.GetComponent<Collider>() == null)
                    wall.AddComponent<BoxCollider>();

                _puzzleWalls[i] = wall;
            }

            SceneView.RepaintAll();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            ReloadResources();
            ClearScene();
            RefreshEditorView();

            NewLevelWindow.OnCreateNewLevel += HandleNewLevelCreated;
            LoadLevelWindow.LevelLoaded += HandleNewLevelCreated;
        }

        private void HandleNewLevelCreated(LevelData newLevel)
        {
            _currentLevelData = newLevel;
            ClearScene();
            UpdatePuzzleHolder(_baseSize, _topOffset);
            PlacePuzzleObjectsAtSavedPositions();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            NewLevelWindow.OnCreateNewLevel -= HandleNewLevelCreated;
            LoadLevelWindow.LevelLoaded -= HandleNewLevelCreated;
        }

        private void ReloadResources()
        {
            _gameData = Resources.Load<GameData>(nameof(GameData));
        }

        private void RefreshEditorView()
        {
            _enteringPlayModeToThrow = false;
            UpdatePuzzleHolder(_baseSize, _topOffset);
        }


        #region UI

        private void DrawSceneGizmos()
        {
            // Draw gizmos in the scene view to indicate the object spawn root
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, _objectSpawnRoot, Quaternion.identity, 0.1f, EventType.Repaint);
            DrawTransparentCone(_objectSpawnRoot, coneAxisToThrow, maxAngleDegToThrow, 2f,
                new Color(0f, 1f, 0f, 0.01f), new Color(0f, 1f, 0f, 0.5f));

            //draw walls
            Handles.color = Color.cyan;
            foreach (var wall in _puzzleWalls)
            {
                if (wall != null)
                {
                    var collider = wall.GetComponent<Collider>();
                    if (collider != null)
                    {
                        Handles.DrawWireCube(collider.bounds.center, collider.bounds.size);
                    }
                }
            }

            //draw camera position and frustum
            if (_sceneCamera != null)
            {
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, _sceneCamera.transform.position, Quaternion.identity, 0.1f,
                    EventType.Repaint);
                Handles.DrawWireCube(
                    _sceneCamera.transform.position +
                    _sceneCamera.transform.forward * (_sceneCamera.farClipPlane * 0.5f),
                    new Vector3(
                        _sceneCamera.orthographicSize * 2f * _sceneCamera.aspect,
                        _sceneCamera.farClipPlane,
                        _sceneCamera.orthographicSize * 2f
                    ));
            }

            SceneView.RepaintAll();
        }

        private void PlacePuzzleObjectsAtSavedPositions()
        {
            ClearScene();
            foreach (var puzzleObjectData in _currentLevelData.PuzzleObjects)
            {
                if(puzzleObjectData != null)
                    continue;
                
                if (!_gameData.TryGetPuzzleObjectViewData(
                        puzzleObjectData.TypeId, out var puzzleObjectViewData))
                {
                    Debug.LogError(
                        $"Puzzle Object View Data not found for TypeId: {puzzleObjectData.TypeId}");
                    ClearScene();
                    return;
                }

                var puzzleObjectPrefab = puzzleObjectViewData.Prefab;
                for (int i = 0; i < puzzleObjectData.Positions.Length; i++)
                {
                    var objectToPlace = Instantiate(
                        puzzleObjectPrefab,
                        puzzleObjectData.Positions[i],
                        puzzleObjectData.Rotations[i],
                        _gameLevelRoot.transform);

                    var rigidbody = objectToPlace.GetComponentInChildren<Rigidbody>();
                    if (!rigidbody)
                    {
                        Debug.LogError(
                            $"Puzzle Object Prefab does not have a Rigidbody component: {puzzleObjectPrefab.name}");
                        ClearScene();
                        return;
                    }

                    if (!_placedPuzzleObjects.ContainsKey(puzzleObjectData))
                    {
                        _placedPuzzleObjects.Add(puzzleObjectData, new List<Rigidbody>());
                    }

                    _placedPuzzleObjects[puzzleObjectData].Add(rigidbody);
                }
            }
        }

        private void ThrowAllObjects()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning(
                    "For deterministic physics, enter Play mode before running the throw routine. The editor will still position objects and apply forces, but the editor physics simulation is limited.");
                return;
            }

            if (_placedPuzzleObjects.Count > 0)
            {
                Debug.LogWarning("There are already placed puzzle objects in the scene. Clearing them first.");
                ClearScene();
            }

            _throwingInProgress = true;
            foreach (var puzzleObjectDataToBeThrown in _currentLevelData.PuzzleObjects)
            {
                if (!_gameData.TryGetPuzzleObjectViewData(
                        puzzleObjectDataToBeThrown.TypeId, out var puzzleObjectViewData))
                {
                    Debug.LogError(
                        $"Puzzle Object View Data not found for TypeId: {puzzleObjectDataToBeThrown.TypeId}");
                    ClearScene();
                    return;
                }


                _placedPuzzleObjects.Add(puzzleObjectDataToBeThrown, new List<Rigidbody>());

                for (int i = 0; i < puzzleObjectDataToBeThrown.Quantity * GameData.MatchCountToClear; i++)
                {
                    var objectToBeThrown = Instantiate(
                        puzzleObjectViewData.Prefab,
                        _gameLevelRoot.transform);

                    var rigidbody = objectToBeThrown.GetComponentInChildren<Rigidbody>();
                    if (!rigidbody)
                    {
                        Debug.LogError(
                            $"Puzzle Object Prefab does not have a Rigidbody component: {puzzleObjectViewData.Prefab.name}");
                        ClearScene();
                        return;
                    }

                    rigidbody.gameObject.SetActive(false);
                    _placedPuzzleObjects[puzzleObjectDataToBeThrown].Add(rigidbody);
                }
            }

            var allRigidbodies = new List<Rigidbody>();
            foreach (var kvp in _placedPuzzleObjects)
            {
                allRigidbodies.AddRange(kvp.Value);
            }

            //shuffle
            for (int i = 0; i < allRigidbodies.Count; i++)
            {
                int j = Random.Range(0, allRigidbodies.Count);
                (allRigidbodies[i], allRigidbodies[j]) = (allRigidbodies[j], allRigidbodies[i]);
            }

            foreach (var rb in allRigidbodies)
            {
                rb.isKinematic = false;
                rb.gameObject.SetActive(true);

                // small random spawn jitter so identical prefabs don't overlap exactly
                Vector3 spawnJitter = Random.insideUnitSphere * 0.12f; // tweak radius as needed

                // Use Impulse for a one-time velocity change; pass the jitter as spawnOffset
                ThrowThroughCone(rb, _objectSpawnRoot, coneAxisToThrow, maxAngleDegToThrow, _throwSpeed,
                    spawnJitter, ForceMode.Impulse);
            }

            _throwingInProgress = false;
        }


        private void DrawFileBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // "Load Level" button
            if (GUILayout.Button("New Level", EditorStyles.toolbarButton))
            {
                NewLevelWindow.ShowWindow();
            }

            if (GUILayout.Button("Load Level", EditorStyles.toolbarButton))
            {
                LoadLevelWindow.ShowWindow();
            }

            // "Save Level" button
            if (GUILayout.Button("Save Level", EditorStyles.toolbarButton))
            {
                var levelData = GenerateLevelData();
                if (IsLevelValid(levelData))
                {
                    SaveLevelWindow.ShowWindow(levelData);
                }
            }

            // This flexible space pushes subsequent buttons to the right
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            DrawFileBar();

            _mainScrollPos = EditorGUILayout.BeginScrollView(_mainScrollPos);
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Level Editor Settings", EditorStyles.boldLabel);


            var levelLoaded = _currentLevelData != null;
            if (!EditorApplication.isPlaying)
            {
                if (_throwingInProgress)
                {
                    EditorCoroutine.StopAllCoroutines();
                    _throwingInProgress = false;
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Object Throw Settings", EditorStyles.boldLabel);
            _objectSpawnRoot = EditorGUILayout.Vector3Field("Object Spawn Root", _objectSpawnRoot);
            _waitDurationAfterThrow = EditorGUILayout.FloatField("Wait Duration After Throw", _waitDurationAfterThrow);
            _throwSpeed = EditorGUILayout.FloatField("Throw Speed", _throwSpeed);
            coneAxisToThrow = EditorGUILayout.Vector3Field("Throw Cone Axis", coneAxisToThrow);
            maxAngleDegToThrow = EditorGUILayout.FloatField("Max Throw Angle (deg)", maxAngleDegToThrow);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Puzzle Holder Settings", EditorStyles.boldLabel);

            Vector3 newBaseSize = EditorGUILayout.Vector3Field("Puzzle Base Size", _baseSize);
            float newTopOffset = EditorGUILayout.FloatField("Puzzle Top Offset", _topOffset);
            float newCameraEdgeOffset = EditorGUILayout.FloatField("Camera Edge Offset", _cameraEdgeOffset);
            Vector2 newCameraPositionOffset =
                EditorGUILayout.Vector2Field("Camera Position Offset", _cameraPositionOffset);

            if (newBaseSize != _baseSize ||
                Math.Abs(newTopOffset - _topOffset) > 0.001f ||
                Math.Abs(newCameraEdgeOffset - _cameraEdgeOffset) > 0.001f ||
                newCameraPositionOffset != _cameraPositionOffset)
            {
                _baseSize = newBaseSize;
                _topOffset = newTopOffset;
                _cameraEdgeOffset = newCameraEdgeOffset;
                _cameraPositionOffset = newCameraPositionOffset;
                UpdatePuzzleHolder(_baseSize, _topOffset);
            }


            GUILayout.Space(10);

            EditorGUILayout.LabelField("Level Data Management", EditorStyles.boldLabel);

            if (GUILayout.Button("Reload Level Data"))
            {
                ReloadResources();
            }

            if (GUILayout.Button("Refresh Level Editor View"))
            {
                ClearScene();
                RefreshEditorView();
            }

            GUI.enabled = !EditorApplication.isPlaying && !_enteringPlayModeToThrow;

            if (GUILayout.Button("Enter Play Mode to Edit Level"))
            {
                _enteringPlayModeToThrow = true;
                EditorApplication.isPlaying = true;
            }

            GUI.enabled = EditorApplication.isPlaying;

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Level Operations", EditorStyles.boldLabel);
            if (levelLoaded)
            {
                EditorGUILayout.LabelField("Current Level ID: " + _currentLevelData.Id);

                var oldLevelId = _currentLevelData.Id;
                var newId = EditorGUILayout.IntField("Level ID", _currentLevelData.Id.Value);
                if (oldLevelId.Value != newId)
                {
                    _currentLevelData.Id = new LevelId(newId);
                }
                
                DrawPuzzleObjects(_currentLevelData.PuzzleObjects);

                //if there are position and rotation data show button to place them in the scene
                GUI.enabled = EditorApplication.isPlaying &&
                              _currentLevelData.PuzzleObjects.Length > 0 &&
                              _currentLevelData.PuzzleObjects[0] != null &&
                              _currentLevelData.PuzzleObjects[0].Positions != null &&
                              _currentLevelData.PuzzleObjects[0].Positions.Length > 0;

                if (GUILayout.Button("Place Objects at Saved Positions"))
                {
                    PlacePuzzleObjectsAtSavedPositions();
                }

                GUI.enabled = EditorApplication.isPlaying;

                _enteringPlayModeToThrow = false;
                if (GUILayout.Button("Throw Object From Spawn Root"))
                {
                    ThrowAllObjects();
                }

                if (_placedPuzzleObjects.Count > 0)
                {
                    if (GUILayout.Button("Shake Objects"))
                    {
                        AddForceToCreatedAllPuzzleObjects();
                    }
                }

                if (GUILayout.Button("Clear Scene"))
                {
                    ClearScene();
                }
            }

            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }


        private void DrawPuzzleObjects(PuzzleObject[] puzzleObjects)
        {
            int puzzleObjectCount = EditorGUILayout.IntField("Number of Puzzle Object Types", puzzleObjects.Length);
            puzzleObjectCount = Math.Max(0, puzzleObjectCount);
            if (puzzleObjectCount != puzzleObjects.Length)
            {
                Array.Resize(ref puzzleObjects, puzzleObjectCount);
                _currentLevelData.PuzzleObjects = puzzleObjects;
            }
            
            //Assign random type ids button without duplicates
            if(GUILayout.Button("Assign Random Type IDs"))
            {
                var typeIds = _gameData.GetAllPuzzleObjectTypeIds();
                var assignedTypeIds = new HashSet<uint>();
                bool allowDuplicates = puzzleObjects.Length > typeIds.Length;
                for (int i = 0; i < puzzleObjects.Length; i++)
                {
                    if (puzzleObjects[i] == null)
                        puzzleObjects[i] = new PuzzleObject();
                    
                    if (allowDuplicates)
                    {
                        int randomIndex = Random.Range(0, typeIds.Length);
                        puzzleObjects[i].TypeId = typeIds[randomIndex];
                        continue;
                    }
                    
                    uint randomTypeId;
                    do
                    {
                        int randomIndex = Random.Range(0, typeIds.Length);
                        randomTypeId = typeIds[randomIndex];
                    } while (assignedTypeIds.Contains(randomTypeId));

                    assignedTypeIds.Add(randomTypeId);
                    puzzleObjects[i].TypeId = randomTypeId;
                }
            }
            
            var randomRange = EditorGUILayout.Vector2IntField("Random Quantity Range", new Vector2Int(1, 10));
            randomRange.x = Mathf.Clamp(randomRange.x, 0, 10);
            randomRange.y = Mathf.Clamp(randomRange.y, randomRange.x, 10);
            //Assign random quantities button
            if(GUILayout.Button($"Assign Random Quantities {randomRange.x} to {randomRange.y}"))
            {
                for (int i = 0; i < puzzleObjects.Length; i++)
                {
                    if (puzzleObjects[i] == null)
                        puzzleObjects[i] = new PuzzleObject();

                    puzzleObjects[i].Quantity = (uint)Random.Range(randomRange.x, randomRange.y + 1);
                }
            }

            for (int i = 0; i < puzzleObjects.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"Puzzle Object Type {i + 1}", EditorStyles.boldLabel);
                uint oldTypeId = puzzleObjects[i]?.TypeId ?? 0;
                uint newTypeId = (uint)EditorGUILayout.IntField("Type ID", (int)oldTypeId);
                if (newTypeId != oldTypeId)
                {
                    if (puzzleObjects[i] == null)
                        puzzleObjects[i] = new PuzzleObject();
                    puzzleObjects[i].TypeId = newTypeId;
                }

                uint oldQuantity = puzzleObjects[i]?.Quantity ?? 0;
                uint newQuantity = (uint)EditorGUILayout.IntField("Quantity", (int)oldQuantity);
                if (newQuantity != oldQuantity)
                {
                    if (puzzleObjects[i] == null)
                        puzzleObjects[i] = new PuzzleObject();
                    puzzleObjects[i].Quantity = newQuantity;
                }

                EditorGUILayout.EndVertical();
            }
        }

        #endregion

        #region Utils

        // Returns a unit direction vector uniformly sampled inside a cone defined by 'coneAxis' and 'maxAngleDeg'.
        public static Vector3 RandomDirectionInCone(Vector3 coneAxis, float maxAngleDeg)
        {
            coneAxis = coneAxis.normalized;
            float maxAngleRad = maxAngleDeg * Mathf.Deg2Rad;

            // Cosine sampling for uniform solid-angle distribution
            float cosMax = Mathf.Cos(maxAngleRad);
            float cosTheta = Mathf.Lerp(cosMax, 1f, Random.value); // in [cosMax, 1]
            float sinTheta = Mathf.Sqrt(1f - cosTheta * cosTheta);
            float phi = Random.Range(0f, Mathf.PI * 2f);

            // Build orthonormal basis (ortho, binormal, coneAxis)
            Vector3 ortho = Vector3.Cross(coneAxis, Vector3.up);
            if (ortho.sqrMagnitude < 1e-6f) ortho = Vector3.Cross(coneAxis, Vector3.right);
            ortho.Normalize();
            Vector3 binormal = Vector3.Cross(coneAxis, ortho);

            // Spherical -> Cartesian in the cone's local frame
            Vector3 dir = coneAxis * cosTheta
                          + ortho * (Mathf.Cos(phi) * sinTheta)
                          + binormal * (Mathf.Sin(phi) * sinTheta);

            return dir.normalized;
        }


        private static void ThrowThroughCone(Rigidbody rb, Vector3 origin, Vector3 coneAxis, float maxAngleDeg,
            float speed, Vector3 spawnOffset = default, ForceMode mode = ForceMode.VelocityChange)
        {
            if (rb == null) return;

            // place via Rigidbody to avoid teleport/kinematic conflicts
            Vector3 spawnPos = origin + spawnOffset;
            rb.position = spawnPos;

            // reset existing motion for deterministic throw
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // pick a direction and apply force
            Vector3 dir = RandomDirectionInCone(coneAxis, maxAngleDeg);
            rb.AddForce(dir * speed, mode);

            // small random torque helps objects separate after collisions
            rb.AddTorque(Random.insideUnitSphere * 0.15f, ForceMode.Impulse);
        }

        private static void AddForceToCreatedAllPuzzleObjects(float minImpulse = 0.3f, float maxImpulse = 0.8f,
            float jitterRadius = 0.05f)
        {
            foreach (var kvp in _placedPuzzleObjects)
            {
                var rigidbodies = kvp.Value;
                foreach (var rb in kvp.Value)
                {
                    if (rb == null) continue;

                    // make sure object participates in physics
                    rb.isKinematic = false;
                    rb.gameObject.SetActive(true);

                    // clear previous motion for deterministic behaviour
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    // tiny positional jitter so identical prefabs don't start exactly overlapped
                    rb.position += Random.insideUnitSphere * jitterRadius;

                    // bias upward a little so objects separate more reliably
                    Vector3 biasUp = Vector3.up * 0.25f;
                    Vector3 dir = (Random.onUnitSphere + biasUp).normalized;

                    // randomize impulse magnitude
                    float mag = Random.Range(minImpulse, maxImpulse);
                    rb.AddForce(dir * mag, ForceMode.Impulse);

                    // random torque to help rotation/separation after collisions
                    rb.AddTorque(Random.insideUnitSphere * Random.Range(0.1f, 0.5f), ForceMode.Impulse);
                }
            }
        }


        private void DrawTransparentCone(Vector3 apex, Vector3 axis, float maxAngleDeg, float length, Color fillColor,
            Color lineColor, int segments = 24)
        {
            if (length <= 0f || segments < 3) return;

            axis = axis.normalized;
            float maxAngleRad = maxAngleDeg * Mathf.Deg2Rad;
            float radius = Mathf.Tan(maxAngleRad) * length;
            Vector3 baseCenter = apex + axis * length;

            // build orthonormal basis
            Vector3 ortho = Vector3.Cross(axis, Vector3.up);
            if (ortho.sqrMagnitude < 1e-6f) ortho = Vector3.Cross(axis, Vector3.right);
            ortho.Normalize();
            Vector3 binormal = Vector3.Cross(axis, ortho);

            // compute base points
            Vector3[] basePoints = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float phi = (i / (float)segments) * Mathf.PI * 2f;
                basePoints[i] = baseCenter + (ortho * Mathf.Cos(phi) + binormal * Mathf.Sin(phi)) * radius;
            }

            // preserve color
            Color prev = Handles.color;

            // draw filled base
            Handles.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a);
            Handles.DrawSolidDisc(baseCenter, axis, radius);

            // draw filled triangular faces (apex, p_i, p_{i+1})
            for (int i = 0; i < segments; i++)
            {
                Vector3 p0 = apex;
                Vector3 p1 = basePoints[i];
                Vector3 p2 = basePoints[(i + 1) % segments];
                Handles.DrawAAConvexPolygon(new Vector3[] { p0, p1, p2 });
            }

            // draw outlines
            Handles.color = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a);
            // base circle outline
            Vector3[] outline = new Vector3[segments + 1];
            for (int i = 0; i < segments; i++) outline[i] = basePoints[i];
            outline[segments] = basePoints[0];
            Handles.DrawPolyLine(outline);

            // edges from apex to base
            for (int i = 0; i < segments; i++)
            {
                Handles.DrawLine(apex, basePoints[i]);
            }

            Handles.color = prev;
        }

        #endregion

        #region Level Loading/Saving

        private LevelData GenerateLevelData()
        {
            foreach (var kvp in _placedPuzzleObjects)
            {
                var puzzleObjectData = kvp.Key;
                var transforms = kvp.Value;

                puzzleObjectData.Positions = new Vector3[transforms.Count];
                puzzleObjectData.Rotations = new Quaternion[transforms.Count];
                for (int i = 0; i < transforms.Count; i++)
                {
                    puzzleObjectData.Positions[i] = transforms[i].position;
                    puzzleObjectData.Rotations[i] = transforms[i].rotation;
                }
            }

            _currentLevelData.CameraData = new CameraData
            {
                Position = _sceneCamera.transform.position,
                Rotation = _sceneCamera.transform.rotation,
                OrthographicSize = _sceneCamera.orthographicSize,
                FarClipPlane = _sceneCamera.farClipPlane
            };

            LevelData levelData = new LevelData
            {
                Id = _currentLevelData.Id,
                PuzzleObjects = _currentLevelData.PuzzleObjects,
                CameraData = _currentLevelData.CameraData
            };

            //Add puzzle object positions and rotations after throwing logic is implemented
            //Add camera position rotation zoom level and far clip plane 
            return levelData;
        }

        #endregion


        #region Checks

        private bool IsLevelValid(LevelData levelData)
        {
            if (levelData.Id.Value <= 0)
            {
                Debug.LogError("Level ID cannot be zero or negative.");
                return false;
            }

            return true;
        }

        #endregion

        private void OnSceneGUI(SceneView sceneView)
        {
            // Draw the gizmo each frame in the scene view
            DrawSceneGizmos();

            // Ensure the SceneView repaints so the gizmo is visible after changes
            sceneView.Repaint();
        }
    }
}