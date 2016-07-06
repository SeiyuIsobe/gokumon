using System;
using System.Collections.Generic;
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
        }

        private Timer _timer = null;

        public void Start()
        {
            _timer = new Timer(
                (sender) =>
                {
                    if (null == _changeAction)
                    {
                        if (_pinvalue == GpioPinValue.High)
                        {
                            _pinvalue = GpioPinValue.Low;
                            _pin.Write(_pinvalue);
                        }
                        else
                        {
                            _pinvalue = GpioPinValue.High;
                            _pin.Write(_pinvalue);
                        }

                        if (null != Tick)
                        {
                            Tick(null, null);
                        }
                    }
                    else
                    {
                        _changeAction();
                    }                    
                },
                null, 1000, 1000);
        }

        public void Start(int t)
        {
            _timer = new Timer(
                (sender) =>
                {
                    if(null != Tick)
                    {
                        Tick(null, null);
                    }
                },
                null, 0, t);
        }

        public void Change(int t)
        {
            _timer.Change(0, t);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private int _current_interval = 1000;

        private Action _changeAction = null;

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
                    _current_interval = 1000;
                    _changeAction = new Action(() =>
                    {
                        this.Change(_current_interval);
                        _changeAction = null;
                    });
                }
            }
            else if(0.2 <= t && t < 0.4)
            {
                if (770 != _current_interval)
                {
                    _current_interval = 770;
                    _changeAction = new Action(() =>
                    {
                        this.Change(_current_interval);
                        _changeAction = null;
                    });
                }
            }
            else if(0.4 <= t && t < 0.6)
            {
                if (530 != _current_interval)
                {
                    _current_interval = 530;
                    _changeAction = new Action(() =>
                    {
                        this.Change(_current_interval);
                        _changeAction = null;
                    });
                }
            }
            else if(0.6 <= t && t < 0.8)
            {
                if (290 != _current_interval)
                {
                    _current_interval = 290;
                    _changeAction = new Action(() =>
                    {
                        this.Change(_current_interval);
                        _changeAction = null;
                    });
                }
            }
            else if(0.8 <= t && t <= 1.0)
            {
                if (50 != _current_interval)
                {
                    _current_interval = 50;
                    _changeAction = new Action(() =>
                    {
                        this.Change(_current_interval);
                        _changeAction = null;
                    });
                }
            }
        }
    }
}
