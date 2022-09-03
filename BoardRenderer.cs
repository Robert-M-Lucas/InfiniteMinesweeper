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

    /// <summary>
    /// Gets or creates a specified chunk
    /// </summary>
    /// <param name="x">Chunk X position</param>
    /// <param name="y">Chunk Y position</param>
    /// <returns>Chunk</returns>
    public Chunk RenderBoard(int x, int y)
    {
        if (Board.ContainsKey(new Tuple<int, int>(x,y)))
        {
            return Board[new Tuple<int, int>(x, y)];
        }
        Chunk chunk = new Chunk(ChunkSize, x, y, _gameManager.SavePrefs.BombRate);
        Board[new Tuple<int, int>(x, y)] = chunk;
        return chunk;
    }

    /// <summary>
    /// Converts a tap position to the corrisponding chunk and cell
    /// </summary>
    /// <param name="tap_pos">Position of tap on screen</param>
    /// <returns>Tuple with the chunk position and the cell position</returns>
    public Tuple<Vector2Int, Vector2Int> ScreenToCellPos(Vector2 tap_pos)
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
        Sprite sprite_to_pull;

        if (cell == 0x0 | cell == 0xC)
        {
            sprite_to_pull = _gameManager.ClosedPrefab;
        }
        else if (cell == 0x9)
        {
            sprite_to_pull = _gameManager.OpenPrefab;
        }
        else if (cell == 0xD)
        {
            sprite_to_pull = _gameManager.Bomb;
        }
        else if (cell == 0xA | cell == 0xE) { sprite_to_pull = _gameManager.FlagPrefab; }
        else if (cell == 0xB | cell == 0xF) { sprite_to_pull = _gameManager.QPrefab; }
        else
        {
            sprite_to_pull = _gameManager.NumPrefabs[cell - 1];
        }

        SpriteRenderer new_block = _gameManager.poolManager.PullSprite(sprite_to_pull);

        new_block.transform.position = new Vector3(((x * ChunkSize) - (ChunkSize / 2)) + nx, ((y * ChunkSize) - (ChunkSize / 2)) + (ChunkSize - ny), 0);

        return new_block;
    }

    /// <summary>
    /// Shows a specified chunk
    /// </summary>
    /// <param name="x">Chunk X position</param>
    /// <param name="y">Chunk Y position</param>
    void ShowChunk(int x, int y) 
    {
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

    /// <summary>
    /// Hides a specified chunk
    /// </summary>
    /// <param name="x">Chunk X position</param>
    /// <param name="y">Chunk Y position</param>
    void HideChunk(int x, int y) 
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

    /// <summary>
    /// Shows all the chunks in a rectangle between two points
    /// </summary>
    /// <param name="game_manager"></param>
    /// <param name="topLeft">Top left point of bounding rectangle</param>
    /// <param name="bottomRight">Bottom right point of bounding rectangle</param>
    void ShowBoard(GameManagerScript game_manager, Vector2Int topLeft, Vector2Int bottomRight)
    {
        List<Tuple<int, int>> new_showing = new List<Tuple<int, int>>();
        List<Tuple<int, int>> new_new_showing = new List<Tuple<int, int>>();

        for (int x = topLeft.x; x <= bottomRight.x; x++)
        {
            for (int y = topLeft.y; y <= bottomRight.y; y++)
            {
                if (Showing.Contains(new Tuple<int, int>(x, y)))
                {
                    Showing.Remove(new Tuple<int, int>(x, y));
                    new_showing.Add(new Tuple<int, int>(x, y));
                }
                else
                {
                    ShowChunk(x, y);
                    new_showing.Add(new Tuple<int, int>(x, y));
                    new_new_showing.Add(new Tuple<int, int>(x, y));
                }
            }

        }

        foreach (Tuple<int, int> chunk_pos in Showing)
        {
            HideChunk(chunk_pos.Item1, chunk_pos.Item2);
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

    /*
    Tuple<Vector2Int, Vector2Int> GetPos(Vector2Int chunkPos, Vector2Int pos)
    {
        if (pos.x >= ChunkSize) { pos = new Vector2Int(0, pos.y); chunkPos += new Vector2Int(1, 0); }
        else if (pos.x < 0) { pos = new Vector2Int(ChunkSize - 1, pos.y); chunkPos -= new Vector2Int(1, 0); }
        if (pos.y >= ChunkSize) { pos = new Vector2Int(pos.x, 0); chunkPos -= new Vector2Int(0, 1); }
        else if (pos.y < 0) { pos = new Vector2Int(pos.x, ChunkSize - 1); chunkPos += new Vector2Int(0, 1); }
        return new Tuple<Vector2Int, Vector2Int>(chunkPos, pos);
    }
    */

    /// <summary>
    /// Renders the board
    /// </summary>
    public void Render(){
        float vert_extent = _gameManager.cam.orthographicSize;
        float horz_extent = vert_extent * Screen.width / Screen.height;

        Vector3 min = _gameManager.cam.transform.position - new Vector3(horz_extent, vert_extent, _gameManager.cam.transform.position.z)*1.2f;
        Vector3 max = _gameManager.cam.transform.position + new Vector3(horz_extent, vert_extent, -_gameManager.cam.transform.position.z)*1.2f;

        Vector3Int minPos = Vector3Int.FloorToInt((min + new Vector3(ChunkSize / 2, ChunkSize / 2)) / ChunkSize);// - new Vector3Int(1, 1, 0);
        Vector3Int maxPos = Vector3Int.CeilToInt((max - new Vector3(ChunkSize / 2, ChunkSize / 2)) / ChunkSize);// + new Vector3Int(1, 1, 0);

        Vector2Int new_vis_min = (Vector2Int)minPos;
        Vector2Int new_vis_max = (Vector2Int)maxPos;

        if (new_vis_min != _gameManager.MinVisible || new_vis_max != _gameManager.MaxVisible)
        {
            _gameManager.MinVisible = new_vis_min;
            _gameManager.MaxVisible = new_vis_max;
            ShowBoard(_gameManager, _gameManager.MinVisible, _gameManager.MaxVisible);    
        }
    }
}