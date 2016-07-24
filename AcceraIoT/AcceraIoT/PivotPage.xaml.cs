using AcceraIoT.Common;
using AcceraIoT.Data;
using AcceraIoT.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ピボット アプリケーション テンプレートについては、http://go.microsoft.com/fwlink/?LinkID=391641 を参照してください

namespace AcceraIoT
{
    public sealed partial class PivotPage : Page, INotifyPropertyChanged
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            InitDevice();

            this.DataContext = this;

        }

        private Accelerometer _accelerometer = null;
        
        private Geolocator _watcher = null;

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
                _accelerometer.ReportInterval = reportInterval;

                _accelerometer.ReadingChanged += _accelerometer_ReadingChanged;
            }

            // GPSを使う
            if (null == _watcher)
            {
                _watcher = new Geolocator();
                if(null != _watcher)
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

        private async void _accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs e)
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
                        _client.Publish(this.XAxis, this.YAxis, this.ZAxis, this.Latitude, this.Longitude);
                    }
                }

            });
        }

        /// <summary>
        /// この <see cref="Page"/> に関連付けられた <see cref="NavigationHelper"/> を取得します。
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// この <see cref="Page"/> のビュー モデルを取得します。
        /// これは厳密に型指定されたビュー モデルに変更できます。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// このページには、移動中に渡されるコンテンツを設定します。前のセッションからページを
        /// 再作成する場合は、保存状態も指定されます。
        /// </summary>
        /// <param name="sender">
        /// イベントのソース (通常、<see cref="NavigationHelper"/>)。
        /// </param>
        /// <param name="e">このページが最初に要求されたときに
        /// <see cref="Frame.Navigate(Type, Object)"/> に渡されたナビゲーション パラメーターと、
        /// 前のセッションでこのページによって保存された状態のディクショナリを提供する
        /// セッション。ページに初めてアクセスするとき、状態は null になります。</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: 対象となる問題領域に適したデータ モデルを作成し、サンプル データを置き換えます
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
        }

        /// <summary>
        /// アプリケーションが中断される場合、またはページがナビゲーション キャッシュから破棄される場合、
        /// このページに関連付けられた状態を保存します。値は、
        /// <see cref="SuspensionManager.SessionState"/> のシリアル化の要件に準拠する必要があります。
        /// </summary>
        /// <param name="sender">イベントのソース (通常、<see cref="NavigationHelper"/>)。</param>
        /// <param name="e">シリアル化可能な状態で作成される空のディクショナリを提供するイベント データ
        ///。</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: ページの一意の状態をここに保存します。
        }

        /// <summary>
        /// アプリ バー ボタンがクリックされたときに項目を一覧に追加します。
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            string groupName = this.pivot.SelectedIndex == 0 ? FirstGroupName : SecondGroupName;
            var group = this.DefaultViewModel[groupName] as SampleDataGroup;
            var nextItemId = group.Items.Count + 1;
            var newItem = new SampleDataItem(
                string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
                string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
                string.Empty,
                string.Empty,
                this.resourceLoader.GetString("NewItemDescription"),
                string.Empty);

            group.Items.Add(newItem);

            // 新しい項目をスクロールして表示します。
            var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
            var listView = container.ContentTemplateRoot as ListView;
            listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// セクション内のアイテムがクリックされたときに呼び出されます。
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 適切な移動先のページに移動し、新しいページを構成します。
            // このとき、必要な情報をナビゲーション パラメーターとして渡します
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            if (!Frame.Navigate(typeof(ItemPage), itemId))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        /// <summary>
        /// スクロールして表示するときに 2 番目のピボット項目のコンテンツを読み込みます。
        /// </summary>
        private async void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-2");
            this.DefaultViewModel[SecondGroupName] = sampleDataGroup;
        }

        #region NavigationHelper の登録

        /// <summary>
        /// このセクションに示したメソッドは、NavigationHelper がページの
        /// ナビゲーション メソッドに応答できるようにするためにのみ使用します。
        /// <para>
        /// ページ固有のロジックは、
        /// <see cref="NavigationHelper.LoadState"/>
        /// および <see cref="NavigationHelper.SaveState"/>。
        /// LoadState メソッドでは、前のセッションで保存されたページの状態に加え、
        /// ナビゲーション パラメーターを使用できます。
        /// </para>
        /// </summary>
        /// <param name="e">ナビゲーション要求を取り消すことのできないナビゲーション メソッドおよびイベント
        /// ハンドラーにデータを提供します。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        SampleAppClient _client = null;

        /// <summary>
        /// 接続ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _connectBluemix_Click(object sender, RoutedEventArgs e)
        {
            if(null == _client)
            {
                _client = new SampleAppClient();
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
                if(true == _client.IsConnected)
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

        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            // GPSを使う
            if (null == _watcher)
            {
                _watcher = new Geolocator();
                _watcher.MovementThreshold = 20;
                _watcher.PositionChanged += this._watcher_PositionChanged;
                _watcher.StatusChanged += this._watcher_StatusChanged;

                //

                //await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                //                async () =>
                //                {
                //                    Geoposition pos = await _watcher.GetGeopositionAsync();
                //                });

            }
        }

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
