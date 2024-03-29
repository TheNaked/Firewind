﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Firewind.Collections;
using Firewind.Core;
using Firewind.HabboHotel.ChatMessageStorage;
using Firewind.HabboHotel.GameClients;
using Firewind.HabboHotel.Rooms;
using Firewind.HabboHotel.Rooms.RoomIvokedItems;
using Firewind.Messages;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;

namespace Firewind.HabboHotel.Support
{
    public class ModerationTool
    {
        #region General

        internal List<SupportTicket> Tickets;

        internal List<string> UserMessagePresets;
        internal List<string> RoomMessagePresets;

        internal ModerationTool()
        {
            Tickets = new List<SupportTicket>();
            UserMessagePresets = new List<string>();
            RoomMessagePresets = new List<string>();
        }

        internal ServerMessage SerializeTool()
        {
            ServerMessage Response = new ServerMessage(Outgoing.OpenModTools);
            Response.AppendInt32(-1);

            Response.AppendInt32(UserMessagePresets.Count);
                foreach (String Preset in UserMessagePresets)
                {
                    Response.AppendString(Preset);
                }
            Response.AppendUInt(6); //Mod Actions Count

            Response.AppendString("Room Problems"); // modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Door Problem"); // mod action Cata
            Response.AppendString("Stop blocking the doors Please"); // Msg
            Response.AppendString("Ban-Problem"); // Mod Action Cata
            Response.AppendString("This your last warning or you received a ban"); // Msg
            Response.AppendString("Help Support");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Bobba Filter"); // Mod Cata
            Response.AppendString("Please stop Playing with the Bobba Filter"); // Msg
            Response.AppendString("Kick user"); // Mod Cata
            Response.AppendString("Please stop Kicking people without a Reason"); // Msg
            Response.AppendString("Ban Room"); // Mod Cata
            Response.AppendString("Please stop banning people without a good reason"); // Msg
            Response.AppendString("RoomName"); // Mod Cata
            Response.AppendString("Please Change your RoomName otherwish we will do it."); // Msg
            Response.AppendString("PH box"); // Mod Cata
            Response.AppendString("Please Contact A administrator about your problem"); // Msg

            Response.AppendString("Player Support");// modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Player Bug"); // mod action Cata
            Response.AppendString("Thank you for supporting us and reporting this bug"); // Msg
            Response.AppendString("Login Problem"); // Mod Action Cata
            Response.AppendString("We contact to the player support and will work on your problem"); // Msg
            Response.AppendString("Help Support");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Call for help Filter"); // Mod Cata
            Response.AppendString("Please stop Playing with the CFH Filter"); // Msg
            Response.AppendString("Privacy"); // Mod Cata
            Response.AppendString("You should not give your privacy stuff away."); // Msg
            Response.AppendString("Warning Swf."); // Mod Cata
            Response.AppendString("Please Contact a administrator/moderator."); // Msg
            Response.AppendString("Cache"); // Mod Cata
            Response.AppendString("if you got problems with memmory Please report it"); // Msg
            Response.AppendString("Temp Problem"); // Mod Cata
            Response.AppendString("Delete your temp!"); // Msg

            Response.AppendString("Users Problems");// modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Scamming"); // mod action Cata
            Response.AppendString("We will Check the problem for you now can you give us more infomation about what's happens"); // Msg
            Response.AppendString("Trading Scam"); // Mod Action Cata
            Response.AppendString("What happens and how happens please explain us"); // Msg
            Response.AppendString("Disconnection");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Room blocking"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Freezing"); // Mod Cata
            Response.AppendString("Can explain us what happens?"); // Msg
            Response.AppendString("Error page"); // Mod Cata
            Response.AppendString("What was the code from your error Example 404"); // Msg
            Response.AppendString("Can't login"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Updates"); // Mod Cata
            Response.AppendString("We always do our best to update everything"); // Msg

            Response.AppendString("Debug Problems");// modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Lag"); // mod action Cata
            Response.AppendString("We will Check the problem for you now can you give us more infomation about what's happens"); // Msg
            Response.AppendString("Disconnection"); // Mod Action Cata
            Response.AppendString("What happens and how happens please explain us"); // Msg
            Response.AppendString("SSO problem");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Char Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("System check"); // Mod Cata
            Response.AppendString("We already checking every debug stuff"); // Msg
            Response.AppendString("Error from WireEncoding"); // Mod Cata
            Response.AppendString("Can you say explain us what happens"); // Msg
            Response.AppendString("Error from BASE64"); // Mod Cata
            Response.AppendString("Can you explain us what happens"); // Msg
            Response.AppendString("Error from Flash player"); // Mod Cata
            Response.AppendString("Can you explain us what happens"); // Msg

            Response.AppendString("System Problems");// modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Version"); // mod action Cata
            Response.AppendString("Ask a Administrator For more Information"); // Msg
            Response.AppendString("Swf Version?"); // Mod Action Cata
            Response.AppendString("Currenly We use the same version like Habbo.com"); // Msg
            Response.AppendString("Freeze");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Name Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Nickname Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("System Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Cursing Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Update Filter"); // Mod Cata
            Response.AppendString("We will update your words in the filter Thanks for report."); // Msg

            Response.AppendString("Swf Problems");// modaction Cata
            Response.AppendUInt(8); // ModAction Count
            Response.AppendString("Version"); // mod action Cata
            Response.AppendString("Ask a Administrator For more Information"); // Msg
            Response.AppendString("Swf Version?"); // Mod Action Cata
            Response.AppendString("Currenly We use the same version like Habbo.com"); // Msg
            Response.AppendString("Freeze");// Mod Action Cata
            Response.AppendString("Please Contact The Player Support For this problem"); // Msg
            Response.AppendString("Name Filter"); // Mod Cata
            Response.AppendString("Can you say The usersname and explain us what happens"); // Msg
            Response.AppendString("Crash on loading"); // Mod Cata
            Response.AppendString("Can you explain us what happens"); // Msg
            Response.AppendString("Crash on login"); // Mod Cata
            Response.AppendString("Can you say the usersname and explain us what happens"); // Msg
            Response.AppendString("Crash in room"); // Mod Cata
            Response.AppendString("Can you say the usersname and explain us what happens"); // Msg
            Response.AppendString("Website error"); // Mod Cata
            Response.AppendString("Can you say the usersname and explain us what happens"); // Msg

            Response.AppendBoolean(true); // ticket_queue fuse
            Response.AppendBoolean(true); // chatlog fuse
            Response.AppendBoolean(true); // message / caution fuse
            Response.AppendBoolean(true); // kick fuse
            Response.AppendBoolean(true); // band fuse
            Response.AppendBoolean(true); // broadcastshit fuse
            Response.AppendBoolean(true); // other StringData fuse

            Response.AppendInt32(RoomMessagePresets.Count);
                foreach (String Preset in RoomMessagePresets)
                {
                    Response.AppendString(Preset);
                }
            Response.AppendInt32(0); // undef
            Response.AppendString(""); // undef
            Response.AppendString(""); // undef
            Response.AppendInt32(0); // count of actions
            /*Response.AppendInt32(0); // actionType
            Response.AppendInt32(0); // actionLen
            Response.AppendInt32(0); // action multiplier (repeat x times)
             */
            return Response;
        }

