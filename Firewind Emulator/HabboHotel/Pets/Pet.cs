﻿using System;

using Firewind.HabboHotel.Rooms;
using Firewind.Messages;
using HabboEvents;

namespace Firewind.HabboHotel.Pets
{
    class Pet
    {
        internal uint PetId;
        internal uint OwnerId;
        internal int VirtualId;

        internal uint Type;
        internal string Name;
        internal string Race;
        internal string Color;

        internal int Expirience;
        internal int Energy;
        internal int Nutrition;

        internal uint RoomId;
        internal int X;
        internal int Y;
        internal double Z;

        internal int Respect;

        internal double CreationStamp;
        internal bool PlacedInRoom;

        internal int[] experienceLevels = new int[] { 100, 200, 400, 600, 1000, 1300, 1800, 2400, 3200, 4300, 7200, 8500, 10100, 13300, 17500, 23000, 51900 }; // ty scott
        internal DatabaseUpdateState DBState;

        internal Room Room
        {
            get
            {
                if (!IsInRoom)
                {
                    return null;
                }

                return FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);
            }
        }

        internal bool IsInRoom
        {
            get
            {
                return (RoomId > 0);
            }
        }

        internal int Level
        {
            get
            {
                for (int level = 0; level < experienceLevels.Length; ++level)
                {
                    if (Expirience < experienceLevels[level])
                        return level + 1;
                }
                return experienceLevels.Length + 1;
            }
        }

        internal static int MaxLevel
        {
            get
            {
                return 20;
            }
        }

        internal int ExpirienceGoal
        {
            get
            {
                return experienceLevels[Level - 1];
            }
        }

        internal static int MaxEnergy
        {
            get
            {
                return 100;
            }
        }

        internal static int MaxNutrition
        {
            get
            {
                return 150;
            }
        }

        internal int Age
        {
            get
            {
                return (int)Math.Floor((FirewindEnvironment.GetUnixTimestamp() - CreationStamp) / 86400);
            }
        }

        // typeId + paletteId + color + unkcount + all unk
        internal string Look
        {
            get
            {
                return Type + " " + Race + " " + Color;
            }
        }

        internal string OwnerName
        {
            get
            {
                return FirewindEnvironment.GetGame().GetClientManager().GetNameById(OwnerId);
            }
        }

        /*
        internal int BasketX
        {
        get
        {
        try
        {
        using (DatabaseClient dbClient = FirewindEnvironment.GetDatabase().GetClient())
        {
        dbClient.addParameter("rID", RoomId);
        int ItemX = dbClient.ReadInt32("SELECT x FROM room_items WHERE room_id = @rID AND base_item = 317");

        // 317- nest
        return ItemX;
        }
        }
        catch { return 0; } // Doesn't Exist
        }
        }

        internal int BasketY
        {
        get
        {
        try
        {
        using (DatabaseClient dbClient = FirewindEnvironment.GetDatabase().GetClient())
        {
        dbClient.addParameter("rID", RoomId);
        int ItemY = dbClient.ReadInt32("SELECT y FROM room_items WHERE room_id = @rID AND base_item = 317");

        // 317- nest
        return ItemY;
        }
        }
        catch { return 0; } // Doesn't Exist
        }
        }
        */

        internal bool HaveSaddle;

        internal Pet(uint PetId, uint OwnerId, uint RoomId, string Name, uint Type, string Race, string Color, int Expirience, int Energy, int Nutrition, int Respect, double CreationStamp, int X, int Y, double Z, bool havesaddle)
        {
            this.PetId = PetId;
            this.OwnerId = OwnerId;
            this.RoomId = RoomId;
            this.Name = Name;
            this.Type = Type;
            this.Race = Race;
            this.Color = Color;
            this.Expirience = Expirience;
            this.Energy = Energy;
            this.Nutrition = Nutrition;
            this.Respect = Respect;
            this.CreationStamp = CreationStamp;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.PlacedInRoom = false;
            this.DBState = DatabaseUpdateState.Updated;
            this.HaveSaddle = havesaddle;
        }

        internal void OnRespect()
        {
            Respect++;

            ServerMessage Message = new ServerMessage(Outgoing.RespectPet);
            Message.AppendInt32(VirtualId); // userId
            Message.AppendInt32(Respect); // respectTotal
            //Room.SendMessage(Message);

            Message = new ServerMessage(Outgoing.PetRespectNotification);

            Message.AppendUInt(OwnerId); // petOwnerId
            Message.AppendInt32(1); // respect
            SerializeInventory(Message);
            //Message.AppendUInt(PetId); // id
            //Message.AppendString(Name); // name
            //Message.AppendUInt(Type); // type
            //Message.AppendInt32(int.Parse(Race)); // breed
            //Message.AppendString(Color); // color

            Room.SendMessage(Message);

            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;
            

            if (Expirience <= 51900)
            {
                AddExpirience(10);
            }
        }

