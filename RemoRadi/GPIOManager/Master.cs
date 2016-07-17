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
        protected GpioPin _pin = null;
        //private GpioPinValue _pinvalue;

        public event EventHandler Tick;

        public Master()
        {
            this.HighTick += (sender, e) =>
            {
                // High
                //WritePin(5);
                WritePin(5, GpioPinValue.High);

                if (null != Tick)
                {
                    Tick(null, new EventArgs());
                }
            };

            this.LowTick += (sender, e) =>
            {
                // Low
                WritePin(5, GpioPinValue.Low);

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
                return;
            }

            //// 5番ピン
            //_pin = gpio.OpenPin(LED_PIN);
            //_pin.Write(GpioPinValue.High);
            //_pin.SetDriveMode(GpioPinDriveMode.Output);
            //_pinvalue = GpioPinValue.High;

            //// 6番ピン
            //gpio.OpenPin(6).Write(GpioPinValue.Low);
            ////gpio.OpenPin(6).SetDriveMode(GpioPinDriveMode.Output);

            _pin5 = gpio.OpenPin(5);
            _pin5.Write(GpioPinValue.Low);
            _pin5.SetDriveMode(GpioPinDriveMode.Output);

            _pin6 = gpio.OpenPin(6);
            _pin6.Write(GpioPinValue.Low);
            _pin6.SetDriveMode(GpioPinDriveMode.Output);

            // ストップウォッチ
            _stopwatch = new Stopwatch();
        }

        private GpioPin _pin5 = null;
        private GpioPin _pin6 = null;

        public void InitGPIO_5_6()
        {
            var gpio = GpioController.GetDefault();

            _pin5 = gpio.OpenPin(5);
            _pin5.Write(GpioPinValue.Low);
            _pin5.SetDriveMode(GpioPinDriveMode.Output);

            _pin6 = gpio.OpenPin(6);
            _pin6.Write(GpioPinValue.Low);
            _pin6.SetDriveMode(GpioPinDriveMode.Output);
        }

        private Stopwatch _stopwatch = null;
        private BackgroundWorker _worker = null;

        private event EventHandler HighTick;
        private event EventHandler LowTick;

        public virtual void Start()
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
        


        public virtual void Stop()
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
        public virtual void Change(double t)
        {
            if (0.0 <= t && t < 0.2)
            {
                if (1000 != _current_interval)
                {
                    _current_interval = 1000;
                }
            }
            else if (0.2 <= t && t < 0.4)
            {
                if (770 != _current_interval)
                {
                    _current_interval = 750;
                }
            }
            else if (0.4 <= t && t < 0.6)
            {
                if (530 != _current_interval)
                {
                    _current_interval = 550;
                }
            }
            else if (0.6 <= t && t < 0.8)
            {
                if (290 != _current_interval)
                {
                    _current_interval = 325;
                }
            }
            else if (0.8 <= t && t <= 1.0)
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

        public int WritePin(int number)
        {
            GpioPin pin = null;

            if(5 == number)
            {
                pin = _pin5;
            }

            if(6 == number)
            {
                pin = _pin6;
            }

            if (null == pin) return -1;

            if(pin.Read() == GpioPinValue.High)
            {
                pin.Write(GpioPinValue.Low);
            }
            else
            {
                pin.Write(GpioPinValue.High);
            }

            if(pin.Read() == GpioPinValue.High)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public void WritePin(int number, GpioPinValue value)
        {
            GpioPin pin = null;

            if (5 == number)
            {
                pin = _pin5;
            }

            if (6 == number)
            {
                pin = _pin6;
            }

            if (null == pin) return;

            pin.Write(value);
        }

        protected void WritePin(GpioPinValue value)
        {
            if (null == _pin) return;
            if(_pin.Read() != value)
            {
                _pin.Write(value);
            }
        }

        public void WriteHigh()
        {
            WritePin(GpioPinValue.High);
        }

        public void WriteLow()
        {
            WritePin(GpioPinValue.Low);
        }
    }
}