        #endregion

        #region Message Presets

        internal void LoadMessagePresets(IQueryAdapter dbClient)
        {
            UserMessagePresets.Clear();
            RoomMessagePresets.Clear();

            dbClient.setQuery("SELECT type,message FROM moderation_presets WHERE enabled = 2");
            DataTable Data = dbClient.getTable();

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
            {
                String Message = (String)Row["message"];

                switch (Row["type"].ToString().ToLower())
                {
                    case "message":

                        UserMessagePresets.Add(Message);
                        break;

                    case "roommessage":

                        RoomMessagePresets.Add(Message);
                        break;
                }
            }
        }

        #endregion

        #region Support Tickets

        internal void LoadPendingTickets(IQueryAdapter dbClient)
        {
            dbClient.runFastQuery("TRUNCATE TABLE moderation_tickets");
            //dbClient.setQuery("SELECT moderation_tickets.*, p1.username AS sender_username, p2.username AS reported_username, p3.username AS moderator_username FROM moderation_tickets LEFT OUTER JOIN users AS p1 ON moderation_tickets.sender_id = p1.id LEFT OUTER JOIN users AS p2 ON moderation_tickets.reported_id = p2.id LEFT OUTER JOIN users AS p3 ON moderation_tickets.moderator_id = p3.id WHERE moderation_tickets.status != 'resolved'");
            //DataTable Data = dbClient.getTable();

            //if (Data == null)
            //{
            //    return;
            //}

            //foreach (DataRow Row in Data.Rows)
            //{
            //    SupportTicket Ticket = new SupportTicket(Convert.ToUInt32(Row["id"]), (int)Row["score"], (int)Row["type"], Convert.ToUInt32(Row["sender_id"]), Convert.ToUInt32(Row["reported_id"]), (String)Row["message"], Convert.ToUInt32(Row["room_id"]), (String)Row["room_name"], (Double)Row["timestamp"], Row["sender_username"], Row["reported_username"], Row["moderator_username"]);

            //    if (Row["status"].ToString().ToLower() == "picked")
            //    {
            //        Ticket.Pick(Convert.ToUInt32(Row["moderator_id"]), false);
            //    }

            //    Tickets.Add(Ticket);
            //}
        }

