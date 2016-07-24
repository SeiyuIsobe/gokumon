/*
 *  Copyright (c) 2016 IBM Corporation and other Contributors.
 *
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html 
 *
 * Contributors:
 *   Hari hara prasad Viswanathan  - Initial Contribution
 */
 
using System;
using System.Text;
using System.Dynamic;
using System.Threading;
using System.Collections.Generic;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using log4net;

namespace IBMWIoTP
{
	/// <summary>
    ///    A device class that connects the device as managed device to IBM Watson IoT Platform Connect and enables the device to perform one or more Device Management operations. Also the DeviceManagement instance can be used to do normal device operations like publishing device events and listening for commands from application.
    /// </summary>
	public class DeviceManagement : DeviceClient
	{
		// Publish MQTT topics
		string MANAGE_TOPIC = "iotdevice-1/mgmt/manage";
		string UNMANAGE_TOPIC = "iotdevice-1/mgmt/unmanage";
		string UPDATE_LOCATION_TOPIC = "iotdevice-1/device/update/location";
		string ADD_ERROR_CODE_TOPIC = "iotdevice-1/add/diag/errorCodes";
		string CLEAR_ERROR_CODES_TOPIC = "iotdevice-1/clear/diag/errorCodes";
		string NOTIFY_TOPIC = "iotdevice-1/notify";
		string RESPONSE_TOPIC = "iotdevice-1/response";
		string ADD_LOG_TOPIC = "iotdevice-1/add/diag/log";
		string CLEAR_LOG_TOPIC = "iotdevice-1/clear/diag/log";
		
		public DeviceInfo deviceInfo = new DeviceInfo();
		public LocationInfo locationInfo = new LocationInfo();
		List<DMRequest> collection = new List<IBMWIoTP.DeviceManagement.DMRequest>();
		ManualResetEvent oSignalEvent = new ManualResetEvent(false);
		bool isSync = false;
		ILog log = log4net.LogManager.GetLogger(typeof(DeviceManagement));
		
		/// <summary>
		///  Constructor of device management, helps to connect as a managed device to Watson IoT 
 		/// </summary>
		/// <param name="orgId">
		///  object of String which denotes your organization Id</param>
		/// <param name="deviceType">object of String which denotes your device type</param>
		/// <param name="deviceID">object of String which denotes device Id</param>
		/// <param name="authmethod">object of String which denotes your authentication method</param>
		/// <param name="authtoken">object of String which denotes your authentication token</param>
		public DeviceManagement(string orgId, string deviceType, string deviceID, string authmethod, string authtoken):
			base(orgId,deviceType,deviceID,authmethod,authtoken)
		{
			
		}
		/// <summary>
		///  Constructor of device management, helps to connect as a managed device to Watson IoT 
		/// </summary>
		///  object of String which denotes your organization Id</param>
		/// <param name="orgId">
		///  object of String which denotes your organization Id</param>
		/// <param name="deviceType">object of String which denotes your device type</param>
		/// <param name="deviceID">object of String which denotes device Id</param>
		/// <param name="authmethod">object of String which denotes your authentication method</param>
		/// <param name="authtoken">object of String which denotes your authentication token</param>
		/// <param name="isSync"> Boolean value to represent the device management request and response in synchronize mode or Async mode</param>
		public DeviceManagement(string orgId, string deviceType, string deviceID, string authmethod, string authtoken,bool isSync):
			base(orgId,deviceType,deviceID,authmethod,authtoken)
		{
			this.isSync = isSync;
		}
		/// <summary>
		///  Constructor of device management, helps to connect as a managed device to Watson IoT  
		/// </summary>
		/// <param name="filePath">object of String which denotes file path that contains device credentials in specified format</param>
		public DeviceManagement(string filePath):
			base(filePath)
		{
		}
		/// <summary>
		///  Constructor of device management, helps to connect as a managed device to Watson IoT  
		/// </summary>
		/// <param name="filePath">object of String which denotes file path that contains device credentials in specified format</param>
		/// <param name="isSync">Boolean value to represent the device management request and response in synchronize mode or Async mode</param>
		public DeviceManagement(string filePath,bool isSync):
			base(filePath)
		{
			this.isSync = isSync;
		}
		/// <summary>
		/// To connect the device to the Watson IoT Platform
		/// </summary>
		public override void connect()
		{
			base.connect();
			suscribeTOManagedTopics();
		}
		
		
		class DMRequest
		{
			public  DMRequest()
			{
			}
			public  DMRequest(string reqId, string topic ,string json)
			{
				this.reqID = reqId;
				this.topic = topic;
				this.json =json;
			}
			public string reqID {get;set;}
			public string topic {get;set;}
			public string json {get;set;}
		}
		
