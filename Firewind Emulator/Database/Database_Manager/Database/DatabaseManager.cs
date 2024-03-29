﻿namespace Database_Manager.Database
{
    using Database_Manager.Database.Database_Exceptions;
    using Database_Manager.Database.Session_Details.Interfaces;
    using Database_Manager.Managers.Database;
    using MySql.Data.MySqlClient;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Text;
    using System.Collections;
    using System.Data.SqlClient;

    public class DatabaseManager
    {
        private int beginClientAmount;
        private string connectionString;
        private bool isConnected = false;
        private uint maxPoolSize;
        private DatabaseServer server;
        private Queue connections;

        public static bool dbEnabled = true;

        public DatabaseManager(uint maxPoolSize, int clientAmount)
        {
            if (maxPoolSize < clientAmount)
                throw new DatabaseException("The poolsize can not be larger than the client amount!");

            this.beginClientAmount = clientAmount;
            this.maxPoolSize = maxPoolSize;
            this.connections = new Queue();
        }

        private void createNewConnectionString()
        {
                MySqlConnectionStringBuilder connectionString = new MySqlConnectionStringBuilder
                {
                    Server = this.server.getHost(),
                    Port = this.server.getPort(),
                    UserID = this.server.getUsername(),
                    Password = this.server.getPassword(),
                    Database = this.server.getDatabaseName(),
                    MinimumPoolSize = this.maxPoolSize / 2,
                    MaximumPoolSize = this.maxPoolSize,
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true,
                    DefaultCommandTimeout = 300,
                    ConnectionTimeout = 10
                };

                this.setConnectionString(connectionString.ToString());
        }

        public void destroy()
        {
            lock (this)
            {
                this.isConnected = false;
            }
        }

        internal string getConnectionString()
        {
            return this.connectionString;
        }

        public IQueryAdapter getQueryreactor()
        {
            IDatabaseClient dbClient = null;
            lock (connections.SyncRoot)
            {
                if (connections.Count > 0)
                {
                    dbClient = (IDatabaseClient)connections.Dequeue();
                }
            }

            if (dbClient != null)
            {
                dbClient.connect();
                dbClient.prepare();
                return dbClient.getQueryReactor();
            }
            else
            {
                IDatabaseClient connection = new MySqlClient(this, 0);
                connection.connect();
                connection.prepare();
                return connection.getQueryReactor();
            }
        }

        internal void FreeConnection(IDatabaseClient dbClient)
        {
            lock (connections.SyncRoot)
            {
                connections.Enqueue(dbClient);
            }
        }

        public void init()
        {
            try
            {
                this.createNewConnectionString();
            }
            catch (MySqlException exception)
            {
                this.isConnected = false;
                throw new Exception("Could not connect the clients to the database: " + exception.Message);
            }
            this.isConnected = true;
        }

        public bool isConnectedToDatabase()
        {
            return this.isConnected;
        }

        private void setConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool setServerDetails(string host, uint port, string username, string password, string databaseName)
        {
            try
            {
                this.server = new DatabaseServer(host, port, username, password, databaseName);
                return true;
            }
            catch (DatabaseException)
            {
                this.isConnected = false;
                return false;
            }
        }
    }
}