        internal void AddExpirience(int Amount)
        {
            Expirience = Expirience + Amount;

            if (Expirience >= 51900)
                return;

            //using (DatabaseClient dbClient = FirewindEnvironment.GetDatabase().GetClient())
            //{
            //    dbClient.addParameter("petid", PetId);
            //    dbClient.addParameter("expirience", Expirience);
            //    dbClient.runFastQuery("UPDATE user_pets SET expirience = @expirience WHERE id = @petid LIMIT 1");
            //}
            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;

            if (Room != null)
            {
                ServerMessage Message = new ServerMessage(Outgoing.AddExperience);
                Message.AppendUInt(PetId); // petId
                Message.AppendInt32(VirtualId); // petRoomIndex
                Message.AppendInt32(Amount); // gainedExperience
                Room.SendMessage(Message);

                if (Expirience >= ExpirienceGoal)
                {
                    // Level the pet

                    ServerMessage ChatMessage = new ServerMessage(Outgoing.Talk);
                    ChatMessage.AppendInt32(VirtualId);
                    ChatMessage.AppendString("*leveled up to level " + Level + " *");
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(0);
                    ChatMessage.AppendInt32(-1);
                    Room.SendMessage(ChatMessage);
                }
            }

        }

        internal void PetEnergy(bool Add)
        {
            int MaxE;

            if (Add)
            {
                if (this.Energy == 100) // If Energy is 100, no point.
                    return;

                if (this.Energy > 85) { MaxE = MaxEnergy - this.Energy; } else { MaxE = 10; }

            }
            else { MaxE = 15; } // Remove Max Energy as 15

            int r = FirewindEnvironment.GetRandomNumber(4, MaxE);

            //using (DatabaseClient dbClient = FirewindEnvironment.GetDatabase().GetClient())
            {
                if (!Add)
                {
                    this.Energy = this.Energy - r;

                    if (this.Energy < 0)
                    {
                        //dbClient.addParameter("pid", PetId);
                        //dbClient.runFastQuery("UPDATE user_pets SET energy = 1 WHERE id = @pid LIMIT 1");

                        this.Energy = 1;

                        r = 1;
                    }

                    //dbClient.addParameter("r", r);
                    //dbClient.addParameter("petid", PetId);
                    //dbClient.runFastQuery("UPDATE user_pets SET energy = energy - @r WHERE id = @petid LIMIT 1");

                }
                else
                {
                    //dbClient.addParameter("r", r);
                    //dbClient.addParameter("petid", PetId);
                    //dbClient.runFastQuery("UPDATE user_pets SET energy = energy + @r WHERE id = @petid LIMIT 1");
                    
                    this.Energy = this.Energy + r;
                }
            }
            if (DBState != DatabaseUpdateState.NeedsInsert)
                DBState = DatabaseUpdateState.NeedsUpdate;
        }

        internal void SerializeInventory(ServerMessage Message)
        {
            Message.AppendUInt(PetId); // id
            Message.AppendString(Name); // name

            // ### FIGURE DATA PART
            Message.AppendUInt(Type); // typeId
            Message.AppendInt32(int.Parse(Race)); // paletteId
            Message.AppendString(Color); // color
            Message.AppendInt32(0); // unknown
            Message.AppendInt32(0); // somthing-count
            // ###

            Message.AppendInt32(Level); // level
        }

        internal ServerMessage SerializeInfo()
        {
            ServerMessage Nfo = new ServerMessage(Outgoing.PetInformation);
            Nfo.AppendUInt(PetId);
            Nfo.AppendString(Name);
            Nfo.AppendInt32(Level);
            Nfo.AppendInt32(MaxLevel);
            Nfo.AppendInt32(Expirience);
            Nfo.AppendInt32(ExpirienceGoal);
            Nfo.AppendInt32(Energy);
            Nfo.AppendInt32(MaxEnergy);
            Nfo.AppendInt32(Nutrition);
            Nfo.AppendInt32(MaxNutrition);
            //Nfo.AppendString(Color.ToLower());
            Nfo.AppendInt32(Respect);
            Nfo.AppendUInt(OwnerId);
            Nfo.AppendInt32(Age);
            Nfo.AppendString(OwnerName);
            Nfo.AppendInt32(1);
            //Logging.WriteLine("have saddle: " + HaveSaddle);

            Nfo.AppendBoolean(HaveSaddle);

            Nfo.AppendBoolean(FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomId).GetRoomUserManager().GetRoomUserByVirtualId(VirtualId).isMounted);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendInt32(0);
            Nfo.AppendString("");
            Nfo.AppendBoolean(false);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendInt32(-1);
            Nfo.AppendBoolean(false);
            return Nfo;
        }
    }

    internal enum DatabaseUpdateState
    {
        Updated,
        NeedsUpdate,
        NeedsInsert
    }
}