		class DMResponce
		{
			public DMResponce()
			{
			}
			public string reqId {get;set;}
			public string rc {get;set;}
			
		}


		
		private void suscribeTOManagedTopics(){
			if(mqttClient.IsConnected)
			{
				
				string[] topics = { "iotdm-1/#"};
				byte[] qos = {1};
				mqttClient.Subscribe(topics, qos);
				mqttClient.MqttMsgPublishReceived += subscriptionHandler;
				log.Info("Subscribes to topic [" +topics[0] + "]");
			}
		}		
		
		public void subscriptionHandler(object sender, MqttMsgPublishEventArgs e)
        {
			try{
	            string result = System.Text.Encoding.UTF8.GetString(e.Message);
	            var serializer  = new System.Web.Script.Serialization.JavaScriptSerializer();
	            var responce = serializer.Deserialize<DMResponce>(result);
	            var itm =  collection.Find( x => x.reqID == responce.reqId );
	            if( itm is DMRequest)
	            {
	            	log.Info("["+responce.rc+"]  "+itm.topic+" of ReqId  "+ itm.reqID);
	            	if(this.mgmtCallback !=null)
	            		this.mgmtCallback(itm.reqID,responce.rc);
	            	collection.Remove(itm);
	            }
	             if(this.isSync){
		            oSignalEvent.Set();
	            	oSignalEvent.Dispose();
	            	oSignalEvent = new ManualResetEvent(false);
	            }
			}
        	catch(Exception ex)
        	{
        		log.Error("Execption has occer in subscriptionHandler ",ex);
        	}

        }
		/// <summary>
		///  To know the current connection status of the device
		/// </summary>
		/// <returns> bool value of status of connection </returns>
		public bool connectionStatus()
		{
			return mqttClient.IsConnected;
		}
		
