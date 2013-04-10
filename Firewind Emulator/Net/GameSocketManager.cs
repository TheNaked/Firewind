﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ConnectionManager.Socket_Exceptions;
using Helpers;
using SharedPacketLib;
using System.Collections;

namespace ConnectionManager
{
    public class SocketManager
    {
        #region declares
        /// <summary>
        /// Contains active connections and there id's.
        /// </summary>
        //private Dictionary<int, ConnectionInformation> activeConnections;
        private Hashtable activeConnections;
        
        /// <summary>
        /// Contains the ip's and their connection counts
        /// </summary>
        //private Dictionary<string, int> ipConnectionCount;
        private Hashtable ipConnectionCount;

        /// <summary>
        /// Contains the max conenctions per ip count
        /// </summary>
        private int maxIpConnectionCount;

        /// <summary>
        /// Indicates the amount of accepted connections.
        /// </summary>
        private int acceptedConnections;

        /// <summary>
        /// The Socket used for incoming data requests.
        /// </summary>
        private Socket connectionListener;

        /// <summary>
        /// The port information, contains the nummeric value the socket should listen on.
        /// </summary>
        private int portInformation;

        /// <summary>
        /// The maximum amount of connections the server should be allowed to have
        /// </summary>
        private int maximumConnections;

        /// <summary>
        /// Indicates if connections should be accepted or not
        /// </summary>
        private bool acceptConnections;

        /// <summary>
        /// This method is called if an connection event occurs
        /// </summary>
        /// <param name="connection">The new Game connection which was generated by the code</param>
        public delegate void ConnectionEvent(ConnectionInformation connection);

        /// <summary>
        /// Occurs when a new connection was established
        /// </summary>
        public event ConnectionEvent connectionEvent;
        private IDataParser parser;
        private bool disableNagleAlgorithm;

        #endregion

        #region initializer
        /// <summary>
        /// Initializes the connection instance
        /// </summary>
        /// <param name="portID">The ID of the port this item should listen on</param>
        /// <param name="maxConnections">The maximum amount of connections</param>
        public void init(int portID, int maxConnections, int connectionsPerIP, IDataParser parser, bool disableNaglesAlgorithm)
        {
            //Out.writeLine("Starting up socket manager on port [" + portID + "] with [" + maxConnections + "] as the maximum connection count", Out.logFlags.ImportantLogLevel);
            //Out.writeLine("Maximum connections per ip [" + connectionsPerIP + "]", Out.logFlags.ImportantLogLevel);
            this.parser = parser;
            this.disableNagleAlgorithm = disableNaglesAlgorithm;
            initializeFields();
            maximumConnections = maxConnections;
            portInformation = portID;
            maxIpConnectionCount = connectionsPerIP;
            acceptedConnections = 0;
            prepareConnectionDetails();
            //Out.writeLine("Connection details set up!", Out.logFlags.ImportantLogLevel);
            
        }

        /// <summary>
        /// Initializes the fields
        /// </summary>
        private void initializeFields()
        {
            activeConnections = new Hashtable();
            ipConnectionCount = new Hashtable();
            //activeConnections = new Dictionary<int, ConnectionInformation>(maximumConnections / 2);
            //ipConnectionCount = new Dictionary<string, int>(maximumConnections / 2);
        }


        /// <summary>
        /// Prepares the socket for connections
        /// </summary>
        private void prepareConnectionDetails()
        {
            connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connectionListener.NoDelay = disableNagleAlgorithm;
            try
            {
                connectionListener.Bind(new IPEndPoint(IPAddress.Any, portInformation));
            }
            catch (SocketException ex)
            {
                throw new SocketInitializationException(ex.Message);
            }
        }

        /// <summary>
        /// Initializes the incoming data requests
        /// </summary>
        public void initializeConnectionRequests()
        {
            //Out.writeLine("Starting to listen to connection requests", Out.logFlags.ImportantLogLevel);
            connectionListener.Listen(100);
            acceptConnections = true;
            string currentHost = System.Net.Dns.GetHostName();
            // Get the IP from the host name
            IPHostEntry host = System.Net.Dns.GetHostEntry(currentHost);
            //Out.writeNotification("Started listening on port [" + portInformation + "] on host [" + currentHost + "]");
            //Out.writeNotification("Listening on the following ip addresses:");
            //for (int i = 0; i < host.AddressList.Length; i++)
            //{
            //    Out.writeNotification(host.AddressList[i].ToString());
            //}
            try
            {
                connectionListener.BeginAccept(new AsyncCallback(newConnectionRequest), connectionListener);
            }
            catch { destroy(); }
            //Out.writeLine("Started listening for connection requests on port [" + portInformation + "]", Out.logFlags.ImportantLogLevel);
            Out.writeLine("Socket -> READY!", Out.logFlags.ImportantLogLevel);
        }
        #endregion

