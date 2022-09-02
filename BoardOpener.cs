using UnityEngine;
using System;
using System.Collections;

public static class BoardOpener
{
    public static void Open(GameManagerScript game_manager, Vector2Int chunk_pos, Vector2Int pos)
    {
        if (game_manager.cursor == 0) // Default cursor
        if (game_manager.cursor == 0) // Default cursor
        {
            // If new game guarantee 3x3 around first click is safe
            if (game_manager.newGame)
            {
                game_manager.newGame = false;
                ChangeCell(game_manager, CellValues.CLOSED, chunk_pos, pos);
                foreach (Vector2Int move in Vector2IntExt.Around)
                {
                    ChangeCell(game_manager, CellValues.CLOSED, chunk_pos, pos + move);
                }
            }
            // If tapped on number
            if (CellValues.IsNumber(GetCell(game_manager, chunk_pos, pos)))
            {
                foreach (Vector2Int move in Vector2IntExt.Around)
                {
                    Ant(game_manager, pos + move, chunk_pos, false);
                }
            }
            // If tapped bomb
            else if (GetCell(game_manager, chunk_pos, pos) == CellValues.BOMB_CLOSED)
            {
                game_manager.SavePrefs.Score -= 500;
                game_manager.menuController.UpdateScore(game_manager.SavePrefs.Score);
                game_manager.shake.ShakeMe();
                ChangeCell(game_manager, 0xD, chunk_pos, pos);
                return;
            }
            else
            {
                Ant(game_manager, pos, chunk_pos,false);
            }
            
            game_manager.menuController.UpdateScore(game_manager.SavePrefs.Score);
        }
        else if (game_manager.cursor == 1) // Flag cursor
        {
            ChangeCell(game_manager, CellValues.ToggleFlag(GetCell(game_manager, chunk_pos, pos)), chunk_pos, pos);
        }
        else // Question cursor
        {
            ChangeCell(game_manager, CellValues.ToggleQuestion(GetCell(game_manager, chunk_pos, pos)), chunk_pos, pos);
        }

        /*
        if (!Premium)
        {
            AdCount += 1;
            if (AdCount > 75 && Advertisement.IsReady())
            {
                Advertisement.Show();
                AdCount = 0;
            }
        }
        */
    }

    static void ChangeCell (GameManagerScript game_manager, byte new_value, Vector2Int chunk_pos, Vector2Int pos)
    {
        if (pos.x >= game_manager.boardRenderer.ChunkSize) { pos = new Vector2Int(0, pos.y); chunk_pos += new Vector2Int(1, 0); }
        else if (pos.x < 0) { pos = new Vector2Int(game_manager.boardRenderer.ChunkSize - 1, pos.y); chunk_pos -= new Vector2Int(1, 0); }
        if (pos.y >= game_manager.boardRenderer.ChunkSize) { pos = new Vector2Int(pos.x, 0); chunk_pos -= new Vector2Int(0, 1); }
        else if (pos.y < 0) { pos = new Vector2Int(pos.x, game_manager.boardRenderer.ChunkSize - 1); chunk_pos += new Vector2Int(0, 1); }

        byte cells2 = game_manager.boardRenderer.Board[new Tuple<int,int>(chunk_pos.x, chunk_pos.y)].Data[(pos.y * (game_manager.boardRenderer.ChunkSize / 2)) + pos.x / 2];

        if (pos.x % 2 == 0)
        {
            game_manager.boardRenderer.Board[new Tuple<int, int>(chunk_pos.x, chunk_pos.y)].Data[(pos.y * (game_manager.boardRenderer.ChunkSize / 2)) + pos.x / 2] = (byte)((cells2 & 0xF) + (new_value << 4));
        }
        else 
        {
            game_manager.boardRenderer.Board[new Tuple<int, int>(chunk_pos.x, chunk_pos.y)].Data[(pos.y * (game_manager.boardRenderer.ChunkSize / 2)) + pos.x / 2] = (byte)(((cells2 >> 4 & 0xF) << 4) + new_value);
        }

        //Hide(ChunkPos.x, ChunkPos.y);
        //Show(ChunkPos.x, ChunkPos.y);

        game_manager.poolManager.ReleaseSprite(game_manager.boardRenderer.ShowingCache[new Tuple<int, int>(chunk_pos.x, chunk_pos.y)][(pos.y * game_manager.boardRenderer.ChunkSize) + pos.x]);
        
        game_manager.boardRenderer.ShowingCache[new Tuple<int, int>(chunk_pos.x, chunk_pos.y)][(pos.y * game_manager.boardRenderer.ChunkSize) + pos.x] =
        game_manager.boardRenderer.RenderCell(new_value, chunk_pos.x, chunk_pos.y, pos.x, pos.y);
    }