        internal void SendNewTicket(GameClient Session, int Category, uint ReportedUser, String Message)
        {
            if (Session.GetHabbo().CurrentRoomId <= 0)
            {
                return;
            }

            RoomData Data = FirewindEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(Session.GetHabbo().CurrentRoomId);

            uint TicketId = 0;

            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO moderation_tickets (score,type,status,sender_id,reported_id,moderator_id,message,room_id,room_name,timestamp) VALUES (1,'" + Category + "','open','" + Session.GetHabbo().Id + "','" + ReportedUser + "','0',@message,'" + Data.Id + "',@name,'" + FirewindEnvironment.GetUnixTimestamp() + "')");
                dbClient.addParameter("message", Message);
                dbClient.addParameter("name", Data.Name);
                TicketId = (uint)dbClient.insertQuery();

                dbClient.runFastQuery("UPDATE user_info SET cfhs = cfhs + 1 WHERE user_id = " + Session.GetHabbo().Id + "");

                //dbClient.setQuery("SELECT id FROM moderation_tickets WHERE sender_id = " + Session.GetHabbo().Id + " ORDER BY id DESC LIMIT 1");
                //TicketId = (uint)dbClient.getRow()[0];
            }

            SupportTicket Ticket = new SupportTicket(TicketId, 1, Category, Session.GetHabbo().Id, ReportedUser, Message, Data.Id, Data.Name, FirewindEnvironment.GetUnixTimestamp());

            Tickets.Add(Ticket);

            SendTicketToModerators(Ticket);
        }

        internal void SerializeOpenTickets(ref QueuedServerMessage serverMessages, uint userID)
        {
            foreach (SupportTicket ticket in Tickets)
            {
                if (ticket.Status == TicketStatus.OPEN || (ticket.Status == TicketStatus.PICKED && ticket.ModeratorId == userID) || (ticket.Status == TicketStatus.PICKED && ticket.ModeratorId == null))
                    serverMessages.appendResponse(ticket.Serialize());
            }
        }

        internal SupportTicket GetTicket(uint TicketId)
        {
            foreach (SupportTicket Ticket in Tickets)
            {
                if (Ticket.TicketId == TicketId)
                {
                    return Ticket;
                }
            }
            return null;
        }

        internal void PickTicket(GameClient Session, uint TicketId)
        {
            SupportTicket Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.OPEN)
            {
                return;
            }

