using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static uPLibrary.Networking.M2Mqtt.MqttClient;

namespace IBMWatsonIoTP.Sample
{
    public class AppClient
    {
        private string _orgId = "";
        private string _appId = "";
        private string _apiKey = "";
        private string _authToken = "";

        public void Do()
        {
            _orgId = "ymn9fh";
            _appId = "64006a567f07";
            _apiKey = "use-token-auth";
            _authToken = "AppSukekiyo";
            _apiKey = "a-ymn9fh-vaqdi4hid9"; // これだとつながる
            _authToken = "qEmrmrkP4?e2H6*0r5"; // これだと繋がる

            ApplicationClient applicationClient = new ApplicationClient(_orgId, _appId, _apiKey, _authToken);
            applicationClient.connect();

            string deviceType = "SukekiyoApp";
            string deviceId = "64006a567f07";
            string data = "name:foo,cpu:60,mem:50";

            applicationClient.publishCommand(deviceType, deviceId, "testcmd", "json", data, 0);
            applicationClient.disconnect();
        }

        private DeviceClient _client = null;


        // 切断時のイベント
        public event ConnectionClosedEventHandler ConnectionClosed;

        public void DoDevice()
        {
            _orgId = "ymn9fh";
            _authToken = "AppSukekiyo";
            string deviceType = "SukekiyoApp";
            string deviceId = "64006a567f07";

            if (null == _client)
            {
                _client = new DeviceClient(_orgId, deviceType, deviceId, "token", _authToken);

                _client.ConnectionClosed += (sender, e) =>
                {
                    if (null != ConnectionClosed)
                    {
                        ConnectionClosed(sender, e);
                    }
                };
            }

            // 接続
            _client.connect();

            //_client.disconnect();
        }

        private AcceraData _acceraData = null;
        private GeopotionData _geoData = null;

        public void Publish(double x, double y, double z)
        {
            _acceraData = new AcceraData(x, y, z);

            _client.publishEvent("test", "json", _acceraData.Data, 0);
        }

        public void Publish(double x, double y)
        {
            _geoData = new GeopotionData(x, y);

            _client.publishEvent("test", "json", _geoData.Data, 0);
        }

        public void Publish(double x, double y, double z, double la, double lo)
        {
            _acceraData = new AcceraData(x, y, z);
            _geoData = new GeopotionData(la, lo);

            string d = "{\"Ac\":" + _acceraData.Data + "," + "\"Ge\":" + _geoData.Data + "}";

            _client.publishEvent("test", "json", d, 0);
        }

        public void Publish(double x, double y, double z, double la, double lo, long elapsedtime)
        {
            _acceraData = new AcceraData(x, y, z);
            _geoData = new GeopotionData(la, lo);

            string d = "{";
            d += "\"ElapsedTime\": " + elapsedtime.ToString() + ",";
            d += "\"Ac\":" + _acceraData.Data + "," + "\"Ge\":" + _geoData.Data + "}";

            _client.publishEvent("test", "json", d, 0);
        }

        public bool IsConnected
        {
            get
            {
                if (null == _client) return false;
                return _client.isConnected();
            }
        }

        public void Disconnect()
        {
            _client.disconnect();
        }
    }

    public class AcceraData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public AcceraData(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public string Data
        {
            get
            {
                return "{" + "\"AcceraX\":" + this.X.ToString() + "," + "\"AcceraY\":" + this.Y.ToString() + "," + "\"AcceraZ\":" + this.Z.ToString() + "}";
            }
        }
    }

    public class GeopotionData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeopotionData(double la, double lo)
        {
            this.Latitude = la;
            this.Longitude = lo;
        }

        public string Data
        {
            get
            {
                return "{" + "\"Latitude\":" + this.Latitude.ToString() + "," + "\"Longitude\":" + this.Longitude.ToString() + "}";
            }
        }
    }
}
