﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Firewind.Collections;
using Firewind.HabboHotel.GameClients;
using Firewind.HabboHotel.Rooms;
using Firewind.Messages;
using Firewind.HabboHotel.Users.Messenger;
using Database_Manager.Database.Session_Details.Interfaces;
using HabboEvents;

namespace Firewind.HabboHotel.Users.Messenger
{
    class HabboMessenger
    {
        private uint UserId;
        public Dictionary<uint, MessengerRequest> requests;
        private Dictionary<uint, MessengerBuddy> friends;

        internal bool AppearOffline;

        internal int myFriends
        {
            get
            {
                return friends.Count;
            }
        }

        internal HabboMessenger(uint UserId)
        {
            this.requests = new Dictionary<uint, MessengerRequest>();
            this.friends = new Dictionary<uint, MessengerBuddy>();
            this.UserId = UserId;
        }

        internal void Init(Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests)
        {
            this.requests = new Dictionary<uint, MessengerRequest>(requests);
            this.friends = new Dictionary<uint, MessengerBuddy>(friends);
        }

        internal void ClearRequests()
        {
            requests.Clear();
        }

        internal MessengerRequest GetRequest(uint senderID)
        {
            if (requests.ContainsKey(senderID))
                return requests[senderID];

            return null;
        }

        internal void Destroy()
        {
            IEnumerable<GameClient> onlineUsers = FirewindEnvironment.GetGame().GetClientManager().GetClientsById(friends.Keys);

            foreach (GameClient client in onlineUsers)
            {
                if (client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                    continue;

                client.GetHabbo().GetMessenger().UpdateFriend(this.UserId, null, true);
            }
        }

        internal void OnStatusChanged(bool notification)
        {
            IEnumerable<GameClient> onlineUsers = FirewindEnvironment.GetGame().GetClientManager().GetClientsById(friends.Keys);

            foreach (GameClient client in onlineUsers)
            {
                if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                    continue;

                client.GetHabbo().GetMessenger().UpdateFriend(this.UserId, client, true);
                UpdateFriend(client.GetHabbo().Id, client, notification);
            }
        }

        internal void UpdateFriend(uint userid, GameClient client, bool notification)
        {
            if (friends.ContainsKey(userid))
            {
                friends[userid].UpdateUser(client);

                if (notification)
                {
                    GameClient Userclient = GetClient();
                    if (Userclient != null)
                        Userclient.SendMessage(SerializeUpdate((MessengerBuddy)friends[userid]));
                }
            }
        }

        internal void HandleAllRequests()
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_requests WHERE sender = " + UserId + " OR receiver = " + UserId);
            }

            ClearRequests();
        }