		private void publishDM (string topic , object message,string reqId)
		{
			var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(message);
			log.Info("Device Management Request For Topic " + topic +" with payload " +json);
			collection.Add(new DMRequest(reqId,topic,json));
			mqttClient.Publish(topic, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE , false);
			if(this.isSync)
				oSignalEvent.WaitOne();
		}
		/// <summary>
		///  To register the device as an managed device to Watson IoT Platform
		/// </summary>
		/// <param name="lifeTime">Time period of the device to remain as managed device</param>
		/// <param name="supportDeviceActions">bool value to represent the support for device actions for the device</param>
		/// <param name="supportFirmwareActions">bool value to represent the support for firmware action for the device</param>
		/// <returns>unique id of the current request</returns>
		public string manage(int lifeTime,bool supportDeviceActions,bool supportFirmwareActions)
		{
			try{
				string uid =Guid.NewGuid().ToString();
				var payload = new {
					lifetime =  lifeTime,
					supports = new {
						deviceActions =  supportDeviceActions,
						firmwareActions = supportFirmwareActions
					},
					deviceInfo = this.deviceInfo ,
					metadata = new {}
				};
				var message = new { reqId = uid , d = payload };
				publishDM(MANAGE_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		
		}
		/// <summary>
		///  To register the device as an managed device to Watson IoT Platform
		/// </summary>
		/// <param name="lifeTime">Time period of the device to remain as managed device</param>
		/// <param name="supportDeviceActions">bool value to represent the support for device actions for the device</param>
		/// <param name="supportFirmwareActions">bool value to represent the support for firmware action for the device</param>
		/// <param name="metaData"> Object that represent the meta information of the device </param>
		/// <returns>unique id of the current request</returns>
		public string manage(int lifeTime,bool supportDeviceActions,bool supportFirmwareActions, Object metaData)
		{
			try{
				string uid =Guid.NewGuid().ToString();
				var payload = new {
					lifetime =  lifeTime,
					supports = new {
						deviceActions =  supportDeviceActions,
						firmwareActions = supportFirmwareActions
					},
					deviceInfo = this.deviceInfo ,
					metadata = metaData
				};
				var message = new { reqId =uid , d = payload };
				publishDM(MANAGE_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
			
		}
		/// <summary>
		///  To register the device as an Unmanaged device to Watson Iot Platform
		/// </summary>
		/// <returns>unique id of the current request</returns>
		public string unmanage()
		{
			try{
				string uid =Guid.NewGuid().ToString();
				var message = new { reqId =uid };
				publishDM(UNMANAGE_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		/// <summary>
		///  To Add error code to the Watson IoT platform for the device 
		/// </summary>
		/// <param name="errorCode">integer value of device error code </param>
		/// <returns>unique id of the current request</returns>
		public string addErrorCode(int errorCode)
		{
			try
			{
				string uid =Guid.NewGuid().ToString();
				var payload = new{
					errorCode = errorCode
				};
				var message = new { reqId =uid ,d = payload};
				publishDM(ADD_ERROR_CODE_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		
		/// <summary>
		///  To Clear the error code added by the device in the Watson IoT Platform
		/// </summary>
		/// <returns>unique id of the current request</returns>
		public string clearErrorCode()
		{
			try
			{
				string uid =Guid.NewGuid().ToString();
				var message = new { reqId =uid};
				publishDM(CLEAR_ERROR_CODES_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		/// <summary>
		///  To Add log of the device status to Watson IoT Platform 
		/// </summary>
		/// <param name="msg">String value of the log message </param>
		/// <param name="dataAsString"> String value of base64-encoded binary diagnostic data </param>
		/// <param name="severityValue">intiger value of severity (0: informational, 1: warning, 2: error)</param>
		/// <returns>unique id of the current request</returns>
		public string addLog(string msg, string dataAsString,int severityValue)
		{
			try
			{
				string uid =Guid.NewGuid().ToString();
				var payload = new{
					message = msg,
					timestamp = DateTime.UtcNow.ToString("o"),
					data = dataAsString,
					severity = severityValue
				};
				var message = new { reqId =uid ,d = payload};
				publishDM(ADD_LOG_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		
		/// <summary>
		///  To clear the log present in Watson IoT platform for the current device
		/// </summary>
		/// <returns>unique id of the current request</returns>
		public string clearLog()
		{
			try
			{
				string uid =Guid.NewGuid().ToString();
				var message = new { reqId =uid};
				publishDM(CLEAR_LOG_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		/// <summary>
		/// To Update the current location of the device to the Watson IoT Platform
		/// </summary>
		/// <param name="longitude">double value of longitude of the device </param>
		/// <param name="latitude">double value of latitude of the device</param>
		/// <param name="elevation">double value of elevation of the device</param>
		/// <param name="accuracy">double value of accuracy of the reading</param>
		/// <returns>unique id of the current request</returns>
		public string setLocation( double  longitude,double latitude,double elevation,double accuracy)
		{
			try
			{
				string uid =Guid.NewGuid().ToString();
				this.locationInfo.longitude = longitude;
				this.locationInfo.latitude = latitude;
				this.locationInfo.elevation = elevation;
				this.locationInfo.accuracy =accuracy;
				this.locationInfo.measuredDateTime = DateTime.Now.ToString("o");
				//this.locationInfo.updatedDateTime = this.locationInfo.measuredDateTime;
				var message = new { reqId =uid ,d = this.locationInfo };
				publishDM(UPDATE_LOCATION_TOPIC,message,uid);
				return uid;
			}
        	catch(Exception e)
        	{
        		log.Error("Execption has occer in manage ",e);
        		return "";
        	}
		}
		
        public delegate void processMgmtResponce( string reqestId, string responceCode);

        public event processMgmtResponce mgmtCallback;
    
	
	}
}
