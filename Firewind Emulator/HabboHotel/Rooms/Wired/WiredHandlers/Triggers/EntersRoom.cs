﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Interfaces;
using Firewind.HabboHotel.Items;
using Firewind.HabboHotel.Rooms.Games;
using Database_Manager.Database.Session_Details.Interfaces;
using System.Data;

namespace Firewind.HabboHotel.Rooms.Wired.WiredHandlers.Triggers
{
    class EntersRoom : IWiredTrigger
    {
        private RoomItem item;
        private WiredHandler handler;
        private bool isOneUser;
        private string userName;
        private RoomEventDelegate delegateFunction;

        public EntersRoom(RoomItem item, WiredHandler handler, RoomUserManager roomUserManager, bool isOneUser, string userName)
        {
            this.item = item;
            this.handler = handler;
            this.isOneUser = isOneUser;
            this.userName = userName;
            this.delegateFunction = new RoomEventDelegate(roomUserManager_OnUserEnter);

            roomUserManager.OnUserEnter += delegateFunction;
        }

        private void roomUserManager_OnUserEnter(object sender, EventArgs e)
        {
            RoomUser user = (RoomUser)sender;

            if ((!user.IsBot && isOneUser && !string.IsNullOrEmpty(userName) && user.GetUsername() == userName) || !isOneUser)
            {
                handler.OnEvent(item.Id);
                handler.RequestStackHandle(item.Coordinate, null, user, Team.none);
            }
        }

        public void Dispose()
        {
            handler = null;
            userName = null;
            if (item != null && item.GetRoom() != null)
                item.GetRoom().GetRoomUserManager().OnUserEnter -= delegateFunction;
            item = null;
        }

        public void SaveToDatabase(IQueryAdapter dbClient)
        {
            WiredUtillity.SaveTriggerItem(dbClient, (int)item.Id, "integer", string.Empty, userName, false);
        }

        public void LoadFromDatabase(IQueryAdapter dbClient, Room insideRoom)
        {
            dbClient.setQuery("SELECT trigger_data FROM trigger_item WHERE trigger_id = @id ");
            dbClient.addParameter("id", (int)this.item.Id);
            DataRow dRow = dbClient.getRow();
            if (dRow != null)
                this.userName = dRow[0].ToString();
            else
                this.userName = string.Empty;
            this.isOneUser = !string.IsNullOrEmpty(this.userName);
        }

        public void DeleteFromDatabase(IQueryAdapter dbClient)
        {
            dbClient.runFastQuery("DELETE FROM trigger_item WHERE trigger_id = '" + this.item.Id + "'");
        }

    }
}
