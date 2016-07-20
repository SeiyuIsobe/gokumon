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
        private int _current_interval = -1;

        public event EventHandler HighTick;
        public event EventHandler LowTick;

        private int _pinnumber = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GpioPwm(int number)
        {
            _stopwatch = new Stopwatch();
            _pinnumber = number;
        }

        public void Initialize()
        {
            var gpio = GpioController.GetDefault();
            if (null != gpio)
            {
                _pin = gpio.OpenPin(_pinnumber);
                _pin.ValueChanged += (sender, e) =>
                {

                };
                _pin.Write(GpioPinValue.Low);
                _pin.SetDriveMode(GpioPinDriveMode.Output);

                LowTick(null, null);
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

                // スイッチが入っている間はずっと無限ループ
                while (true)
                {
                    // 間隔経過
                    if(_current_interval < 0)
                    {
                        if(true == _stopwatch.IsRunning)
                        {
                            _stopwatch.Stop();
                        }
                    }
                    else if (_current_interval <= _stopwatch.ElapsedMilliseconds)
                    {
                        _stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        // High
                        WriteHigh();

                        HighTick(null, null);
                        isHigh = true;
                    }
                    else if (20 <= _stopwatch.ElapsedMilliseconds)
                    {
                        if (true == isHigh)
                        {
                            // Low
                            WriteLow();

                            LowTick(null, null);
                            isHigh = false;
                        }
                    }
                }

            }));

            await task;
        }

        private int _min_level = 1000; // パルス幅：1000ミリ秒
        private int _max_level = 100; // パルス幅：100ミリ秒

        public override void Change(double t)
        {
            // 静止状態
            if(_current_interval < 0)
            {
                _current_interval = _min_level;
                _stopwatch.Start();
                return;
            }
            else
            {
                // スピード変化はcosカーブに従う
                double y = (_min_level - _max_level) * Math.Cos((Math.PI / 2.0) * t) + _max_level;
                _current_interval = (int)y;
            }
        }
    }
}
