﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Firewind.HabboHotel.Items;
using Firewind.HabboHotel.Rooms.Games;
using System.Collections;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;


namespace Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class TeleportToItem : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private Gamemap gamemap;
        private WiredHandler handler;

        private List<RoomItem> items;
        private int delay;
        private int cycles;
        private Queue delayedUsers;
        private Random rnd;
        private uint itemID;
        private bool disposed;

        public TeleportToItem(Gamemap gamemap, WiredHandler handler, List<RoomItem> items, int delay, uint itemID)
        {
            this.gamemap = gamemap;
            this.handler = handler;
            this.items = items;
            this.delay = delay+2;
            this.itemID = itemID;
            this.cycles = 0;
            this.delayedUsers = new Queue();
            this.rnd = new Random();
            this.disposed = false;
        }

        public bool OnCycle()
        {
            cycles++;
            if (cycles > delay)
            {
                if (delayedUsers.Count > 0)
                {
                    lock (delayedUsers.SyncRoot)
                    {
                        while (delayedUsers.Count > 0)
                        {
                            RoomUser user = (RoomUser)delayedUsers.Dequeue();
                            Room room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(user.RoomId);
                            if (room == null || room.GetRoomUserManager().GetRoomUserByHabbo(user.userID) == null)
                                continue;

                            TeleportUser(user);
                        }
                    }
                }
                return false;
            }

            return true;
        }

        public bool Handle(RoomUser user, Team team, RoomItem item)
        {
            if (user == null)
                return false;
            user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(4);
            cycles = 0;
            if (delay == 0 && user != null)
            {
                return TeleportUser(user);
            }
            else
            {
                lock (delayedUsers.SyncRoot)
                {
                    delayedUsers.Enqueue(user);
                }
                handler.RequestCycle(this);
            }
            return false;
        }

        private bool TeleportUser(RoomUser user)
        {
            if (user == null || user.IsBot || user.IsPet || disposed)
                return false;

            if (items.Count > 1)
            {
                int toTest = 0;
                RoomItem item;
                for (int i = 0; i < items.Count; i++)
                {
                    toTest = rnd.Next(0, items.Count);
                    item = items[toTest];

                    if (item.Coordinate != user.Coordinate)
                    {
                        gamemap.TeleportToItem(user, item);
                        user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
                        return true;
                    }
                }
            }
            else if (items.Count == 1)
            {
                gamemap.TeleportToItem(user, items.First());
                user.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ApplyCustomEffect(0);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            disposed = true;
            gamemap = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
            if (delayedUsers != null)
                delayedUsers.Clear();
        }

        public bool IsSpecial(out SpecialEffects function)
        {
            function = SpecialEffects.None;
            return false;
        }
        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, (int)itemID, "integer", string.Empty, delay.ToString(), false);
            lock (items)
            {
                dbClient.runFastQuery("DELETE FROM trigger_in_place WHERE original_trigger = '" + this.itemID + "'");
                foreach (RoomItem i in items)
                {
                    WiredUtillity.SaveTrigger(dbClient, (int)itemID, (int)i.Id);
                }
            }
        }

        public void LoadFromDatabase(IQueryAdapter dbClient, Room insideRoom)
        {
            dbClient.setQuery("SELECT trigger_data FROM trigger_item WHERE trigger_id = @id ");
            dbClient.addParameter("id", (int)this.itemID);
            this.delay = dbClient.getInteger();

            dbClient.setQuery("SELECT triggers_item FROM trigger_in_place WHERE original_trigger = " + this.itemID);
            DataTable dTable = dbClient.getTable();
            RoomItem targetItem;
            foreach (DataRow dRows in dTable.Rows)
            {
                targetItem = insideRoom.GetRoomItemHandler().GetItem(Convert.ToUInt32(dRows[0]));
                if (targetItem == null || this.items.Contains(targetItem))
                    continue;
                this.items.Add(targetItem);
            }
        }

        public void DeleteFromDatabase(IQueryAdapter dbClient)
        {
            dbClient.runFastQuery("DELETE FROM trigger_item WHERE trigger_id = '" + this.itemID + "'");
            dbClient.runFastQuery("DELETE FROM trigger_in_place WHERE original_trigger = '" + this.itemID + "'");
        }

        public bool Disposed()
        {
            return disposed;
        }
    }
}
