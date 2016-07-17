using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace ShimadzuGPIO
{
    public class GpioPwm : Master
    {
        private Stopwatch _stopwatch = null;
        private int _current_interval = 1000;

        public event EventHandler HighTick;
        public event EventHandler LowTick;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GpioPwm(int number)
        {
            _stopwatch = new Stopwatch();

            var gpio = GpioController.GetDefault();
            if(null != gpio)
            {
                _pin = gpio.OpenPin(number);
                _pin.Write(GpioPinValue.Low);
                _pin.SetDriveMode(GpioPinDriveMode.Output);
            }
            else
            {
                throw new Exception();
            }
        }

        async public override void Start()
        {
            Task task = Task.Run(new Action(() =>
            {
                _stopwatch.Start();

                bool isHigh = false;

                while (true)
                {
                    // 間隔経過
                    if (_current_interval <= _stopwatch.ElapsedMilliseconds)
                    {
                        _stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        HighTick(null, null);
                        isHigh = true;
                    }
                    else if (20 <= _stopwatch.ElapsedMilliseconds)
                    {
                        if (true == isHigh)
                        {
                            LowTick(null, null);
                            isHigh = false;
                        }
                    }

                    // Stop
                    if(true == _isStop)
                    {
                        _isStop = false;
                        _stopwatch.Stop();
                        break;
                    }
                }

            }));

            await task;
        }

        private bool _isStop = false;
    }
}
