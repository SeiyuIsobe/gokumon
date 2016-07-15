using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace ShimadzuGPIO
{
    public class Master
    {
        private const int LED_PIN = 5;
        private GpioPin _pin;
        private GpioPinValue _pinvalue;

        public event EventHandler Tick;

        public Master()
        {
            this.HighTick += (sender, e) =>
            {
                // High
                _pinvalue = GpioPinValue.High;
                _pin.Write(_pinvalue);

                if (null != Tick)
                {
                    Tick(null, new EventArgs());
                }
            };

            this.LowTick += (sender, e) =>
            {
                // Low
                _pinvalue = GpioPinValue.Low;
                _pin.Write(_pinvalue);

                if (null != Tick)
                {
                    Tick(null, null);
                }
            };
        }

        public void InitGPIO()
        {
            var gpio = GpioController.GetDefault();
            if(null == gpio)
            {
                _pin = null;
                return;
            }

            // 5番ピン
            _pin = gpio.OpenPin(LED_PIN);
            _pin.Write(GpioPinValue.High);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
            _pinvalue = GpioPinValue.High;

            // ストップウォッチ
            _stopwatch = new Stopwatch();
        }

        private Stopwatch _stopwatch = null;
        private BackgroundWorker _worker = null;

        private event EventHandler HighTick;
        private event EventHandler LowTick;

        public void Start()
        {
            if(null == _worker)
            {
                _worker = new BackgroundWorker();
                _worker.DoWork += (sender, e) =>
                {
                    _stopwatch.Start();

                    bool isHigh = false;

                    while(true)
                    {
                        // 間隔経過
                        if(_current_interval <= _stopwatch.ElapsedMilliseconds)
                        {
                            _stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            HighTick(null, null);
                            isHigh = true;
                        }
                        else if(20 <= _stopwatch.ElapsedMilliseconds)
                        {
                            if(true == isHigh)
                            {
                                LowTick(null, null);
                                isHigh = false;
                            }
                        }
                    }
                };
                _worker.RunWorkerAsync();
            }
            
        }

        #region
        //public void Start()
        //{
        //    _timer = new Timer(
        //        (sender) =>
        //        {
        //            if (null == _changeAction)
        //            {
        //                // High
        //                _pinvalue = GpioPinValue.High;
        //                _pin.Write(_pinvalue);

        //                // PWM
        //                _pwmTimer = new Timer(
        //                    (ss) =>
        //                    {
        //                        if (null != _pwmTimer)
        //                        {
        //                            _pinvalue = GpioPinValue.Low;
        //                            _pin.Write(_pinvalue);
        //                            _pwmTimer = null; // nullにして2回目を拾わないようにする
        //                        }

        //                    },
        //                    null, 50, Timeout.Infinite);


        //                if (null != Tick)
        //                {
        //                    Tick(null, null);
        //                }
        //            }
        //            else
        //            {
        //                _changeAction();
        //            }
        //        },
        //        null, 0, 0);
        //}

        //public void Start(int t)
        //{
        //    _timer = new Timer(
        //        (sender) =>
        //        {
        //            if (null != Tick)
        //            {
        //                Tick(null, null);
        //            }
        //        },
        //        null, 0, t);
        //}
        #endregion


        public void Stop()
        {
            _worker.CancelAsync();
            _worker.Dispose();
            _worker = null;
        }

        private int _current_interval = 1000;

        /// <summary>
        /// tは加速度の値
        /// </summary>
        /// <param name="t"></param>
        public void Change(double t)
        {
            if(0.0 <= t && t < 0.2)
            {
                if(1000 != _current_interval)
                {
                    _current_interval = 3000;
                }
            }
            else if(0.2 <= t && t < 0.4)
            {
                if (770 != _current_interval)
                {
                    _current_interval = 750;
                }
            }
            else if(0.4 <= t && t < 0.6)
            {
                if (530 != _current_interval)
                {
                    _current_interval = 550;
                }
            }
            else if(0.6 <= t && t < 0.8)
            {
                if (290 != _current_interval)
                {
                    _current_interval = 325;
                }
            }
            else if(0.8 <= t && t <= 1.0)
            {
                if (50 != _current_interval)
                {
                    _current_interval = 100;
                }
            }

            if (null == _worker)
            {
                this.Start();
            }
        }
    }
}