    public static byte GetCell(GameManagerScript game_manager, Vector2Int chunk_pos, Vector2Int pos)
    {
        BoardRenderer boardRenderer = game_manager.boardRenderer;
        if (pos.x >= boardRenderer.ChunkSize) { pos = new Vector2Int(0, pos.y); chunk_pos += new Vector2Int(1, 0); }
        else if (pos.x < 0) { pos = new Vector2Int(boardRenderer.ChunkSize - 1, pos.y); chunk_pos -= new Vector2Int(1, 0); }
        if (pos.y >= boardRenderer.ChunkSize) { pos = new Vector2Int(pos.x, 0); chunk_pos -= new Vector2Int(0, 1); }
        else if (pos.y < 0) { pos = new Vector2Int(pos.x, boardRenderer.ChunkSize - 1); chunk_pos += new Vector2Int(0, 1); }

        if (!boardRenderer.Board.ContainsKey(new Tuple<int, int>(chunk_pos.x, chunk_pos.y))) { boardRenderer.RenderBoard(chunk_pos.x, chunk_pos.y); }

        byte cells2 = boardRenderer.Board[new Tuple<int, int>(chunk_pos.x, chunk_pos.y)].Data[(pos.y * (boardRenderer.ChunkSize / 2)) + pos.x / 2];
        byte[] cellsplit = new byte[2] { (byte)(cells2 >> 4 & 0xF), (byte)(cells2 & 0xF) };
        return cellsplit[pos.x % 2];
    }

    public static void Ant(GameManagerScript game_manager, Vector2Int pos, Vector2Int chunk_pos, bool avoid_bomb = true)
    {
        if (pos.x >= game_manager.boardRenderer.ChunkSize) { pos = new Vector2Int(0, pos.y); chunk_pos += new Vector2Int(1, 0); }
        else if (pos.x < 0) { pos = new Vector2Int(game_manager.boardRenderer.ChunkSize - 1, pos.y); chunk_pos -= new Vector2Int(1, 0); }
        if (pos.y >= game_manager.boardRenderer.ChunkSize) { pos = new Vector2Int(pos.x, 0); chunk_pos -= new Vector2Int(0, 1); }
        else if (pos.y < 0) { pos = new Vector2Int(pos.x, game_manager.boardRenderer.ChunkSize - 1); chunk_pos += new Vector2Int(0, 1); }

        if (!game_manager.boardRenderer.Showing.Contains(new Tuple<int, int>(chunk_pos.x, chunk_pos.y))) {
            game_manager.boardRenderer.RenderBoard(chunk_pos.x, chunk_pos.y).OpenFrom.Add(new Tuple<int, int>(pos.x, pos.y));
            return; 
        }

        

        byte cell = GetCell(game_manager, chunk_pos, pos);
        if (cell == CellValues.CLOSED)
        {
            byte neigbours = 0;
            foreach (Vector2Int move in Vector2IntExt.Around)
            {
                if (CellValues.IsBomb(GetCell(game_manager, chunk_pos, pos + move))) { neigbours += 1; }
            }

            if (neigbours == 0) { 
                neigbours = CellValues.OPEN;
            }

            game_manager.SavePrefs.Score += 1;        

            ChangeCell(game_manager, neigbours, chunk_pos, pos);

            if (neigbours == CellValues.OPEN)
            {
                foreach (Vector2Int move in Vector2IntExt.Around)
                {
                    Ant(game_manager, pos + move, chunk_pos);
                }
            }
        }
        else if (cell == CellValues.BOMB_CLOSED && avoid_bomb == false)
        {
            // Bomb
            game_manager.SavePrefs.Score -= 500;
            game_manager.menuController.UpdateScore(game_manager.SavePrefs.Score);
            game_manager.shake.ShakeMe();
            ChangeCell(game_manager, CellValues.BOMB_OPEN, chunk_pos, pos);
        }
    }
}