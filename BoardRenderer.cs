using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer {
    public Dictionary<Tuple<int, int>, Chunk> Board = new Dictionary<Tuple<int, int>, Chunk>();
    public int ChunkSize = 10;

    public List<Tuple<int, int>> Showing = new List<Tuple<int, int>>();
    public Dictionary<Tuple<int, int>, List<SpriteRenderer>> ShowingCache = new Dictionary<Tuple<int, int>, List<SpriteRenderer>>();

    private GameManagerScript _gameManager;

    public BoardRenderer(GameManagerScript gameManager){
        _gameManager = gameManager;
    }

    public Chunk RenderBoard(int x, int y)
    {
        //print("Render call");
        if (Board.ContainsKey(new Tuple<int, int>(x,y)))
        {
            return Board[new Tuple<int, int>(x, y)];
        }
        Chunk chunk = new Chunk(ChunkSize, x, y, _gameManager.SavePrefs.BombRate);
        Board[new Tuple<int, int>(x, y)] = chunk;
        return chunk;
    }

    public Tuple<Vector2Int, Vector2Int> PosToPos(Vector2 tap_pos)
    {
        Vector3 world_pos = _gameManager.cam.ScreenToWorldPoint(tap_pos) + new Vector3(ChunkSize / 2, -ChunkSize / 2) + new Vector3(0.5f, -0.5f);
        Vector2Int chunk_pos = Vector2Int.FloorToInt(world_pos / ChunkSize) + new Vector2Int(0, 1);
        world_pos += new Vector3(0, -world_pos.y * 2, 0);

        int pos_y;
        if (world_pos.y > 0)
        {
            pos_y = (int)world_pos.y % ChunkSize;
        }
        else
        {
            pos_y = 9 - ((int)-world_pos.y % ChunkSize);
        }

        int pos_x;
        if (world_pos.x > 0)
        {
            pos_x = (int)world_pos.x % ChunkSize;
        }
        else
        {
            pos_x = 9 + ((int)world_pos.x % ChunkSize);
        }

        Vector2Int pos = new Vector2Int(pos_x, pos_y);

        return new Tuple<Vector2Int, Vector2Int>(chunk_pos, pos);
    }

    public SpriteRenderer RenderCell(byte cell, int x, int y, int nx, int ny)
    {
        SpriteRenderer new_Block;

        if (cell == 0x0 | cell == 0xC)
        {
            new_Block = _gameManager.poolManager.PullSprite(_gameManager.ClosedPrefab);
        }
        else if (cell == 0x9)
        {
            new_Block = _gameManager.poolManager.PullSprite(_gameManager.OpenPrefab);
        }
        else if (cell == 0xD)
        {
            new_Block = _gameManager.poolManager.PullSprite(_gameManager.Bomb);
        }
        else if (cell == 0xA | cell == 0xE) { new_Block = _gameManager.poolManager.PullSprite(_gameManager.FlagPrefab); }
        else if (cell == 0xB | cell == 0xF) { new_Block = _gameManager.poolManager.PullSprite(_gameManager.QPrefab); }
        else
        {
            new_Block = _gameManager.poolManager.PullSprite(_gameManager.NumPrefabs[cell - 1]);
        }

        new_Block.gameObject.SetActive(true);
        new_Block.transform.position = new Vector3(((x * ChunkSize) - (ChunkSize / 2)) + nx, ((y * ChunkSize) - (ChunkSize / 2)) + (ChunkSize - ny), 0);

        return new_Block;
    }

    void Show(int x, int y) 
    {
        //print("Show call");
        if (! Board.ContainsKey(new Tuple<int, int>(x, y)))
        {
            RenderBoard(x, y);
        }

        // RENDERING
        List<SpriteRenderer> cache = new List<SpriteRenderer>();

        for (int ny = 0; ny < ChunkSize; ny++)
        {
            for (int nx = 0; nx < ChunkSize; nx+=2)
            {
                byte cells2 = Board[new Tuple<int, int>(x, y)].Data[(ny * (ChunkSize / 2)) + nx / 2];
                byte[] cellsplit = new byte[2] { (byte)(cells2 >> 4 & 0xF), (byte)(cells2 & 0xF) };

                int index = 0;
                foreach (byte cell in cellsplit)
                {
                    
                    cache.Add(RenderCell(cell, x, y, nx + index, ny));

                    index = 1;
                }
            }
        }

        
        ShowingCache.Add(new Tuple<int, int>(x, y), cache);
    }

    void Hide(int x, int y) 
    {
        //print("Hide call");

        if (ShowingCache.ContainsKey(new Tuple<int, int>(x, y)))
        {
            foreach (SpriteRenderer s in ShowingCache[new Tuple<int, int>(x, y)])
            {
                _gameManager.poolManager.ReleaseSprite(s);
            }
            ShowingCache.Remove(new Tuple<int, int>(x, y));
        }
    }

    void ShowBoard(GameManagerScript game_manager, int[] visible)
    {
        List<Tuple<int, int>> new_showing = new List<Tuple<int, int>>();
        List<Tuple<int, int>> new_new_showing = new List<Tuple<int, int>>();

        for (int x = visible[0]; x <= visible[2]; x++)
        {
            for (int y = visible[1]; y <= visible[3]; y++)
            {
                if (Showing.Contains(new Tuple<int, int>(x, y)))
                {
                    Showing.Remove(new Tuple<int, int>(x, y));
                    new_showing.Add(new Tuple<int, int>(x, y));
                }
                else
                {
                    Show(x, y);
                    new_showing.Add(new Tuple<int, int>(x, y));
                    new_new_showing.Add(new Tuple<int, int>(x, y));
                }
            }

        }

        foreach (Tuple<int, int> chunk_pos in Showing)
        {
            Hide(chunk_pos.Item1, chunk_pos.Item2);
        }

        Showing = new_showing;

        foreach (Tuple<int, int> xy in new_new_showing)
        {
            foreach (Tuple<int, int> open in Board[new Tuple<int, int>(xy.Item1, xy.Item2)].OpenFrom)
            {
                BoardOpener.Ant(game_manager, new Vector2Int(open.Item1, open.Item2), new Vector2Int(xy.Item1, xy.Item2), false);
                //Open(new Vector2Int(xy.Item1, xy.Item2), open);
            }
            Board[new Tuple<int, int>(xy.Item1, xy.Item2)].OpenFrom = new List<Tuple<int, int>>();
        }

        game_manager.menuController.UpdateScore(game_manager.SavePrefs.Score);
    }

    Tuple<Vector2Int, Vector2Int> GetPos(Vector2Int chunkPos, Vector2Int pos)
    {
        if (pos.x >= ChunkSize) { pos = new Vector2Int(0, pos.y); chunkPos += new Vector2Int(1, 0); }
        else if (pos.x < 0) { pos = new Vector2Int(ChunkSize - 1, pos.y); chunkPos -= new Vector2Int(1, 0); }
        if (pos.y >= ChunkSize) { pos = new Vector2Int(pos.x, 0); chunkPos -= new Vector2Int(0, 1); }
        else if (pos.y < 0) { pos = new Vector2Int(pos.x, ChunkSize - 1); chunkPos += new Vector2Int(0, 1); }
        return new Tuple<Vector2Int, Vector2Int>(chunkPos, pos);
    }

    public void Render(){
        float vert_extent = _gameManager.cam.orthographicSize;
        float horz_extent = vert_extent * Screen.width / Screen.height;

        Vector3 min = _gameManager.cam.transform.position - new Vector3(horz_extent, vert_extent, _gameManager.cam.transform.position.z);
        Vector3 max = _gameManager.cam.transform.position + new Vector3(horz_extent+2, vert_extent+2, -_gameManager.cam.transform.position.z);

        Vector3Int minPos = Vector3Int.FloorToInt((min + new Vector3(ChunkSize / 2, ChunkSize / 2)) / ChunkSize);// - new Vector3Int(1, 1, 0);
        Vector3Int maxPos = Vector3Int.FloorToInt((max + new Vector3(ChunkSize / 2, ChunkSize / 2)) / ChunkSize);// + new Vector3Int(1, 1, 0);

        int[] new_vis = new int[4] { minPos[0], minPos[1], maxPos[0], maxPos[1] };

        if (new_vis != _gameManager.visible)
        {
            _gameManager.visible = new_vis;
            ShowBoard(_gameManager, _gameManager.visible);    
        }

        
    }
}