/*
 *  Copyright (c) 2016 IBM Corporation and other Contributors.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html 
 *
 * Contributors:
 * 	 kaberi Singh - Initial Contribution
 *   Hari hara prasad Viswanathan  - updations
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using static uPLibrary.Networking.M2Mqtt.MqttClient;

namespace IBMWatsonIoTP
{
    /// <summary>
    ///     A client that handles connections with the IBM Watson IoT Platform. <br>
    ///     This is an abstract class which has to be extended
    /// </summary>
    public abstract class AbstractClient
    {
        private string clientUsername;
        private string clientPassword;
        private string clientId;
        private string orgId;
        
        protected static readonly String CLIENT_ID_DELIMITER = ":";
        protected static readonly String DOMAIN = ".messaging.internetofthings.ibmcloud.com";
        protected static readonly int MQTTS_PORT = 8883;
        protected MqttClient mqttClient;

        // 切断時のイベント
        public event ConnectionClosedEventHandler ConnectionClosed;

        /// <summary>
        ///     Note that this class does not have a default constructor <br>
	    /// </summary>
        /// <param name="orgid">
        ///     object of String which denotes OrgId </param>
        /// <param name="clientId">
        ///     object of String which denotes clientId </param>
        /// <param name="userName">
        ///     object of String which denotes userName </param>
        /// <param name="password">
        ///     object of String which denotes password </param>
        public AbstractClient(string orgid, string clientId, string userName, string password)
        {
            this.clientId = clientId;
            this.clientUsername = userName;
            this.clientPassword = password;
            this.orgId = orgid;
            String now = DateTime.Now.ToString(".yyyy.MM.dd-THH.mm.fff");

            string hostName = orgid + DOMAIN;
            
            mqttClient = new MqttClient(hostName);
            mqttClient.ConnectionClosed += (sender, e) =>
            {
                if(null != ConnectionClosed)
                {
                    ConnectionClosed(sender, e);
                }
            };

        }

       

        /// <summary>
        ///     Connect the device from the IBM Watson IoT Platform
        /// </summary>
        public virtual void connect()
        {
            try 
            {
                
                if(orgId == "quickstart"){
                    mqttClient.Connect(clientId);
                }
                else
                {
                	mqttClient.Connect(clientId, clientUsername, clientPassword);
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        ///     Disconnect the device from the IBM Watson IoT Platform
        /// </summary>
        public void disconnect()
        {
            try
            {
                mqttClient.Disconnect();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        ///     Determine whether this device or application is currently connected to the IBM Internet
        ///     of Things Foundation.
        /// </summary>
        /// <returns>
        ///     Whether the device or application is connected to the IBM Watson IoT Platform
        /// </returns>
        public bool isConnected()
        {
            return mqttClient.IsConnected;
        }

        /// <summary>
        ///     Provides a human readable String representation of this Device, including the number
        ///     of messages sent and the current connect status.
        /// </summary>
        /// <returns>
        ///     String representation of the Device status
        /// </returns>
        public string toString()
        {
            return "[" + clientId + "] " +  "Connected = " + isConnected();
        }
		
        /// <summary>
        ///  Parse the given file and gives key value pared dictionary of parameters separated with "=",parsing starts after the lien with the given region and end with a new line 
        /// </summary>
        /// <param name="path">
        /// file path to be parsed</param>
        /// <param name="region">
        /// starting line of the requires segment</param>
        /// <returns>
        /// Dictionary of key,value pair in string,string format </returns>
  //      public static Dictionary<string, string> parseFile(string path , string region)
		//{
		//	Dictionary<string, string>  myDictionary = new Dictionary<string, string>();
		//	string[] lines = System.IO.File.ReadAllLines(path);
		//	var idx =Array.FindIndex(lines, row => row == region);
		//	for (int i = idx+1; i < lines.Length; i++) {
		//		if(string.IsNullOrWhiteSpace(lines[i]))
		//		   break;
		//		string[] tokens = lines[i].Split('=');
		//		myDictionary.Add(tokens[0].Trim(),tokens[1].Trim());
			
		//	}
		//	return myDictionary;
		//}
    }
}