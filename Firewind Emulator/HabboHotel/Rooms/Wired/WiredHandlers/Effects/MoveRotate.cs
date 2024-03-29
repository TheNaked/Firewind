﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Firewind.HabboHotel.Items;
using Firewind.HabboHotel.Rooms.Games;
using Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Database_Manager.Database.Session_Details.Interfaces;

namespace Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Effects
{
    class MoveRotate : MovementManagement, IWiredEffect, IWiredTrigger, IWiredCycleable
    {
        private Room room;
        private WiredHandler handler;
        private uint itemID;

        private MovementState movement;
        private RotationState rotation;
        private List<RoomItem> items;
        
        private int delay;
        private int cycles;

        private bool isDisposed;

        public MoveRotate(MovementState movement, RotationState rotation, List<RoomItem> items, int delay, Room room, WiredHandler handler, uint itemID)
        {
            this.movement = movement;
            this.rotation = rotation;
            this.items = items;
            this.delay = delay;
            this.room = room;
            this.handler = handler;
            this.cycles = 0;
            this.itemID = itemID;
            this.isDisposed = false;
        }

        public bool Handle(RoomUser user, Team team, RoomItem item)
        {
            cycles = 0;

            if (delay == 0)
            {
                HandleItems();
            }
            else
            {
                handler.RequestCycle(this);
            }
            return false;
        }

        public bool OnCycle()
        {
            if (room == null)
                return false;

            cycles++;
            if (cycles > delay)
            {
                cycles = 0;
                HandleItems();
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            isDisposed = true;
            room = null;
            handler = null;
            if (items != null)
                items.Clear();
            items = null;
        }

        private bool HandleItems()
        {
            handler.OnEvent(itemID);
            bool itemHandled = false;
            foreach (RoomItem item in items)
            {
                if (HandleMovement(item))
                    itemHandled = true;
            }

            return itemHandled;
        }

        private bool HandleMovement(RoomItem item)
        {
            Point newPoint = base.HandleMovement(item.Coordinate, movement, item.Rot);
            int newRotation = base.HandleRotation(item.Rot, rotation);

            if (newPoint != item.Coordinate || newRotation != item.Rot)
            {
                if (room.GetGameMap().SquareIsOpen(newPoint.X, newPoint.Y, false))
                    return room.GetRoomItemHandler().SetFloorItem(null, item, newPoint.X, newPoint.Y, newRotation, false, false, true);
            }

            return false;
        }

        public bool IsSpecial(out SpecialEffects function)
        {
            function = SpecialEffects.None;
            return false;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, (int)itemID, "integer", string.Empty, delay.ToString(), false);

            dbClient.setQuery("REPLACE INTO trigger_rotation SET item_id = @id, rotation_status = @rot_id,  movement_status = @mov_id");
            dbClient.addParameter("id", (int)itemID);
            dbClient.addParameter("rot_id", (int)this.rotation);
            dbClient.addParameter("mov_id", (int)this.movement);
            dbClient.runQuery();

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

            dbClient.setQuery("SELECT rotation_status, movement_status FROM trigger_rotation WHERE item_id = @id");
            dbClient.addParameter("id", (int)this.itemID);
            DataRow dRow = dbClient.getRow();
            if (dRow != null)
            {
                this.rotation = (RotationState)Convert.ToInt32(dRow[0]);
                this.movement = (MovementState)Convert.ToInt32(dRow[1]);
            }
            else
            {
                rotation = RotationState.none;
                movement = MovementState.none;
            }


            dbClient.setQuery("SELECT triggers_item FROM trigger_in_place WHERE original_trigger = " + this.itemID);
            DataTable dTable = dbClient.getTable();
            RoomItem targetItem;
            foreach (DataRow dRows in dTable.Rows)
            {
                targetItem = insideRoom.GetRoomItemHandler().GetItem(Convert.ToUInt32(dRows[0]));
                if (targetItem == null || this.items.Contains(targetItem))
                {
                    continue;
                }

                this.items.Add(targetItem);
            }
        }

        public void DeleteFromDatabase(IQueryAdapter dbClient)
        {
            dbClient.runFastQuery("DELETE FROM trigger_item WHERE trigger_id = '" + this.itemID + "'");
            dbClient.runFastQuery("DELETE FROM trigger_in_place WHERE original_trigger = '" + this.itemID +"'");
            dbClient.runFastQuery("DELETE FROM trigger_rotation WHERE item_id = '" + this.itemID + "'");
        }

        public bool Disposed()
        {
            return isDisposed;
        }

    }
}
