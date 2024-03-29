﻿using Firewind.HabboHotel.Items;

namespace Firewind.HabboHotel.Rooms.Wired.WiredHandlers
{
    struct UserWalksFurniValue
    {
        internal readonly RoomUser user;
        internal readonly RoomItem item;

        public UserWalksFurniValue(RoomUser user, RoomItem item)
        {
            this.user = user;
            this.item = item;
        }
    }
}
