using UnityEngine;
using System;
using System.Collections;

public static class BoardOpener
{
    /// <summary>
    /// Starts opening the board from a specified cell
    /// </summary>
    /// <param name="gameManager">Game Manager</param>
    /// <param name="chunkPos">Chunk to start opening from</param>
    /// <param name="cellPos">Cell to start opening from</param>
    public static void Open(GameManagerScript gameManager, Vector2Int chunkPos, Vector2Int cellPos)
    {
        if (gameManager.cursor == 0) // Default cursor
        {
            // If new game guarantee 3x3 around first click is safe
            if (gameManager.newGame)
            {
                gameManager.newGame = false;
                foreach (Vector2Int move in Vector2IntExt.AroundAndCentre)
                {
                    ChangeCell(gameManager, CellValues.CLOSED, chunkPos, cellPos + move);
                }
            }
            // If tapped on number
            if (CellValues.IsNumber(GetCell(gameManager, chunkPos, cellPos)))
            {
                foreach (Vector2Int move in Vector2IntExt.Around)
                {
                    Ant(gameManager, cellPos + move, chunkPos, false);
                }
            }
            // If tapped bomb
            else if (GetCell(gameManager, chunkPos, cellPos) == CellValues.BOMB_CLOSED)
            {
                gameManager.SavePrefs.Score -= 500;
                gameManager.menuController.UpdateScore(gameManager.SavePrefs.Score);
                gameManager.shake.Shake();
                ChangeCell(gameManager, 0xD, chunkPos, cellPos);
                return;
            }
            else
            {
                Ant(gameManager, cellPos, chunkPos,false);
            }
            
            gameManager.menuController.UpdateScore(gameManager.SavePrefs.Score);
        }
        else if (gameManager.cursor == 1) // Flag cursor
        {
            ChangeCell(gameManager, CellValues.ToggleFlag(GetCell(gameManager, chunkPos, cellPos)), chunkPos, cellPos);
        }
        else // Question cursor
        {
            ChangeCell(gameManager, CellValues.ToggleQuestion(GetCell(gameManager, chunkPos, cellPos)), chunkPos, cellPos);
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

    /// <summary>
    /// Changes the value of a cell
    /// </summary>
    /// <param name="gameManager">Game Manager</param>
    /// <param name="newValue">New value of the cell</param>
    /// <param name="chunkPos">Chunk containing the cell</param>
    /// <param name="cellPos">Cell position</param>
    static void ChangeCell (GameManagerScript gameManager, byte newValue, Vector2Int chunkPos, Vector2Int cellPos)
    {
        if (cellPos.x >= gameManager.boardRenderer.ChunkSize) { cellPos = new Vector2Int(0, cellPos.y); chunkPos += new Vector2Int(1, 0); }
        else if (cellPos.x < 0) { cellPos = new Vector2Int(gameManager.boardRenderer.ChunkSize - 1, cellPos.y); chunkPos -= new Vector2Int(1, 0); }
        if (cellPos.y >= gameManager.boardRenderer.ChunkSize) { cellPos = new Vector2Int(cellPos.x, 0); chunkPos -= new Vector2Int(0, 1); }
        else if (cellPos.y < 0) { cellPos = new Vector2Int(cellPos.x, gameManager.boardRenderer.ChunkSize - 1); chunkPos += new Vector2Int(0, 1); }

        byte cells2 = gameManager.boardRenderer.Board[new Tuple<int,int>(chunkPos.x, chunkPos.y)].Data[(cellPos.y * (gameManager.boardRenderer.ChunkSize / 2)) + cellPos.x / 2];

        if (cellPos.x % 2 == 0)
        {
            gameManager.boardRenderer.Board[new Tuple<int, int>(chunkPos.x, chunkPos.y)].Data[(cellPos.y * (gameManager.boardRenderer.ChunkSize / 2)) + cellPos.x / 2] = (byte)((cells2 & 0xF) + (newValue << 4));
        }
        else 
        {
            gameManager.boardRenderer.Board[new Tuple<int, int>(chunkPos.x, chunkPos.y)].Data[(cellPos.y * (gameManager.boardRenderer.ChunkSize / 2)) + cellPos.x / 2] = (byte)(((cells2 >> 4 & 0xF) << 4) + newValue);
        }

        //Hide(ChunkPos.x, ChunkPos.y);
        //Show(ChunkPos.x, ChunkPos.y);

        gameManager.poolManager.ReleaseSprite(gameManager.boardRenderer.ShowingCache[new Tuple<int, int>(chunkPos.x, chunkPos.y)][(cellPos.y * gameManager.boardRenderer.ChunkSize) + cellPos.x]);
        
        gameManager.boardRenderer.ShowingCache[new Tuple<int, int>(chunkPos.x, chunkPos.y)][(cellPos.y * gameManager.boardRenderer.ChunkSize) + cellPos.x] =
        gameManager.boardRenderer.RenderCell(newValue, chunkPos.x, chunkPos.y, cellPos.x, cellPos.y);
    }

    /// <summary>
    /// Gets the value of a cell
    /// </summary>
    /// <param name="gameManager">Game Manager</param>
    /// <param name="chunkPos">Chunk containing the cell</param>
    /// <param name="cellPos">Cell position</param>
    /// <returns></returns>
    public static byte GetCell(GameManagerScript gameManager, Vector2Int chunkPos, Vector2Int cellPos)
    {
        BoardRenderer boardRenderer = gameManager.boardRenderer;
        if (cellPos.x >= boardRenderer.ChunkSize) { cellPos = new Vector2Int(0, cellPos.y); chunkPos += new Vector2Int(1, 0); }
        else if (cellPos.x < 0) { cellPos = new Vector2Int(boardRenderer.ChunkSize - 1, cellPos.y); chunkPos -= new Vector2Int(1, 0); }
        if (cellPos.y >= boardRenderer.ChunkSize) { cellPos = new Vector2Int(cellPos.x, 0); chunkPos -= new Vector2Int(0, 1); }
        else if (cellPos.y < 0) { cellPos = new Vector2Int(cellPos.x, boardRenderer.ChunkSize - 1); chunkPos += new Vector2Int(0, 1); }

        if (!boardRenderer.Board.ContainsKey(new Tuple<int, int>(chunkPos.x, chunkPos.y))) { boardRenderer.RenderBoard(chunkPos.x, chunkPos.y); }

        byte cells2 = boardRenderer.Board[new Tuple<int, int>(chunkPos.x, chunkPos.y)].Data[(cellPos.y * (boardRenderer.ChunkSize / 2)) + cellPos.x / 2];
        byte[] cellsplit = new byte[2] { (byte)(cells2 >> 4 & 0xF), (byte)(cells2 & 0xF) };
        return cellsplit[cellPos.x % 2];
    }

    public static void Ant(GameManagerScript gameManager, Vector2Int cellPos, Vector2Int chunkPos, bool avoidBomb = true)
    {
        if (cellPos.x >= gameManager.boardRenderer.ChunkSize) { cellPos = new Vector2Int(0, cellPos.y); chunkPos += new Vector2Int(1, 0); }
        else if (cellPos.x < 0) { cellPos = new Vector2Int(gameManager.boardRenderer.ChunkSize - 1, cellPos.y); chunkPos -= new Vector2Int(1, 0); }
        if (cellPos.y >= gameManager.boardRenderer.ChunkSize) { cellPos = new Vector2Int(cellPos.x, 0); chunkPos -= new Vector2Int(0, 1); }
        else if (cellPos.y < 0) { cellPos = new Vector2Int(cellPos.x, gameManager.boardRenderer.ChunkSize - 1); chunkPos += new Vector2Int(0, 1); }

        if (!gameManager.boardRenderer.Showing.Contains(new Tuple<int, int>(chunkPos.x, chunkPos.y))) {
            gameManager.boardRenderer.RenderBoard(chunkPos.x, chunkPos.y).OpenFrom.Add(new Tuple<int, int>(cellPos.x, cellPos.y));
            return; 
        }

        byte cell = GetCell(gameManager, chunkPos, cellPos);
        if (cell == CellValues.CLOSED)
        {
            byte neigbours = 0;
            foreach (Vector2Int move in Vector2IntExt.Around)
            {
                if (CellValues.IsBomb(GetCell(gameManager, chunkPos, cellPos + move))) { neigbours ++; }
            }

            if (neigbours == 0) { 
                neigbours = CellValues.OPEN;
            }

            gameManager.SavePrefs.Score += 1;        

            ChangeCell(gameManager, neigbours, chunkPos, cellPos);

            if (neigbours == CellValues.OPEN)
            {
                foreach (Vector2Int move in Vector2IntExt.Around)
                {
                    Ant(gameManager, cellPos + move, chunkPos);
                }
            }
        }
        else if (cell == CellValues.BOMB_CLOSED && avoidBomb == false)
        {
            // Bomb
            gameManager.SavePrefs.Score -= 500;
            gameManager.menuController.UpdateScore(gameManager.SavePrefs.Score);
            gameManager.shake.Shake();
            ChangeCell(gameManager, CellValues.BOMB_OPEN, chunkPos, cellPos);
        }
    }
}