﻿using System.Collections.Generic;
using System.Drawing;
using Firewind.HabboHotel.Items;
using Firewind.HabboHotel.Rooms.Games;
using Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;
using System;

namespace Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class PositionReset : IWiredTrigger, IWiredCycleable, IWiredEffect
    {
        private RoomItemHandling roomItemHandler;
        private WiredHandler handler;
        private uint itemID;

        private List<RoomItem> items;
        private int delay;
        private int cycles;

        private bool disposed;

        public PositionReset(List<RoomItem> items, int delay, RoomItemHandling roomItemHandler, WiredHandler handler, uint itemID)
        {
            this.items = items;
            this.delay = delay;
            this.roomItemHandler = roomItemHandler;
            this.cycles = 0;
            this.itemID = itemID;
            this.handler = handler;
            this.disposed = false;
        }

        public bool OnCycle()
        {
            cycles++;
            if (cycles > delay)
            {
                HandleItems();
                return false;
            }
            return true;
        }

        public bool Handle(RoomUser user, Team team, RoomItem item)
        {
            cycles = 0;
            if (delay == 0)
            {
                return HandleItems();
            }
            else
            {
                handler.RequestCycle(this);
            }
            return false;
        }

        private bool HandleItems()
        {
            handler.OnEvent(itemID);
            bool itemIsMoved = false;
            foreach (RoomItem item in items)
            {
                Point oldCoordinate = item.GetPlacementPosition();
                if (roomItemHandler.SetFloorItem(null, item, oldCoordinate.X, oldCoordinate.Y, item.Rot, false, false, true))
                    itemIsMoved = true;
            }

            return itemIsMoved;
        }

        public void Dispose()
        {
            disposed = true;
            roomItemHandler = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
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
            DataRow dRow = dbClient.getRow();
            if (dRow != null)
            {
                this.delay = Convert.ToInt32(dRow[0].ToString());
            }
            else
            {
                delay = 20;
            }

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
