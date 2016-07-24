using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using IBMWatsonIoTP.Sample;
using System.Diagnostics;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace BluePublisher
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public MainPage()
        {
            this.InitializeComponent();

            InitDevice();

            this.DataContext = this;
        }


        private Accelerometer _accelerometer = null;

        private Geolocator _watcher = null;

        private Stopwatch _stopwatch = null;

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private async void InitDevice()
        {
            // 加速度計を使う
            _accelerometer = Accelerometer.GetDefault();
            if (null != _accelerometer)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                //_accelerometer.ReportInterval = reportInterval;
                _accelerometer.ReportInterval = 100; // 100ミリにする

                _accelerometer.ReadingChanged += _accelerometer_ReadingChanged;
            }

            // GPSを使う
            if (null == _watcher)
            {
                _watcher = new Geolocator();
                if (null != _watcher)
                {
                    _watcher.MovementThreshold = 20;
                    _watcher.PositionChanged += this._watcher_PositionChanged;
                    _watcher.StatusChanged += this._watcher_StatusChanged;

                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            Geoposition pos = await _watcher.GetGeopositionAsync();
                        });

                }

            }
        }

        private long _prereportcount = 0;

        private async void _accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs e)
        {
            var et = this.GetElapsedTime();

            // なんとなく加速度が変化したイベントがReportIntervalより早く来ている
            // 気がするので、念のため前回の経過時間を保持しておいて
            // ReportIntervalより経過指定かどうかを確かめることにする
            if (et - _prereportcount >= _accelerometer.ReportInterval)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AccelerometerReading reading = e.Reading;
                    this.XAxis = reading.AccelerationX;
                    this.YAxis = reading.AccelerationY;
                    this.ZAxis = reading.AccelerationZ;

                    if (null != _client)
                    {
                        if (true == _client.IsConnected)
                        {
                            _client.Publish(this.XAxis, this.YAxis, this.ZAxis, this.Latitude, this.Longitude, et);
                            //_client.Publish(this.XAxis, this.YAxis, this.ZAxis, this.Latitude, this.Longitude);
                        }
                    }

                });

                _prereportcount = et;
            }
        }

        private long GetElapsedTime()
        {
            if(null == _stopwatch)
            {
                _stopwatch = Stopwatch.StartNew();
                return 0;
            }

            return _stopwatch.ElapsedMilliseconds;
        }

        //SampleAppClient _client = null;
        AppClient _client = null;

        /// <summary>
        /// 接続ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _connectBluemix_Click(object sender, RoutedEventArgs e)
        {
            if (null == _client)
            {
                _client = new AppClient();
                _client.ConnectionClosed += async (ss, ee) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _connectBluemixCaption.Text = "接続";

                        _client = null;
                    });
                };

                // ボタン
                _connectBluemixCaption.Text = "切断";

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    _client.DoDevice();
                });
            }
            else
            {
                if (true == _client.IsConnected)
                {
                    _client.Disconnect();
                }
            }



        }

        /// <summary>
        /// X軸
        /// </summary>
        private double _xAxis = 0.0;
        public double XAxis
        {
            get
            {
                return _xAxis;
            }

            set
            {
                _xAxis = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Y軸
        /// </summary>
        private double _yAxis = 0.0;
        public double YAxis
        {
            get
            {
                return _yAxis;
            }

            set
            {
                _yAxis = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Z軸
        /// </summary>
        private double _zAxis = 0.0;
        public double ZAxis
        {
            get
            {
                return _zAxis;
            }

            set
            {
                _zAxis = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// 緯度
        /// </summary>
        public double Latitude
        {
            get
            {
                return _latitude;
            }

            set
            {
                _latitude = value;
            }
        }

        /// <summary>
        /// 経度
        /// </summary>
        public double Longitude
        {
            get
            {
                return _longitude;
            }

            set
            {
                _longitude = value;
            }
        }

        private double _latitude = 0.0;
        private double _longitude = 0.0;

        private void _watcher_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            //await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            //    () =>
            //    {
            Geoposition pos = e.Position;

            this.Latitude = pos.Coordinate.Point.Position.Latitude;
            this.Longitude = pos.Coordinate.Point.Position.Longitude;
            //_tbAccuracy.Text = pos.Coordinate.Accuracy.ToString();

            System.Diagnostics.Debug.WriteLine("{0},{1}", pos.Coordinate.Point.Position.Latitude, pos.Coordinate.Point.Position.Longitude);


            //// Specify a known location.
            //BasicGeoposition snPosition = new BasicGeoposition
            //{
            //    Latitude = pos.Coordinate.Point.Position.Latitude,
            //    Longitude = pos.Coordinate.Point.Position.Longitude
            //};
            //});
        }

        private async void _watcher_StatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                                () =>
                                {
                                    //_tbStatus.Text = GetStatusString(sender.LocationStatus);
                                });
        }

        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:
                    strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }

            return (strStatus);

        }
    }

}