            Ticket.Pick(Session.GetHabbo().Id, true);
            SendTicketToModerators(Ticket);
        }

        internal void ReleaseTicket(GameClient Session, uint TicketId)
        {
            SupportTicket Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.PICKED || Ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            Ticket.Release(true);
            SendTicketToModerators(Ticket);
        }

        internal void CloseTicket(GameClient Session, uint TicketId, int Result)
        {
            SupportTicket Ticket = GetTicket(TicketId);

            if (Ticket == null || Ticket.Status != TicketStatus.PICKED || Ticket.ModeratorId != Session.GetHabbo().Id)
            {
                return;
            }

            GameClient Client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(Ticket.SenderId);

            TicketStatus NewStatus;
            int ResultCode;

            switch (Result)
            {
                case 1:

                    ResultCode = 1;
                    NewStatus = TicketStatus.INVALID;
                    break;

                case 2:

                    ResultCode = 2;
                    NewStatus = TicketStatus.ABUSIVE;

                    using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                    {
                        dbClient.runFastQuery("UPDATE user_info SET cfhs_abusive = cfhs_abusive + 1 WHERE user_id = " + Ticket.SenderId + "");
                    }

                    break;

                case 3:
                default:

                    ResultCode = 0;
                    NewStatus = TicketStatus.RESOLVED;
                    break;
            }

            if (Client != null)
            {
                Client.GetMessageHandler().GetResponse().Init(3800);
                Client.GetMessageHandler().GetResponse().AppendInt32(ResultCode);
                Client.GetMessageHandler().SendResponse();
                /*Client.GetMessageHandler().GetResponse().Init(Outgoing.UpdateIssue);
                Client.GetMessageHandler().GetResponse().AppendInt32(1); // size
                Client.GetMessageHandler().GetResponse().AppendUInt(Ticket.TicketId);
                Client.GetMessageHandler().GetResponse().AppendUInt(Ticket.ModeratorId);
                Client.GetMessageHandler().GetResponse().AppendString((FirewindEnvironment.getHabboForId(Ticket.ModeratorId) != null) ? (FirewindEnvironment.getHabboForId(Ticket.ModeratorId).Username) : "Undefined");
                Client.GetMessageHandler().GetResponse().AppendBoolean(false); // retryEnabled
                Client.GetMessageHandler().GetResponse().AppendInt32(0); // retryEnabled
                Client.GetMessageHandler().SendResponse();*/
            }

            Ticket.Close(NewStatus, true);
            //SendTicketUpdateToModerators(Ticket);
            SendTicketToModerators(Ticket);
        }

        internal Boolean UsersHasPendingTicket(UInt32 Id)
        {
            foreach (SupportTicket Ticket in Tickets)
            {
                if (Ticket.SenderId == Id && Ticket.Status == TicketStatus.OPEN)
                {
                    return true;
                }
            }
            return false;
        }

        internal void DeletePendingTicketForUser(UInt32 Id)
        {
            foreach (SupportTicket Ticket in Tickets)
            {
                if (Ticket.SenderId == Id)
                {
                    Ticket.Delete(true);
                    SendTicketToModerators(Ticket);
                    return;
                }
            }
        }

        internal static void SendTicketUpdateToModerators(SupportTicket Ticket)
        {
            //FirewindEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Ticket.SerializeUpdate(), "fuse_mod");
        }

        internal static void SendTicketToModerators(SupportTicket Ticket)
        {
            FirewindEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(Ticket.Serialize(), "fuse_mod");
        }


        internal void LogStaffEntry(string modName, string target, string type, string description)
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("INSERT INTO staff_logs (staffuser,target,action_type,description) VALUES (@username,@target,@type,@desc)");
                dbClient.addParameter("username", modName);
                dbClient.addParameter("target", target);
                dbClient.addParameter("type", type);
                dbClient.addParameter("desc", description);
                dbClient.runQuery();
            }
        }
        #endregion

        #region Room Moderation

        internal static void PerformRoomAction(GameClient ModSession, uint RoomId, Boolean KickUsers, Boolean LockRoom, Boolean InappropriateRoom)
        {
            Room Room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);

            if (Room == null)
            {
                return;
            }

            if (LockRoom)
            {
                Room.State = 1;

                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE rooms SET state = 'locked' WHERE id = " + Room.RoomId);
                }
                Room.Name = "Inappropriate to Hotel Management";
            }

            if (InappropriateRoom)
            {
                Room.Name = LanguageLocale.GetValue("moderation.room.roomclosed");
                Room.Description = LanguageLocale.GetValue("moderation.room.roomclosed");
                Room.ClearTags();

                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE rooms SET caption = 'Inappropriate to Hotel Management', description = 'Inappropriate to Hotel Management', tags = '' WHERE id = " + Room.RoomId + "");
                }
            }

            if (KickUsers)
            {
                onCycleDoneDelegate kick = new onCycleDoneDelegate(Room.onRoomKick);
                Room.GetRoomUserManager().UserList.QueueDelegate(kick);
            }
        }


        internal static void RoomAlert(uint RoomId, Boolean Caution, String Message)
        {
            Room Room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);

            if (Room == null || Message.Length <= 1)
            {
                return;
            }

            RoomAlert alert = new RoomAlert(Message, 3);
        }

        internal static ServerMessage SerializeRoomTool(RoomData Data)
        {
            Room Room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(Data.Id);
            UInt32 OwnerId = 0;

            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                try
                {
                    dbClient.setQuery("SELECT id FROM users WHERE username = @owner");
                    dbClient.addParameter("owner", Data.Owner);
                    OwnerId = Convert.ToUInt32(dbClient.getRow()[0]);
                }
                catch (Exception e)
                {
                    Logging.HandleException(e, "ModerationTool.SerializeRoomTool");
                }
            }

            ServerMessage Message = new ServerMessage(Outgoing.RoomTool);
            Message.AppendUInt(Data.Id);
            Message.AppendInt32(Data.UsersNow); // user count

            if (Room != null)
            {
                Message.AppendBoolean((Room.GetRoomUserManager().GetRoomUserByHabbo(Data.Owner) != null));
            }
            else
            {
                Message.AppendBoolean(false);
            }

            Message.AppendUInt(OwnerId);
            Message.AppendString(Data.Owner);
            Message.AppendBoolean((Room != null)); // show data?
            Message.AppendString(Data.Name);
            Message.AppendString(Data.Description);
            Message.AppendInt32(Data.TagCount);

            foreach (string Tag in Data.Tags)
            {
                Message.AppendString(Tag);
            }

            Message.AppendBoolean((Room != null) ? Room.HasOngoingEvent : false);
            if (Room != null)
            {
                if (Room.Event != null)
                {
                    Message.AppendString(Room.Event.Name);
                    Message.AppendString(Room.Event.Description);
                    Message.AppendInt32(Room.Event.Tags.Count);

                    foreach (string Tag in Room.Event.Tags.ToArray())
                    {
                        Message.AppendString(Tag);
                    }
                }
            }

            return Message;
        }

        #endregion

        #region User Moderation

        internal static void KickUser(GameClient ModSession, uint UserId, String Message, Boolean Soft)
        {
            GameClient Client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client == null || Client.GetHabbo().CurrentRoomId < 1 || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.kick.missingrank"));
                return;
            }

            Room Room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(Client.GetHabbo().CurrentRoomId);

            if (Room == null)
            {
                return;
            }

            Room.GetRoomUserManager().RemoveUserFromRoom(Client, true, false);
            Client.CurrentRoomUserID = -1;

            if (!Soft)
            {
                Client.SendNotif(Message);

                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_info SET cautions = cautions + 1 WHERE user_id = " + UserId + "");
                }
            }
        }

        internal static void AlertUser(GameClient ModSession, uint UserId, String Message, Boolean Caution)
        {
            GameClient Client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client == null || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Caution && Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.caution.missingrank"));
                Caution = false;
            }

            Client.SendNotif(Message, Caution);

            if (Caution)
            {
                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.runFastQuery("UPDATE user_info SET cautions = cautions + 1 WHERE user_id = " + UserId + "");
                }
            }
        }

        internal static void BanUser(GameClient ModSession, uint UserId, int Length, String Message)
        {
            GameClient Client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client == null || Client.GetHabbo().Id == ModSession.GetHabbo().Id)
            {
                return;
            }

            if (Client.GetHabbo().Rank >= ModSession.GetHabbo().Rank)
            {
                ModSession.SendNotif(LanguageLocale.GetValue("moderation.ban.missingrank"));
                return;
            }

            Double dLength = Length;

            FirewindEnvironment.GetGame().GetBanManager().BanUser(Client, ModSession.GetHabbo().Username, dLength, Message, false);
        }

        #endregion

        #region User Info

        internal static ServerMessage SerializeUserInfo(uint UserId)
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id, username, online, mail FROM users WHERE id = " + UserId + "");
                DataRow User = dbClient.getRow();

                dbClient.setQuery("SELECT reg_timestamp, login_timestamp, cfhs, cfhs_abusive, cautions, bans FROM user_info WHERE user_id = " + UserId + "");
                DataRow Info = dbClient.getRow();

                if (User == null)
                {
                    throw new NullReferenceException("No user found in database");
                }

                ServerMessage Message = new ServerMessage(Outgoing.UserTool);

                Message.AppendUInt(Convert.ToUInt32(User["id"]));
                Message.AppendString((string)User["username"]);

                if (Info != null)
                {
                    Message.AppendInt32((int)Math.Ceiling((FirewindEnvironment.GetUnixTimestamp() - (Double)Info["reg_timestamp"]) / 60));
                    Message.AppendInt32((int)Math.Ceiling((FirewindEnvironment.GetUnixTimestamp() - (Double)Info["login_timestamp"]) / 60));
                }
                else
                {
                    Message.AppendInt32(0);
                    Message.AppendInt32(0);
                }

                Message.AppendBoolean(FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToUInt32(User["id"])) != null);

                if (Info != null)
                {
                    Message.AppendInt32((int)Info["cfhs"]);
                    Message.AppendInt32((int)Info["cfhs_abusive"]);
                    Message.AppendInt32((int)Info["cautions"]);
                    Message.AppendInt32((int)Info["bans"]);
                }
                else
                {
                    Message.AppendInt32(0); // cfhs
                    Message.AppendInt32(0); // abusive cfhs
                    Message.AppendInt32(0); // cautions
                    Message.AppendInt32(0); // bans
                }
                Message.AppendString("hallo"); // last_purchase_txt
                Message.AppendInt32(0); // identityinformationtool.url + this
                Message.AppendInt32(0); // id_bans_txt
                Message.AppendString((string)User["mail"]); // email_address_txt
                return Message;
            }
        }

        internal static ServerMessage SerializeRoomVisits(UInt32 UserId)
        {
            //using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                //dbClient.setQuery("SELECT room_id,hour,minute FROM user_roomvisits WHERE user_id = " + UserId + " ORDER BY entry_timestamp DESC LIMIT 50");
                //DataTable Data = dbClient.getTable();

                ServerMessage Message = new ServerMessage(Outgoing.RoomVisits);
                Message.AppendUInt(UserId);
                Message.AppendString(FirewindEnvironment.GetGame().GetClientManager().GetNameById(UserId));

                //if (Data != null)
                //{
                //    Message.AppendInt32(Data.Rows.Count);

                //    foreach (DataRow Row in Data.Rows)
                //    {
                //        RoomData RoomData = FirewindEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData(Convert.ToUInt32(Row["room_id"]));

                //        Message.AppendBoolean(RoomData.IsPublicRoom);
                //        Message.AppendUInt(RoomData.Id);
                //        Message.AppendString(RoomData.Name);
                //        Message.AppendInt32((int)Row["hour"]);
                //        Message.AppendInt32((int)Row["minute"]);
                //    }
                //}
                //else
                //{
                    Message.AppendInt32(0);
                //}

                return Message;
            }
        }

        #endregion

        #region Chatlogs

        internal static ServerMessage SerializeUserChatlog(UInt32 UserId)
        {
            GameClient client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (client == null || client.GetHabbo() == null)
            {
                ServerMessage Message = new ServerMessage(Outgoing.UserChatlog);
                Message.AppendUInt(UserId);
                Message.AppendString("User not online");
                Message.AppendInt32(0);

                return Message;
            }
            else
            {
                ChatMessageManager manager = client.GetHabbo().GetChatMessageManager();
                Dictionary<int, List<ChatMessage>> messages = manager.GetSortedMessages();

                ServerMessage Message = new ServerMessage(Outgoing.UserChatlog);
                Message.AppendUInt(UserId);
                Message.AppendString(client.GetHabbo().Username);
                Message.AppendInt32(messages.Count);
                foreach (KeyValuePair<int, List<ChatMessage>> valuePair in messages)
                {
                 
                    List<ChatMessage> sortedMessages = valuePair.Value;
                    ChatMessage firstMessage = sortedMessages.First();

                    Message.AppendBoolean(false); // is public
                    Message.AppendUInt(firstMessage.roomID);
                    Message.AppendString(firstMessage.roomName);

                    Message.AppendInt32(sortedMessages.Count);

                    foreach (ChatMessage message in sortedMessages)
                    {
                        message.Serialize(ref Message);
                    }
                }

                return Message;
            }

            //using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            //{
            //    dbClient.setQuery("SELECT room_id,entry_timestamp,exit_timestamp FROM user_roomvisits WHERE user_id = " + UserId + " ORDER BY entry_timestamp DESC LIMIT 5");
            //    DataTable Visits = dbClient.getTable();


            //    ServerMessage Message = new ServerMessage(536);
            //    Message.AppendUInt(UserId);
            //    Message.AppendString(FirewindEnvironment.GetGame().GetClientManager().GetNameById(UserId));

            //    if (Visits != null)
            //    {
            //        Message.AppendInt32(Visits.Rows.Count);

            //        foreach (DataRow Visit in Visits.Rows)
            //        {
            //            DataTable Chatlogs = null;

            //            if ((Double)Visit["exit_timestamp"] <= 0.0)
            //            {
            //                Visit["exit_timestamp"] = FirewindEnvironment.GetUnixTimestamp();
            //            }

            //            //using (DatabaseClient dbClient = FirewindEnvironment.GetDatabase().GetClient())
            //            //{
            //            //Chatlogs = dbClient.getTable("SELECT user_id,user_name,hour,minute,message FROM chatlogs WHERE room_id = " + (uint)Visit["room_id"] + " AND timestamp > " + (Double)Visit["entry_timestamp"] + " AND timestamp < " + (Double)Visit["exit_timestamp"] + " ORDER BY timestamp DESC");
            //            //}

            //            RoomData RoomData = FirewindEnvironment.GetGame().GetRoomManager().GenerateNullableRoomData((UInt32)Visit["room_id"]);

            //            Message.AppendBoolean(RoomData.IsPublicRoom);
            //            Message.AppendUInt(RoomData.Id);
            //            Message.AppendString(RoomData.Name);

            //            if (Chatlogs != null)
            //            {
            //                Message.AppendInt32(Chatlogs.Rows.Count);

            //                foreach (DataRow Log in Chatlogs.Rows)
            //                {
            //                    Message.AppendInt32((int)Log["hour"]);
            //                    Message.AppendInt32((int)Log["minute"]);
            //                    Message.AppendUInt((UInt32)Log["user_id"]);
            //                    Message.AppendString((string)Log["user_name"]);
            //                    Message.AppendString((string)Log["message"]);
            //                }
            //            }
            //            else
            //            {
            //                Message.AppendInt32(0);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Message.AppendInt32(0);
            //    }

            //    return Message;
           // }
        }

        internal static ServerMessage SerializeTicketChatlog(SupportTicket Ticket, RoomData RoomData, Double Timestamp)
        {
            Room currentRoom = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomData.Id);

            ServerMessage Message = new ServerMessage(Outgoing.IssueChatlog);
            Message.AppendUInt(Ticket.TicketId);
            Message.AppendUInt(Ticket.SenderId);
            Message.AppendUInt(Ticket.ReportedId);
            Message.AppendUInt(RoomData.Id); //maybe?
            Message.AppendBoolean(false); // is public
            Message.AppendUInt(RoomData.Id);
            Message.AppendString(RoomData.Name);

            if (currentRoom == null)
            {
                Message.AppendInt32(0);
                return Message;
            }
            else
            {
                ChatMessageManager manager = currentRoom.GetChatMessageManager();
                Message.AppendInt32(manager.messageCount);
                manager.Serialize(ref Message);

                return Message;
            }


            //using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            //{
            //    dbClient.setQuery("SELECT user_id,user_name,hour,minute,message FROM chatlogs WHERE room_id = " + RoomData.Id + " AND timestamp >= " + (Timestamp - 300) + " AND timestamp <= " + Timestamp + " ORDER BY timestamp DESC");
            //    DataTable Data = dbClient.getTable();

            //    ServerMessage Message = new ServerMessage(534);
            //    Message.AppendUInt(Ticket.TicketId);
            //    Message.AppendUInt(Ticket.SenderId);
            //    Message.AppendUInt(Ticket.ReportedId);
            //    Message.AppendBoolean(RoomData.IsPublicRoom);
            //    Message.AppendUInt(RoomData.Id);
            //    Message.AppendString(RoomData.Name);

            //    if (Data != null)
            //    {
            //        Message.AppendInt32(Data.Rows.Count);

            //        foreach (DataRow Row in Data.Rows)
            //        {
            //            Message.AppendInt32((int)Row["hour"]);
            //            Message.AppendInt32((int)Row["minute"]);
            //            Message.AppendUInt((UInt32)Row["user_id"]);
            //            Message.AppendString((String)Row["user_name"]);
            //            Message.AppendString((String)Row["message"]);
            //        }
            //    }
            //    else
            //    {
            //        Message.AppendInt32(0);
            //    }

            //    return Message;
            //}
        }

        internal static ServerMessage SerializeRoomChatlog(UInt32 roomID)
        {
            Room currentRoom = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(roomID);

            ServerMessage Message = new ServerMessage(Outgoing.RoomChatlog);
            Message.AppendBoolean(false); // is public
            Message.AppendUInt(currentRoom.RoomId);
            Message.AppendString(currentRoom.Name);

            if (currentRoom == null)
            {
                Message.AppendInt32(0);
                return Message;
            }
            else
            {
                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT user_id,user_name,hour,minute,message FROM chatlogs WHERE room_id = " + currentRoom.RoomId + " ORDER BY timestamp DESC LIMIT 150");
                    DataTable Data = dbClient.getTable();

                    if (Data != null)
                    {
                        Message.AppendInt32(Data.Rows.Count);

                        foreach (DataRow Row in Data.Rows)
                        {
                            Message.AppendInt32((int)Row["hour"]);
                            Message.AppendInt32((int)Row["minute"]);
                            Message.AppendUInt((UInt32)Row["user_id"]);
                            Message.AppendString((string)Row["user_name"]);
                            Message.AppendString((string)Row["message"]);
                        }
                    }
                    else
                    {
                        Message.AppendInt32(0);
                    }

                    return Message;
                }
            }

            //Room Room = FirewindEnvironment.GetGame().GetRoomManager().GetRoom(RoomId);

            //if (Room == null)
            //{
            //    throw new NullReferenceException("Noo room found.");
            //}

            //Boolean IsPublic = false;

            //if (Room.Type.ToLower() == "public")
            //{
            //    IsPublic = true;
            //}

            //using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            //{
            //    dbClient.setQuery("SELECT user_id,user_name,hour,minute,message FROM chatlogs WHERE room_id = " + Room.RoomId + " ORDER BY timestamp DESC LIMIT 150");
            //    DataTable Data = dbClient.getTable();

            //    ServerMessage Message = new ServerMessage(535);
            //    Message.AppendBoolean(IsPublic);
            //    Message.AppendUInt(Room.RoomId);
            //    Message.AppendString(Room.Name);

            //    if (Data != null)
            //    {
            //        Message.AppendInt32(Data.Rows.Count);

            //        foreach (DataRow Row in Data.Rows)
            //        {
            //            Message.AppendInt32((int)Row["hour"]);
            //            Message.AppendInt32((int)Row["minute"]);
            //            Message.AppendUInt((UInt32)Row["user_id"]);
            //            Message.AppendString((string)Row["user_name"]);
            //            Message.AppendString((string)Row["message"]);
            //        }
            //    }
            //    else
            //    {
            //        Message.AppendInt32(0);
            //    }

            //    return Message;
            //}
        }
        #endregion
    }
}
