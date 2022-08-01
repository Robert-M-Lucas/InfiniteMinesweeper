using System.Collections.Generic;
using System;

[Serializable]
public class Chunk
{
    public int[] Pos;
    public byte[] Data;
    
    public List<Tuple<int, int>> OpenFrom = new List<Tuple<int, int>>();

    public Chunk(int size, int xpos, int ypos, float bomb_thresh)
    {
        if ((size * size) % 2 != 0)
        {
            Data = new byte[((size * size) / 2) + 1];
        }
        else
        {
            Data = new byte[(size * size) / 2];
        }

        int total_bombs = 0;
        for (int i = 0; i < Data.Length; i++)
        {
            float rand1 = UnityEngine.Random.Range(0f, 1f);
            float rand2 = UnityEngine.Random.Range(0f, 1f);

            if (rand1 < bomb_thresh & rand2 < bomb_thresh)
            {
                Data[i] = 0xCC;
                total_bombs += 2;
            }
            else if (rand1 < bomb_thresh)
            {
                Data[i] = 0xC;
                total_bombs += 1;
            }
            else if (rand2 < bomb_thresh)
            {
                Data[i] = 0xC0;
                total_bombs += 1;
            }

        }

        Pos = new int[2] { xpos, ypos };
    }
}