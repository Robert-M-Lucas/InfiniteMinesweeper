using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 0 - nothing
 * 1 - 1
 * 2 - 2
 * 3 - 3
 * 4 - 4
 * 5 - 5
 * 6 - 6
 * 7 - 7
 * 8 - 8
 * 9 - open
 * A - flag
 * B - ?
 * C - Bomb (closed)
 * D - Bomb (Open)
 * E - Bomb Flag
 * F - Bomb ?

*/// 1 byte = 2 cells

public static class CellValues
{
    public const byte CLOSED = 0;
    public const byte ONE = 1;
    public const byte TWO = 2;
    public const byte THREE = 3;
    public const byte FOUR = 4;
    public const byte FIVE = 5;
    public const byte SIX = 6;
    public const byte SEVEN = 7;
    public const byte EIGHT = 8;
    public const byte OPEN = 9;
    public const byte FLAG = 0xA;
    public const byte QUESTION = 0xB;
    public const byte BOMB_CLOSED = 0xC;
    public const byte BOMB_OPEN = 0xD;
    public const byte BOMB_FLAG = 0xE;
    public const byte BOMB_QUESTION = 0xF;

    public static bool IsNumber(byte cell)
    {
        return cell >= 1 && cell <= 8;
    }

    public static bool IsBomb(byte cell)
    {
        return cell >= 0xC;
    }

    public static byte ToggleFlag(byte cell)
    {
        switch (cell)
        {
            case FLAG:
                return CLOSED;
            case BOMB_FLAG:
                return BOMB_CLOSED;
            case QUESTION:
                return CLOSED;
            case BOMB_QUESTION:
                return BOMB_CLOSED;
            case CLOSED:
                return FLAG;
            case BOMB_CLOSED:
                return BOMB_FLAG;
            default:
                return cell;
        }
    }

    public static byte ToggleQuestion(byte cell)
    {
        switch (cell)
        {
            case QUESTION:
                return CLOSED;
            case BOMB_QUESTION:
                return BOMB_CLOSED;
            case FLAG:
                return CLOSED;
            case BOMB_FLAG:
                return BOMB_CLOSED;
            case CLOSED:
                return QUESTION;
            case BOMB_CLOSED:
                return BOMB_QUESTION;
            default:
                return cell;
        }
    }
}