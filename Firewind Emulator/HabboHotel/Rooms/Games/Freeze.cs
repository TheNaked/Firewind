﻿using System.Collections;
using Firewind.HabboHotel.Items;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
using Firewind.Collections;
using Firewind.HabboHotel.Pathfinding;
using Firewind.Messages;
using HabboEvents;
using Firewind.Core;


namespace Firewind.HabboHotel.Rooms.Games
{
    class Freeze
    {
        private Room room;
        private Hashtable freezeTiles;
        private Hashtable freezeBlocks;
        private RoomItem exitTeleport;
        private Random rnd;
        private bool gameStarted;

        internal bool GameIsStarted
        {
            get
            {
                return gameStarted;
            }
        }

        internal RoomItem ExitTeleport
        {
            get
            {
                return exitTeleport;
            }
            set
            {
                exitTeleport = value;
            }
        }

        public Freeze(Room room)
        {
            this.room = room;
            this.freezeTiles = new Hashtable();
            this.freezeBlocks = new Hashtable();
            this.exitTeleport = null;
            this.rnd = new Random();
            this.gameStarted = false;
        }

        internal void StartGame()
        {
            gameStarted = true;
            CountTeamPoints();
            ResetGame();
            
            room.GetGameManager().LockGates();
            room.GetGameManager().StartGame();
        }

        internal void StopGame()
        {
            gameStarted = false;
            room.GetGameManager().UnlockGates();
            room.GetGameManager().StopGame();
            Team winners = room.GetGameManager().getWinningTeam();

            foreach (RoomUser user in room.GetRoomUserManager().UserList.Values)
            {
                user.FreezeLives = 0;
                if (user.team == winners)
                {
                    user.Unidle();

                    user.DanceId = 0;

                    ServerMessage Message = new ServerMessage(Outgoing.Action);
                    Message.AppendInt32(user.VirtualId);
                    Message.AppendInt32(1);
                    room.SendMessage(Message);
                }
            }
        }

        internal static void OnCycle()
        {
         
        }

        internal void CycleUser(RoomUser user)
        {
            if (user.Freezed)
            {
                user.FreezeCounter++;

                if (user.FreezeCounter > 10)
                {
                    user.Freezed = false;
                    user.FreezeCounter = 0;
                    
                    //user.ApplyEffect((int)user.team + 39);
                    ActivateShield(user);
                }
            }

            if (user.shieldActive)
            {
                user.shieldCounter++;

                if (user.shieldCounter > 10)
                {
                    user.shieldActive = false;
                    user.shieldCounter = 10;
                    
                    user.ApplyEffect((int)user.team + 39);
                }
            }
        }

        internal void ResetGame()
        {
            foreach (RoomItem item in freezeBlocks.Values)
            {
                if (!string.IsNullOrEmpty(((StringData)item.data).Data) && !item.GetBaseItem().InteractionType.Equals(InteractionType.freezebluegate) && !item.GetBaseItem().InteractionType.Equals(InteractionType.freezeredgate) && !item.GetBaseItem().InteractionType.Equals(InteractionType.freezegreengate) && !item.GetBaseItem().InteractionType.Equals(InteractionType.freezeyellowgate))
                {
                    ((StringData)item.data).Data = "";
                    
                    item.UpdateState(false, true);
                    room.GetGameMap().AddItemToMap(item, false);
                }
            }
        }

        internal void OnUserWalk(RoomUser user)
        {
            if (!gameStarted || user.team == Team.none)
                return;

            if (user.X == user.GoalX && user.GoalY == user.Y && user.throwBallAtGoal)
            {
                foreach (RoomItem item in freezeTiles.Values)
                {
                    if (item.interactionCountHelper == 0)
                    {
                        if (item.GetX == user.X && item.GetY == user.Y)
                        {
                            item.interactionCountHelper = 1;
                            ((StringData)item.data).Data = "1000";
                            item.UpdateState();
                            item.InteractingUser = user.userID;
                            item.freezePowerUp = user.banzaiPowerUp;
                            item.ReqUpdate(4, true);

                            switch (user.banzaiPowerUp)
                            {
                                case FreezePowerUp.GreenArrow:
                                case FreezePowerUp.OrangeSnowball:
                                    {
                                        user.banzaiPowerUp = FreezePowerUp.None;
                                        break;
                                    }
                            }
                            break;
                        }
                    }
                }
            }

            foreach (RoomItem item in freezeBlocks.Values)
            {
                if (user.X == item.GetX && user.Y == item.GetY)
                {
                    if (item.freezePowerUp != FreezePowerUp.None)
                    {
                        PickUpPowerUp(item, user);
                    }
                }
            }
        }

