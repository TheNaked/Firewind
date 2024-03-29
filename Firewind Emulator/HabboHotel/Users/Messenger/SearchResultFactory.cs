﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Database_Manager.Database.Session_Details.Interfaces;
using Firewind;
using System.Data;

namespace Firewind.HabboHotel.Users.Messenger
{
    class SearchResultFactory
    {
        internal static List<SearchResult> GetSearchResult(string query)
        {
            List<SearchResult> results = new List<SearchResult>();

            DataTable dTable;
            using (IQueryAdapter dbClient = FirewindEnvironment.GetDatabaseManager().getQueryreactor())
            {
                dbClient.setQuery("SELECT id,username,motto,look,last_online FROM users WHERE username LIKE @query LIMIT 50");
                dbClient.addParameter("query", query + "%");
                dTable = dbClient.getTable();
            }

            uint userID;
            string username;
            string motto;
            string look;
            string last_online;
            foreach (DataRow dRow in dTable.Rows)
            {
                userID = Convert.ToUInt32(dRow[0]);
                username = (string)dRow[1];
                motto = (string)dRow[2];
                look = (string)dRow[3];
                last_online = (string)dRow[4];

                SearchResult result = new SearchResult(userID, username, motto, look, last_online);
                results.Add(result);
            }

            return results;
        }
    }
}
