﻿using System;
using System.Text;

using Firewind.Messages;
using Firewind.Util;
using System.Collections.Generic;
using Firewind.Core;

namespace Firewind.HabboHotel.Rooms
{
    internal enum SquareState
    {
        OPEN = 0,
        BLOCKED = 1,
        SEAT = 2,
        POOL = 3 //Should be closed ASAP
    }

    class RoomModel
    {
        //internal string Name;

        internal int DoorX;
        internal int DoorY;
        internal double DoorZ;
        internal int DoorOrientation;

        internal string Heightmap;

        internal SquareState[,] SqState;
        internal short[,] SqFloorHeight;
        internal byte[,] SqSeatRot;

        internal int MapSizeX;
        internal int MapSizeY;

        internal bool ClubOnly;

        internal RoomModel(int DoorX, int DoorY, double DoorZ, int DoorOrientation, string Heightmap, bool ClubOnly)
        {
            try
            {
                this.DoorX = DoorX;
                this.DoorY = DoorY;
                this.DoorZ = DoorZ;
                this.DoorOrientation = DoorOrientation;

                this.Heightmap = Heightmap.ToLower();
                string[] tmpHeightmap = Heightmap.Split(Convert.ToChar(13));

                this.MapSizeX = tmpHeightmap[0].Length;
                this.MapSizeY = tmpHeightmap.Length;
                this.ClubOnly = ClubOnly;

                SqState = new SquareState[MapSizeX, MapSizeY];
                SqFloorHeight = new short[MapSizeX, MapSizeY];
                SqSeatRot = new byte[MapSizeX, MapSizeY];

                //this.Furnis = Furnis;

                for (int y = 0; y < MapSizeY; y++)
                {
                    string line = tmpHeightmap[y];
                    line = line.Replace("\r", "");
                    line = line.Replace("\n", "");

                    int x = 0;
                    foreach (char square in line)
                    {
                        if (square == 'x')
                        {
                            SqState[x, y] = SquareState.BLOCKED;
                        }
                        else
                        {
                            SqState[x, y] = SquareState.OPEN;
                            SqFloorHeight[x, y] = parse(square);
                        }
                        x++;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteLine("Error during room modeldata loading for model " + Heightmap);
                //throw e;
            }
        }

        internal static bool isNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }

        internal static short parse(char input)
        {
            switch (input)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                default:
                    throw new FormatException("The input was not in a correct format: input must be a number between 0 and 9");
            }
        }

        internal static byte parseByte(char input)
        {
            switch (input)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                default:
                    throw new FormatException("The input was not in a correct format: input must be a number between 0 and 9");
            }
        }

        //internal ServerMessage SerializeHeightmap()
        //{
        //    StringBuilder HeightMap = new StringBuilder();

        //    foreach (string MapBit in Heightmap.Split("\r\n".ToCharArray()))
        //    {
        //        if (string.IsNullOrEmpty(MapBit))
        //        {
        //            continue;
        //        }

        //        HeightMap.Append(MapBit);
        //        HeightMap.Append(Convert.ToChar(13));
        //    }

        //    ServerMessage Message = new ServerMessage(31);
        //    Message.AppendString(HeightMap.ToString());
        //    return Message;
        //}

        //internal ServerMessage SerializeRelativeHeightmap()
        //{
        //    ServerMessage Message = new ServerMessage(470);

        //    string[] tmpHeightmap = Heightmap.Split(Convert.ToChar(13));

        //    for (int y = 0; y < MapSizeY; y++)
        //    {
        //        if (y > 0)
        //        {
        //            tmpHeightmap[y] = tmpHeightmap[y].Substring(1);
        //        }

        //        for (int x = 0; x < MapSizeX; x++)
        //        {
        //            string Square = tmpHeightmap[y].Substring(x, 1).Trim().ToLower();

        //            if (DoorX == x && DoorY == y)
        //            {
        //                Square = (int)DoorZ + "";
        //            }

        //            Message.AppendString(Square);
        //        }

        //        Message.AppendString("" + Convert.ToChar(13));
        //    }

        //    return Message;
        //}
    }
}