        internal void HandleRequest(uint sender)
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_requests WHERE (sender = " + UserId + " AND receiver = " + sender + ") OR (receiver = " + UserId + " AND sender = " + sender + ")");
            }

            requests.Remove(sender);
        }

        internal void CreateFriendship(uint friendID)
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO messenger_friendships (sender,receiver) VALUES (" + UserId + "," + friendID + ")");
            }

            OnNewFriendship(friendID);

            GameClient User = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User != null && User.GetHabbo().GetMessenger() != null)
                User.GetHabbo().GetMessenger().OnNewFriendship(UserId);
        }

        internal void DestroyFriendship(uint friendID)
        {
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("DELETE FROM messenger_friendships WHERE (sender = " + UserId + " AND receiver = " + friendID + ") OR (receiver = " + UserId + " AND sender = " + friendID + ")");
            }

            OnDestroyFriendship(friendID);

            GameClient User = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User != null && User.GetHabbo().GetMessenger() != null)
                User.GetHabbo().GetMessenger().OnDestroyFriendship(UserId);
        }

        internal void OnNewFriendship(uint friendID)
        {
            GameClient friend = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            MessengerBuddy newFriend;
            if (friend == null || friend.GetHabbo() == null)
            {
                DataRow dRow;
                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT username,motto,look,last_online FROM users WHERE id = " + friendID);
                    dRow = dbClient.getRow();
                }

                newFriend = new MessengerBuddy(friendID, (string)dRow["username"], (string)dRow["look"], (string)dRow["motto"], (string)dRow["last_online"]);
            }
            else
            {
                Habbo user = friend.GetHabbo();
                newFriend = new MessengerBuddy(friendID, user.Username, user.Look, user.Motto, string.Empty);
                newFriend.UpdateUser(friend);
            }

            if (!friends.ContainsKey(friendID))
                friends.Add(friendID, newFriend);

            GetClient().SendMessage(SerializeUpdate(newFriend));
        }

        internal bool RequestExists(uint requestID)
        {
            if (requests.ContainsKey(requestID))
                return true;

            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT sender FROM messenger_friendships WHERE sender = @myID AND receiver = @friendID");
                dbClient.addParameter("myID", (int)this.UserId);
                dbClient.addParameter("friendID", (int)requestID);
                return dbClient.findsResult();
            }

        }

        internal bool FriendshipExists(uint friendID)
        {
            return friends.ContainsKey(friendID);
        }

        internal void OnDestroyFriendship(uint Friend)
        {
            friends.Remove(Friend);

            GetClient().GetMessageHandler().GetResponse().Init(Outgoing.FriendUpdate);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(0);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(1);
            GetClient().GetMessageHandler().GetResponse().AppendInt32(-1);
            GetClient().GetMessageHandler().GetResponse().AppendUInt(Friend);
            GetClient().GetMessageHandler().SendResponse();
        }

        internal bool RequestBuddy(string UserQuery)
        {
            uint userID;
            bool hasFQDisabled;

            GameClient client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUsername(UserQuery);

            if (client == null)
            {
                DataRow Row = null;
                using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
                {
                    dbClient.setQuery("SELECT id,block_newfriends FROM users WHERE username = @query");
                    dbClient.addParameter("query", UserQuery.ToLower());
                    Row = dbClient.getRow();
                }

                if (Row == null)
                    return false;

                userID = Convert.ToUInt32(Row["id"]);
                hasFQDisabled = FirewindEnvironment.EnumToBool(Row["block_newfriends"].ToString());
            }
            else
            {
                userID = client.GetHabbo().Id;
                hasFQDisabled = client.GetHabbo().HasFriendRequestsDisabled;
            }

            if (hasFQDisabled)
            {
                GetClient().GetMessageHandler().GetResponse().Init(Outgoing.MessengerError);
                GetClient().GetMessageHandler().GetResponse().AppendInt32(0); // clientMessageId (not that important really)
                GetClient().GetMessageHandler().GetResponse().AppendInt32(3); // errorCode (1=limit reached,2=limit reached for other person,3=requests disabled,4=requestnotfound)
                GetClient().GetMessageHandler().SendResponse();
                return true;
            }

            uint ToId = userID;

            if (RequestExists(ToId))
            {
                return true;
            }

            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.runFastQuery("REPLACE INTO messenger_requests (sender,receiver) VALUES (" + this.UserId + "," + ToId + ")");
            }

            GameClient ToUser = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);

            if (ToUser == null || ToUser.GetHabbo() == null)
            {
                return true;
            }

            MessengerRequest Request = new MessengerRequest(ToId, UserId, FirewindEnvironment.GetGame().GetClientManager().GetNameById(UserId));

            ToUser.GetHabbo().GetMessenger().OnNewRequest(UserId);

            ServerMessage NewFriendNotif = new ServerMessage(Outgoing.SendFriendRequest);
            Request.Serialize(NewFriendNotif);
            ToUser.SendMessage(NewFriendNotif);
            requests.Add(ToId, Request);
            return true;
        }

        internal void OnNewRequest(uint friendID)
        {
            if (!requests.ContainsKey(friendID))
                requests.Add(friendID, new MessengerRequest(UserId, friendID, FirewindEnvironment.GetGame().GetClientManager().GetNameById(friendID)));
        }

        internal void SendInstantMessage(UInt32 ToId, string Message)
        {
            if (!FriendshipExists(ToId))
            {
                DeliverInstantMessageError(6, ToId);
                return;
            }

            GameClient Client = FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);

            if (Client == null || Client.GetHabbo().GetMessenger() == null)
            {
                DeliverInstantMessageError(5, ToId);
                return;
            }

            if (GetClient().GetHabbo().Muted)
            {
                DeliverInstantMessageError(4, ToId);
                return;
            }

            if (Client.GetHabbo().Muted)
            {
                DeliverInstantMessageError(3, ToId); // No return, as this is just a warning.
            }

            if (Message == "")
                return;

            Client.GetHabbo().GetMessenger().DeliverInstantMessage(Message, UserId);
        }

        internal void DeliverInstantMessage(string message, uint convoID)
        {
            ServerMessage InstantMessage = new ServerMessage(Outgoing.InstantChat);
            InstantMessage.AppendUInt(convoID);
            InstantMessage.AppendString(message);
            InstantMessage.AppendString(FirewindEnvironment.GetUnixTimestamp() + string.Empty);
            GetClient().SendMessage(InstantMessage);
        }

        internal void DeliverInstantMessageError(int ErrorId, UInt32 ConversationId)
        {
            /*
3                =     Your friend is muted and cannot reply.
4                =     Your message was not sent because you are muted.
5                =     Your friend is not online.
6                =     Receiver is not your friend anymore.
7                =     Your friend is busy.
8                =     Your friend is wanking*/

            ServerMessage reply = new ServerMessage(Outgoing.InstantChatError);
            reply.AppendInt32(ErrorId);
            reply.AppendUInt(ConversationId);
            GetClient().SendMessage(reply);
        }

        internal ServerMessage SerializeFriends()
        {
            ServerMessage reply = new ServerMessage(Outgoing.InitFriends);
            reply.AppendInt32((GetClient().GetHabbo().GetSubscriptionManager().HasSubscription("habbo_vip") ? 1100 : 300));
            reply.AppendInt32(300);
            reply.AppendInt32(800);
            reply.AppendInt32(1100);
            reply.AppendInt32(0); // categorys
            reply.AppendInt32(friends.Count);

            foreach (MessengerBuddy friend in friends.Values)
            {
                
                friend.Serialize(reply);
            }

            return reply;
        }

        internal static ServerMessage SerializeUpdate(MessengerBuddy friend)
        {
            ServerMessage reply = new ServerMessage(Outgoing.FriendUpdate);
            reply.AppendInt32(0); // category
            reply.AppendInt32(1); // number of updates
            reply.AppendInt32(0); // don't know

            friend.Serialize(reply);
            reply.AppendBoolean(false);

            return reply;
        }

        internal ServerMessage SerializeRequests()
        {
            ServerMessage reply = new ServerMessage(Outgoing.InitRequests);

            reply.AppendInt32((requests.Count > FirewindEnvironment.friendRequestLimit) ? (int)FirewindEnvironment.friendRequestLimit : requests.Count);
            reply.AppendInt32((requests.Count > FirewindEnvironment.friendRequestLimit) ? (int)FirewindEnvironment.friendRequestLimit : requests.Count);

            int i = 0;
            foreach (MessengerRequest request in requests.Values)
            {
                i++;
                if (i > FirewindEnvironment.friendRequestLimit)
                    break;
                request.Serialize(reply);
            }

            return reply;
        }

        internal ServerMessage PerformSearch(string query)
        {
            List<SearchResult> results = SearchResultFactory.GetSearchResult(query);

            List<SearchResult> existingFriends = new List<SearchResult>();
            List<SearchResult> othersUsers = new List<SearchResult>();

            foreach (SearchResult result in results)
            {
                if (result.userID == GetClient().GetHabbo().Id)
                    continue;
                if (FriendshipExists(result.userID))
                    existingFriends.Add(result);
                else
                    othersUsers.Add(result);
            }

            ServerMessage reply = new ServerMessage(Outgoing.SearchFriend);
            reply.AppendInt32(existingFriends.Count);
            foreach (SearchResult result in existingFriends)
            {
                result.Searialize(reply);
            }

            reply.AppendInt32(othersUsers.Count);
            foreach (SearchResult result in othersUsers)
            {
                result.Searialize(reply);
            }

            return reply;
        }

        private GameClient GetClient()
        {
            return FirewindEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
        }

        internal IEnumerable<RoomData> GetActiveFriendsRooms()
        {
            foreach (MessengerBuddy buddy in friends.Values.Where(p => p.InRoom))
                yield return buddy.CurrentRoom.RoomData;
        }
    }
}