        #region destructor
        /// <summary>
        /// Destroys the current connection manager and disconnects all users
        /// </summary>
        public void destroy()
        {
            this.acceptConnections = false;
            try
            {
                this.connectionListener.Close();
            }
            catch { }
            this.connectionListener = null;
            //if (activeConnections != null)
            //{
            //    List<ConnectionInformation> connections = new List<ConnectionInformation>(activeConnections.Count);
            //    foreach (ConnectionInformation item in activeConnections.Values)
            //    {
            //        connections.Add(item);
            //    }
            //    foreach (ConnectionInformation item in connections)
            //    {
            //        item.disconnect();
            //    }
            //    activeConnections = null;
            //}

        }
        #endregion

        #region connection request

        /// <summary>
        /// Handels a new incoming data request from some computer from arround the world
        /// </summary>
        /// <param name="iAr">the IAsyncResult of the connection</param>
        private void newConnectionRequest(IAsyncResult iAr)
        {
            if (this.connectionListener != null)
            {
                if (acceptConnections)
                {
                    try
                    {
                        Socket replyFromComputer = ((Socket)iAr.AsyncState).EndAccept(iAr);
                        replyFromComputer.NoDelay = this.disableNagleAlgorithm;
                        Out.writeLine("New connection from [" + replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] + "].", Out.logFlags.StandardLogLevel);

                        if (activeConnections.Count < maximumConnections)
                        {

                            string Ip = replyFromComputer.RemoteEndPoint.ToString().Split(':')[0];
                            //if (getAmountOfConnectionFromIp(Ip) < maxIpConnectionCount)
                            {
                                acceptedConnections++;
                                Out.writeLine("Accepted connection [" + acceptedConnections + "] from  [" + replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] + "].", Out.logFlags.BelowStandardlogLevel);
                                ConnectionInformation c = new ConnectionInformation(replyFromComputer, acceptedConnections, this, parser.Clone() as IDataParser, Ip);
                                reportUserLogin(Ip);
                                c.connectionChanged += c_connectionChanged;
                                //activeConnections.Add(acceptedConnections, c);
                                if (connectionEvent != null)
                                    connectionEvent(c);



                            }
                            //else
                            //    Out.writeLine("Connection denied from [" + replyFromComputer.RemoteEndPoint.ToString().Split(':')[0] + "]. The user has too many connections", Out.logFlags.StandardLogLevel);
                        }
                        else
                        {
                            Out.writeLine("Maximum amount of connections reached " + maximumConnections, Out.logFlags.ImportantLogLevel);
                            replyFromComputer.Shutdown(SocketShutdown.Both);
                            replyFromComputer.Close();
                        }
                    }
                    catch
                    {
                        //Out.writeError(ex.Message); }
                        
                    }
                    finally
                    {
                        connectionListener.BeginAccept(new AsyncCallback(newConnectionRequest), connectionListener);
                    }
                }
                else
                {
                    Out.writeLine("Connection denied, server is not currently accepting connections!", Out.logFlags.StandardLogLevel);
                }
            }
        }

        void c_connectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.closed)
            {
                reportDisconnect(information);
            }
        }

        #endregion

        #region connection disconnected

        /// <summary>
        /// Reports a gameconnection as disconnected
        /// </summary>
        /// <param name="gameConnection">The connection which is logging out</param>
        public void reportDisconnect(ConnectionInformation gameConnection)
        {
            gameConnection.connectionChanged -= c_connectionChanged;
            reportUserLogout(gameConnection.getIp());
            //activeConnections.Remove(gameConnection.getConnectionID());
        }

        #endregion

        #region ip connection management

        /// <summary>
        /// reports the user with an ip as "logged in"
        /// </summary>
        /// <param name="ip">The ip of the user</param>
        private void reportUserLogin(string ip)
        {
            alterIpConnectionCount(ip, (getAmountOfConnectionFromIp(ip) + 1));
        }

        /// <summary>
        /// reports the user with an ip as "logged out"
        /// </summary>
        /// <param name="ip">The ip of the user</param>
        private void reportUserLogout(string ip)
        {
            alterIpConnectionCount(ip, (getAmountOfConnectionFromIp(ip) - 1));
        }

        /// <summary>
        /// Alters the ip connection count
        /// </summary>
        /// <param name="ip">The ip of the user</param>
        /// <param name="amount">The amount of connections</param>
        private void alterIpConnectionCount(string ip, int amount)
        {
            //if (ipConnectionCount.ContainsKey(ip))
            //    ipConnectionCount.Remove(ip);
            //ipConnectionCount.Add(ip, amount);
        }

        /// <summary>
        /// Gets the amount of connections from 1 ip
        /// </summary>
        /// <param name="ip">The ip of the user</param>
        /// <returns>The amount of connections from the given ip address</returns>
        private int getAmountOfConnectionFromIp(string ip)
        {
            //if (ipConnectionCount.ContainsKey(ip))
            //{
            //    return ipConnectionCount[ip];
            //}
            //else
            //{
            //    return 0;
            //}
            return 0;
        }

        #endregion

        #region getters
        /// <summary>
        /// Returns connection information about this item
        /// </summary>
        /// <returns>Indicator if this item is connected or not</returns>
        internal bool isConnected()
        {
            return this.connectionListener != null;
        }
        #endregion


        public int getAcceptedConnections()
        {
            return this.acceptedConnections;
        }
    }
}
