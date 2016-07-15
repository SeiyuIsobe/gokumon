using ShimadzuGPIO;
using ShimadzuIoT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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



// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace RemoChassis
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        AccelerIoT _iot = new AccelerIoT();
        Master _gpio = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (null == _gpio)
            {
                _gpio = new Master();
                _gpio.Tick += async (ss, ee) =>
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            if(null != ee)
                            {
                                _ell.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                _ell.Visibility = Visibility.Collapsed;
                            }
                        });
                };
                _gpio.InitGPIO();
            }

            _iot = new AccelerIoT();
            _iot.WsUri = "ws://sukekiyo.mybluemix.net/ws/accera";
            _iot.PropertyChanged += async (ss, ee) =>
            {
                //System.Diagnostics.Debug.WriteLine("-> " + _iot.Acceler.AcceraX.ToString());
                
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    this.AccelerX = _iot.Acceler.AcceraX;
                    this.AccelerZ = _iot.Acceler.AcceraZ * -1.0;

                    _gpio.Change(this.AccelerZ);
                });
            };

            _iot.Start();

            button.IsEnabled = false;
        }

        private double _accelerX = 0.0;
        public double AccelerX
        {
            get
            {
                if (null == _iot.Acceler) return 0.0;
                return _accelerX;
            }

            set
            {
                _accelerX = value;
                this.NotifyPropertyChanged();
            }
        }

        public double AccelerZ
        {
            get
            {
                if (null == _iot.Acceler) return 0.0;
                return _accelerZ;
            }

            set
            {
                _accelerZ = value;
                this.NotifyPropertyChanged();
            }
        }

        private double _accelerZ = 0.0;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
