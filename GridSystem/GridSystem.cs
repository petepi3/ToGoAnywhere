using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Petepi.TGA.Grid
{
    [ExecuteInEditMode]
    public class GridSystem : MonoBehaviour
    {
        public Mesh tileMesh;
        public Material material;
        public Vector2Int numberOfChunks;
        public Vector2Int chunkPreviewRange;
        public int seed;
        public Camera mainCamera;
        public List<ResourceMesh> resourceMeshes;

        public const int ChunkSize = 16;
        public const float TileWidth = 0.8660254037844386f;
        
        private Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
        private Vector2Int _previousNumberOfChunks;
        
        private RenderParams _renderParams;
        
        private bool _isGenerated;
        private List<Generator> _generators;
        private List<Generator> _activeGenerators;
        private List<Vector2Int> _chunkPositionsInCamera = new List<Vector2Int>();
        private List<ViewPortTilePosition> _viewportSpaceTilePositions;
        private static Dictionary<Resource, Mesh> _resourceMeshesDict;
        private static Dictionary<Resource, RenderParams> _resourceRenderParams;
        public Vector2Int SelectedTilePosition { get; private set; }

        private struct ViewPortTilePosition
        {
            public Vector2Int GridPosition;
            public Vector2 ViewportPosition;
            public Vector3 WorldPosition;
        }

        [Serializable]
        public struct ResourceMesh
        {
            public Resource Resource;
            public Mesh Mesh;
            public Material Material;
        }
        
        public struct Chunk
        {
            public bool IsGenerated;
            public bool IsQueuedForGeneration;
            public bool NeedsShaderDataUpdate;
            public Vector2Int Position;
            public AggregatedTiles Tiles;
            public Matrix4x4[] Transforms;
            public MaterialPropertyBlock MaterialPropertyBlock;
            public Dictionary<Resource, List<Matrix4x4>> Resources;
            public float[] ResourceAmounts;

            public Chunk(Vector2Int position)
            {
                IsGenerated = false;
                IsQueuedForGeneration = false;
                NeedsShaderDataUpdate = false;
                Position = position;
                Tiles = new AggregatedTiles(ChunkSize);
                Transforms = new Matrix4x4[ChunkSize * ChunkSize];
                MaterialPropertyBlock = new MaterialPropertyBlock();
                Resources = _resourceMeshesDict.Keys.ToDictionary(resource=> resource, _ => new List<Matrix4x4>());
                ResourceAmounts = Enumerable.Repeat(0f, ChunkSize*ChunkSize).ToArray();
            }

            public void UpdateShaderData()
            {
                MaterialPropertyBlock.SetVectorArray("_Color", Tiles.Color);
                MaterialPropertyBlock.SetVectorArray("_SideColor", Tiles.SideColor);
                MaterialPropertyBlock.SetFloatArray("_Glossiness", Tiles.Glossiness);
                MaterialPropertyBlock.SetFloatArray("_Metallic", Tiles.Metallic);
                MaterialPropertyBlock.SetFloatArray("_WaterDepth", Tiles.WaterDepth);
                Resources = _resourceMeshesDict.Keys.ToDictionary(resource => resource, _ => new List<Matrix4x4>());
                var resourceIndex = 0;
                for (int i = 0; i < ChunkSize * ChunkSize; i++)
                {
                    if(Tiles.Resource[i] == Resource.None) continue;
                    ResourceAmounts[resourceIndex] = Tiles.ResourceAmount[i];
                    Resources[Tiles.Resource[i]].Add(Transforms[i]);
                    resourceIndex++;
                    // HACK - it's not possible to send different size arrays
                    // to instance shaders, so we need to always send an array
                    // of maximum size and only set the first indices 
                    // that correspond to the array of resources.
                }

                NeedsShaderDataUpdate = false;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _renderParams = new RenderParams(material);
            _renderParams.shadowCastingMode = ShadowCastingMode.TwoSided;
            _renderParams.receiveShadows = true;
            _resourceMeshesDict = resourceMeshes.ToDictionary(rm => rm.Resource, rm => rm.Mesh);
            _resourceRenderParams = resourceMeshes.ToDictionary(rm => rm.Resource, rm =>
            {
                var rp = new RenderParams(rm.Material);
                rp.shadowCastingMode = ShadowCastingMode.TwoSided;
                rp.receiveShadows = true;
                rp.matProps = new MaterialPropertyBlock();
                return rp;
            });
            ResetGenerators();
        }

        // converts chunk coordinates to chunk-relative local positions
        public static Vector3 ChunkGridToLocalFlat(Vector2Int positionInChunk)
        {
            var skew = positionInChunk.y % 2 == 0 ? 0.5f : 0f;
            return new Vector3(positionInChunk.x + skew, 0, positionInChunk.y * TileWidth);
        }

        public static Vector2Int WorldToGrid(Vector3 position)
        {
            position.z /= TileWidth;
            var skew = Mathf.RoundToInt(position.z) % 2 == 0 ? 0.5f : 0f;
            return new Vector2Int(Mathf.RoundToInt(position.x - skew), Mathf.RoundToInt(position.z));
        }

        public static Vector2Int[] GetNeighbours(Vector2Int position)
        {
            var skew = position.y % 2 == 0 ? 1 : 0;
            return new[]
            {
                position + new Vector2Int(-1 + skew, 1), // top left
                position + new Vector2Int(0 + skew, 1), // top
                position + new Vector2Int(1, 0), // right
                position + new Vector2Int(0 + skew, -1), // bottom
                position + new Vector2Int(-1 + skew, -1), // bottom left
                position + new Vector2Int(-1, 0), // left
            };
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            var skew = gridPosition.y % 2 == 0 ? 0.5f : 0f;
            var tile = GetTile(gridPosition);
            return new Vector3(gridPosition.x + skew, tile.HeightOffset, gridPosition.y * TileWidth);
        }

        private Vector2Int GetChunkPosition(Vector2Int position)
        {
            return Vector2Int.FloorToInt(new Vector2(position.x, position.y) / ChunkSize);
        }

        public Tile GetTile(Vector2Int position)
        {
            var chunkPosition = Vector2Int.FloorToInt(new Vector2(position.x, position.y) / ChunkSize);
            var positionInChunk = position - (chunkPosition * ChunkSize);
            var i = LocalChunkPositionToIndex(positionInChunk);
            var agr = _chunks[chunkPosition].Tiles;
            return agr.GetTile(i);
        }

        public bool DoesTileExist(Vector2Int position)
        {
            var chunkPosition = Vector2Int.FloorToInt(new Vector2(position.x, position.y) / ChunkSize);
            return _chunks.ContainsKey(chunkPosition) && _chunks[chunkPosition].IsGenerated;
        }

        public float GetHeightDifference(Vector2Int fromPosition, Vector2Int toPosition)
        {
            var from = GetTile(fromPosition);
            var to = GetTile(toPosition);

            return Mathf.Abs(to.HeightOffset - from.HeightOffset);
        }

        public void SetTile(Tile tile)
        {
            var chunkPosition = Vector2Int.FloorToInt(new Vector2(tile.GridPosition.x, tile.GridPosition.y) / ChunkSize);
            var positionInChunk = tile.GridPosition - (chunkPosition * ChunkSize);
            var i = LocalChunkPositionToIndex(positionInChunk);
            var chunk = _chunks[chunkPosition];
            Tile.Aggregate(ref chunk.Tiles, ref tile, i);
            chunk.NeedsShaderDataUpdate = true;
            _chunks[chunkPosition] = chunk;
        }

        private void DoGenerate()
        {
            Initialize();
            GenerateChunks();
        }

        private void OnRelativeValidate()
        {
            DoGenerate();
        }

        private void GetCameraBounds(out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight, out Vector3 bottomLeft)
        {
            var plane = new Plane(Vector3.up, Vector3.zero);
            var topLeftRay = mainCamera.ViewportPointToRay(new Vector3(0, 1));
            var topRightRay = mainCamera.ViewportPointToRay(new Vector3(1, 1));
            var bottomRightRay = mainCamera.ViewportPointToRay(new Vector3(1, 0));
            var bottomLeftRay = mainCamera.ViewportPointToRay(new Vector3(0, 0));

            topLeft = Vector3.zero;
            topRight = Vector3.zero;
            bottomRight = Vector3.zero;
            bottomLeft = Vector3.zero;



            if (!plane.Raycast(topLeftRay, out var topLeftDistance)
                || !plane.Raycast(topRightRay, out var topRightDistance)
                || !plane.Raycast(bottomRightRay, out var bottomRightDistance)
                || !plane.Raycast(bottomLeftRay, out var bottomLeftDistance))
            {
                throw new Exception("Invalid camera position, make sure camera is always pointed at the ground");
            }

            topLeft = topLeftRay.GetPoint(topLeftDistance);
            topRight = topRightRay.GetPoint(topRightDistance);
            bottomRight = bottomRightRay.GetPoint(bottomRightDistance);
            bottomLeft = bottomLeftRay.GetPoint(bottomLeftDistance);
        }

        private List<Vector2Int> GetChunkPositionsInCamera()
        {
            var chunks = new List<Vector2Int>();
            GetCameraBounds(out var topLeft, out var topRight, out var bottomRight, out var bottomLeft);
            var minX = Mathf.Min(topLeft.x, topRight.x, bottomRight.x, bottomLeft.x);
            var maxX = Mathf.Max(topLeft.x, topRight.x, bottomRight.x, bottomLeft.x);
            var minY = Mathf.Min(topLeft.z, topRight.z, bottomRight.z, bottomLeft.z) / TileWidth;
            var maxY = Mathf.Max(topLeft.z, topRight.z, bottomRight.z, bottomLeft.z) / TileWidth;

            for (float x = minX - ChunkSize; x < maxX + ChunkSize; x += ChunkSize)
            {
                for (float y = minY - ChunkSize; y < maxY + ChunkSize; y += ChunkSize)
                {
                    var chunkPositionX = Mathf.Round(x / ChunkSize) * ChunkSize;
                    var chunkPositionY = (Mathf.Round(y / ChunkSize) * ChunkSize);
                    chunks.Add(new Vector2Int( Mathf.RoundToInt((chunkPositionX)/ChunkSize),
                        Mathf.RoundToInt(chunkPositionY/ChunkSize)));
                }
            }

            return chunks;
        }

        private void GenerateChunk(Chunk chunk)
        {
            var tileSize = 1 / TileWidth / 2;
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    var tileId = x + (y * ChunkSize);
                    var position = (chunk.Position * ChunkSize) + new Vector2Int(x, y);
                    var tile = GenerateTile(position);
                    tile.GridPosition = position;
                    var tileTransform = Matrix4x4.Translate(ChunkGridToLocalFlat(position) + (Vector3.up * tile.HeightOffset)) *
                                    Matrix4x4.Scale(new Vector3(tileSize, 1, tileSize));
                    chunk.Transforms[tileId] = tileTransform;
                    tile.WorldPosition = tileTransform.MultiplyPoint(Vector3.zero);
                    
                    Tile.Aggregate(ref chunk.Tiles, ref tile, tileId);
                }
            }
            
            chunk.UpdateShaderData();
            chunk.IsGenerated = true;
            _chunks[chunk.Position] = chunk;
        }

        private void GenerateEmptyChunks(List<Vector2Int> positions)
        {
            positions.ForEach(CreateEmptyChunk);
        }

        private void CreateEmptyChunk(Vector2Int position)
        {
            _chunks[position] = new Chunk(position);
        }
        
        // Used only in editor for a preview of the whole map that ignores camera culling
        private void GenerateChunks()
        {
            if (_previousNumberOfChunks != numberOfChunks)
            {
                _chunks = new Dictionary<Vector2Int, Chunk>(numberOfChunks.x * numberOfChunks.y);
            }
            
            var positions = new List<Vector2Int>(numberOfChunks.x * numberOfChunks.y);
            for (int x = 0; x < numberOfChunks.x; x++)
            {
                for (int y = 0; y < numberOfChunks.y; y++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }
            
            GenerateEmptyChunks(positions);
            
            positions.AsParallel().ForAll(position =>
            {
                var chunk = _chunks[position];
                GenerateChunk(chunk);
            });

            _isGenerated = true;
        }

        private void ResetGenerators()
        {
            Random.InitState(seed);

            _generators?.ForEach(generator =>
            {
                if (generator) generator.editorPropertyChange.RemoveListener(OnRelativeValidate);
            });
            _generators = GetComponents<Generator>().ToList();
            _generators.ForEach(generator =>
            {
                generator.editorPropertyChange.AddListener(OnRelativeValidate);
                generator.OnInit();
            });
            _generators.ForEach(generator => generator.OnSeed());
            _activeGenerators = _generators.Where(generator => generator.enabled).ToList();
        }

        private float CalculateBorderDistance(Vector2Int position)
        {
            var left = position.x;
            var right = (numberOfChunks.x * ChunkSize) - position.x;
            var bottom = position.y;
            var top = (numberOfChunks.y * ChunkSize) - position.y;
            return Mathf.Min(left, right, bottom, top);
        }

        private Tile GenerateTile(Vector2Int gridPos)
        {
            var tile = new Tile
            {
                GridPosition = gridPos,
                Resource = Resource.None,
                DistanceToBorder = CalculateBorderDistance(gridPos),
            };

            _activeGenerators.ForEach(generator =>
            {
                generator.Generate(ref tile);
            });
            return tile; 
        }
        
        private void OnValidate()
        {
            DoGenerate();
        }

        public Vector2Int[] FindChunksInRange(Vector2Int position, float range)
        {
            var chunkPosition = GetChunkPosition(position);
            var scaledRange = Mathf.CeilToInt(range / ChunkSize);
            var width = scaledRange * 2 + 1;
            var chunks = new Vector2Int[width*width];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    var positionX = x + (chunkPosition.x - scaledRange);
                    var positionY = y + (chunkPosition.y - scaledRange);
                    chunks[x + y * width] = new Vector2Int(positionX, positionY);
                }
            }

            return chunks;
        }

        public List<Vector2Int> FindResourcesInRange(Vector2Int position, float range, Resource resource)
        {
            var chunkPositions = FindChunksInRange(position, range);
            var chunks = 
                chunkPositions
                .Where(chunkPosition => _chunks.ContainsKey(chunkPosition))
                .Select(chunkPosition => _chunks[chunkPosition]);

            var resourcePositions = new List<Vector2Int>();
            foreach (var chunk in chunks)
            {
                if (!chunk.IsGenerated)
                {
                    continue;
                }
                for (int i = 0; i < ChunkSize * ChunkSize; i++)
                {
                    var positionInChunk = IndexToLocalChunkPosition(i);
                    var worldPos = 
                        chunk.Position * ChunkSize + new Vector2Int(positionInChunk.x, positionInChunk.y);
                    
                    if (chunk.Tiles.Resource[i] == resource && Vector2Int.Distance(worldPos, position) < range)
                    {
                        resourcePositions.Add(worldPos);
                    }
                }
            }

            return resourcePositions;
        }

        public static Vector2Int IndexToLocalChunkPosition(int i)
        {
            var y = Mathf.FloorToInt((float)i / ChunkSize); // explicit cast to avoid rounding to int
            var x = i - y * ChunkSize;
            return new Vector2Int(x, y);
        }

        public static int LocalChunkPositionToIndex(int x, int y)
        {
            return x + y * ChunkSize;
        }
        
        public static int LocalChunkPositionToIndex(Vector2Int p)
        {
            return LocalChunkPositionToIndex(p.x, p.y);
        }
        

        private void FixedUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            _chunkPositionsInCamera = GetChunkPositionsInCamera();
            _viewportSpaceTilePositions = new List<ViewPortTilePosition>(_chunkPositionsInCamera.Count*(ChunkSize*ChunkSize));
            _chunkPositionsInCamera.ForEach(position =>
            {
                if (!_chunks.ContainsKey(position))
                {
                    CreateEmptyChunk(position);
                }

                var chunk = _chunks[position];

                if (!chunk.IsGenerated && !chunk.IsQueuedForGeneration)
                {
                    chunk.IsQueuedForGeneration = true;
                    ThreadPool.QueueUserWorkItem(state => GenerateChunk(chunk));
                    return;
                }

                if (chunk.NeedsShaderDataUpdate)
                {
                    chunk.UpdateShaderData();
                }

                _chunks[position] = chunk;
            });


            var worldToViewportPoint = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;
            
            _chunkPositionsInCamera.ForEach(position =>
            {
                var chunk = _chunks[position];
                if (chunk.IsGenerated)
                {
                    for (int i = 0; i < ChunkSize * ChunkSize; i++)
                    {
                        var viewportPosition = worldToViewportPoint.MultiplyPoint(chunk.Tiles.WorldPosition[i]);
                        // worldToViewportPoint is a different matrix than what unity uses  so need to convert it
                        viewportPosition /= 2f;
                        viewportPosition += Vector3.one / 2f;
                        if (viewportPosition.x is < 0 or > 1 || viewportPosition.y is < 0 or > 1)
                        {
                            continue;
                        }
                        _viewportSpaceTilePositions.Add(new ViewPortTilePosition
                        {
                            GridPosition = chunk.Tiles.GridPosition[i],
                            ViewportPosition = new Vector2(viewportPosition.x, viewportPosition.y),
                            WorldPosition = chunk.Tiles.WorldPosition[i]
                        });
                    }
                }
            });
        }
        
        private void RenderChunk(Vector2Int position)
        {
            var chunk = _chunks[position];
            if (!chunk.IsGenerated)
            {
                return;
            }
            _renderParams.matProps = chunk.MaterialPropertyBlock;
            Graphics.RenderMeshInstanced(_renderParams, tileMesh, 0, chunk.Transforms);
            foreach (var resAndPos in chunk.Resources)
            {
                if (!resAndPos.Value.Any())
                { 
                    continue;
                }
                _resourceRenderParams[resAndPos.Key].matProps.SetFloatArray("_Ammout", chunk.ResourceAmounts);
                Graphics.RenderMeshInstanced(_resourceRenderParams[resAndPos.Key], _resourceMeshesDict[resAndPos.Key], 0, resAndPos.Value);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying && !_isGenerated)
            {
                return;
            }

            if (Application.isPlaying)
            {
                foreach (var position in _chunkPositionsInCamera)
                {
                    RenderChunk(position);
                }
            }
            else
            {
                foreach (var chunk in _chunks.Keys.Where(c => c.x <= chunkPreviewRange.x && c.y <= chunkPreviewRange.y))
                {
                    RenderChunk(chunk);
                }
            }
            
            // Find tile under cursor
            if (Application.isPlaying)
            {
                var cursorPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);

                var closest = Mathf.Infinity;
                var closestPosition = Vector2Int.zero;
                foreach (var tile in _viewportSpaceTilePositions)
                {
                    var distance = Vector2.Distance(cursorPosition, tile.ViewportPosition);
                    var isCloser = distance < closest;
                    closest = isCloser ? distance : closest;
                    closestPosition = isCloser ? tile.GridPosition : closestPosition;
                }

                SelectedTilePosition = closestPosition;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GetCameraBounds(out var topLeft, out var topRight, out var bottomRight, out var bottomLeft);
            
            Gizmos.color = Color.red;
            var points = new Vector3[]
            {
                topLeft, topRight,
                topRight, bottomRight,
                bottomRight, bottomLeft,
                bottomLeft, topLeft
            };
            Gizmos.DrawLineList(points);
            var chunks = GetChunkPositionsInCamera();
            foreach (var chunk in chunks)
            {
                Gizmos.DrawSphere(new Vector3(chunk.x*ChunkSize, 0, chunk.y*ChunkSize), 1);
            }
        }
#endif
    }
}

