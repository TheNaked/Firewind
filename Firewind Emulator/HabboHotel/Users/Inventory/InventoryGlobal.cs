﻿using System.Collections.Generic;
using System.Threading;
using Firewind.Collections;
using Firewind.HabboHotel;
using Firewind.HabboHotel.GameClients;
using Firewind.HabboHotel.Users.Inventory;
using System;
using Firewind.Core;
using Firewind.HabboHotel.Users.UserDataManagement;

namespace Firewind.HabboHotel.Users.Inventory
{
    class InventoryGlobal
    {

        public InventoryGlobal()
        {
            //storage = new SafeDictionary<uint, InventoryComponent>();
            //globalThread = new Thread(new ThreadStart(WorkItem));
            //globalThread.Start();
        }

        private static void WorkItem()
        {
            //while (true)
            //{
            //    try
            //    {
            //        lock (storage)
            //        {
            //            foreach (InventoryComponent userComponent in storage.Values)
            //                if (userComponent.NeedsUpdate && userComponent.isInactive)
            //                    userComponent.RunCycleUpdate();
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Logging.HandleException(e, "InventoryGlobal.WorkItem()");
            //    }
            //    Thread.Sleep(10000);
            //}
        }

        internal static InventoryComponent GetInventory(uint UserId, GameClient Client, UserData data)
        {
            return new InventoryComponent(UserId, Client, data);
            //InventoryComponent component;
            //if (storage.TryGetValue(UserId, out component))
            //    return component;
            //else
            //{
            //    InventoryComponent toReturn = 
            //    storage.Add(UserId, toReturn);
            //    return toReturn;
            //}
        }

        //internal InventoryComponent GetInventory(uint UserId)
        //{
        //    //InventoryComponent component;
        //    //if (storage.TryGetValue(UserId, out component))
        //    //    return component;

        //    return null;
        //}

        internal void saveAll()
        {
        //    List<uint> toRemove = new List<uint>();
        //    foreach (InventoryComponent component in storage.Values)
        //    {
        //        component.RunDBUpdate();
        //        if (component.isInactive)
        //            toRemove.Add(component.UserId);
        //    }
        }
    }
}