        private void CountTeamPoints()
        {
            room.GetGameManager().Reset();

            foreach (RoomUser user in room.GetRoomUserManager().UserList.Values)
            {
                if (user.IsBot || user.team == Team.none || user.GetClient() == null)
                    continue;

                user.banzaiPowerUp = FreezePowerUp.None;
                user.FreezeLives = 3;
                user.shieldActive = false;
                user.shieldCounter = 11;

                room.GetGameManager().AddPointToTeam(user.team, 40, null);

                ServerMessage message = new ServerMessage();
                message.Init(Outgoing.UpdateFreezeLives);
                message.AppendInt32(user.InternalRoomID);
                message.AppendInt32(user.FreezeLives);

                user.GetClient().SendMessage(message);
            }
        }

        private static void RemoveUserFromTeam(RoomUser user)
        {
            user.team = Team.none;
            user.ApplyEffect(-1);
        }

        private RoomItem GetFirstTile(int x, int y)
        {
            foreach (RoomItem item in room.GetGameMap().GetCoordinatedItems(new Point(x, y)))
            {
                if (item.GetBaseItem().InteractionType == InteractionType.freezetile)
                    return item;
            }

            return null;
        }

        internal void onFreezeTiles(RoomItem item, FreezePowerUp powerUp, uint userID)
        {
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(userID);
            if (user == null)
                return;

            List<RoomItem> items;

            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5);
                        break;
                    }
                case FreezePowerUp.GreenArrow:
                    {
                        items = GetDiagonalItems(item.GetX, item.GetY, 5);
                        break;
                    }
                case FreezePowerUp.OrangeSnowball:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 5);
                        items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5));
                        break;
                    }
                default:
                    {
                        items = GetVerticalItems(item.GetX, item.GetY, 3);
                        break;
                    }
            }

            HandleBanzaiFreezeItems(items);
        }

        private static void ActivateShield(RoomUser user)
        {
            user.ApplyEffect((int)user.team + 48);
            user.shieldActive = true;
            user.shieldCounter = 0;
        }

        private void HandleBanzaiFreezeItems(List<RoomItem> items)
        {
            foreach (RoomItem item in items)
            {
                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.freezetile:
                        {
                            Logging.WriteLine("Freeze tile founded!");
                            ((StringData)item.data).Data = "11000";
                            item.UpdateState(false, true);
                            continue;
                        }

                    case InteractionType.freezetileblock:
                        {
                            Logging.WriteLine("Freeze block founded!");
                            SetRandomPowerUp(item);
                            item.UpdateState(false, true);
                            continue;
                        }
                    default:
                        {
                            Logging.WriteLine("Other StringData founded! " + item.GetBaseItem().InteractionType);
                            continue;
                        }

                }
            }
        }

        private void SetRandomPowerUp(RoomItem item)
        {
            if (!string.IsNullOrEmpty((string)item.data.GetData()))
                return;

            int next = rnd.Next(1, 14);

            switch (next)
            {
                case 2:
                    {
                        ((StringData)item.data).Data = "2000";
                        item.freezePowerUp = FreezePowerUp.BlueArrow;
                        break;
                    }
                case 3:
                    {
                        ((StringData)item.data).Data = "3000";
                        item.freezePowerUp = FreezePowerUp.Snowballs;
                        break;
                    }
                case 4:
                    {
                        ((StringData)item.data).Data = "4000";
                        item.freezePowerUp = FreezePowerUp.GreenArrow;
                        break;
                    }
                case 5:
                    {
                        ((StringData)item.data).Data = "5000";
                        item.freezePowerUp = FreezePowerUp.OrangeSnowball;
                        break;
                    }
                case 6:
                    {
                        ((StringData)item.data).Data = "6000";
                        item.freezePowerUp = FreezePowerUp.Heart;
                        break;
                    }
                case 7:
                    {
                        ((StringData)item.data).Data = "7000";
                        item.freezePowerUp = FreezePowerUp.Shield;
                        break;
                    }
                default:
                    {
                        ((StringData)item.data).Data = "1000";
                        item.freezePowerUp = FreezePowerUp.None;
                        break;
                    }
            }

            room.GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private void PickUpPowerUp(RoomItem item, RoomUser user)
        {
            switch (item.freezePowerUp)
            {
                case FreezePowerUp.Heart:
                    {
                        if (user.FreezeLives < 3)
                        {
                            user.FreezeLives++;
                            room.GetGameManager().AddPointToTeam(user.team, 10, user);
                        }

                        ServerMessage message = new ServerMessage();
                        message.Init(Outgoing.UpdateFreezeLives);
                        message.AppendInt32(user.InternalRoomID);
                        message.AppendInt32(user.FreezeLives);

                        user.GetClient().SendMessage(message);
                        break;
                    }
                case FreezePowerUp.Shield:
                    {
                        ActivateShield(user);
                        break;
                    }
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                    {
                        user.banzaiPowerUp = item.freezePowerUp;
                        break;
                    }
            }

            item.freezePowerUp = FreezePowerUp.None;
            ((StringData)item.data).Data = "1" + ((StringData)item.data).Data;
            item.UpdateState(false, true);
        }

        internal void AddFreezeTile(RoomItem item)
        {
            freezeTiles.Add(item.Id, item);
        }

        internal void RemoveFreezeTile(uint itemID)
        {
            freezeTiles.Remove(itemID);
        }

        internal void AddFreezeBlock(RoomItem item)
        {
            if (freezeBlocks.ContainsKey(item.Id))
                freezeBlocks.Remove(item.Id);

            freezeBlocks.Add(item.Id, item);
        }

        internal void RemoveFreezeBlock(uint itemID)
        {
            freezeBlocks.Remove(itemID);
        }

        private void HandleUserFreeze(Point point)
        {
            RoomUser user = room.GetGameMap().GetRoomUsers(point).FirstOrDefault(); ;

            if (user != null)
            {
                if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                    return;
                FreezeUser(user);
            }
        }

        private void FreezeUser(RoomUser user)
        {
            if (user.IsBot || user.shieldActive || user.team == Team.none)
                return;

            if (user.Freezed)
            {
                user.Freezed = false;
                user.ApplyEffect((int)user.team + 39);
                return;
            }
            user.Freezed = true;
            user.FreezeCounter = 0;

            user.FreezeLives--;

            if (user.FreezeLives <= 0)
            {
                ServerMessage message2 = new ServerMessage();
                message2.Init(Outgoing.UpdateFreezeLives);
                message2.AppendInt32(user.InternalRoomID);
                message2.AppendInt32(user.FreezeLives);
                user.GetClient().SendMessage(message2);

                user.ApplyEffect(-1);
                room.GetGameManager().AddPointToTeam(user.team, -20, user);
                TeamManager t = room.GetTeamManagerForFreeze();
                t.OnUserLeave(user);      
                user.team = Team.none;
                if (exitTeleport != null)
                    room.GetGameMap().TeleportToItem(user, exitTeleport);
                
                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;

                if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count > 0)
                    this.StopGame(); // yellow team win
                else if (t.BlueTeam.Count > 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count <= 0)
                    this.StopGame(); // blue team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count > 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count <= 0)
                    this.StopGame(); // red team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count > 0 && t.YellowTeam.Count <= 0)
                    this.StopGame(); // green team win
                return;
            }

            room.GetGameManager().AddPointToTeam(user.team, -10, user);
            user.ApplyEffect(12);

            ServerMessage message = new ServerMessage();
            message.Init(Outgoing.UpdateFreezeLives);
            message.AppendInt32(user.InternalRoomID);
            message.AppendInt32(user.FreezeLives);

            user.GetClient().SendMessage(message);
        }

        private static void ExitGame(RoomUser user)
        {
            user.team = Team.none;
        }

        private List<RoomItem> GetVerticalItems(int x, int y, int length)
        {
            List<RoomItem> totalItems = new List<RoomItem>();

            for (int i = 0; i < length; i++)
            {
                Point point = new Point(x + i, y);

                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                Point point = new Point(x, y + i);

                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                Point point = new Point(x - i, y);
                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                Point point = new Point(x, y - i);
                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<RoomItem> GetDiagonalItems(int x, int y, int length)
        {
            List<RoomItem> totalItems = new List<RoomItem>();

            for (int i = 0; i < length; i++)
            {
                Point point = new Point(x + i, y + i);

                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                Point point = new Point(x - i, y - i);
                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                Point point = new Point(x - i, y + i);
                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                Point point = new Point(x + i , y - i);
                List<RoomItem> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<RoomItem> GetItemsForSquare(Point point)
        {
            return room.GetGameMap().GetCoordinatedItems(point);
        }

        private static bool SquareGotFreezeTile(List<RoomItem> items)
        {
            foreach (RoomItem item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.freezetile)
                    return true;
            }

            return false;
        }

        private static bool SquareGotFreezeBlock(List<RoomItem> items)
        {
            foreach (RoomItem item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.freezetileblock)
                    return true;
            }

            return false;
        }

        internal void Destroy()
        {
            freezeBlocks.Clear();
            freezeTiles.Clear();

            room = null;
            freezeTiles = null;
            freezeBlocks = null;
            exitTeleport = null;
            rnd = null;
        }

    }

    enum FreezePowerUp
    {
        None = 0,
        BlueArrow = 1,
        GreenArrow = 2,
        Shield = 3,
        Heart = 4,
        OrangeSnowball = 5,
        Snowballs = 6
    }
}
