using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using DzuoolIoT.DataModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShimadzuIoT
{
    public class AccelerIoT : BaseIoT, INotifyPropertyChanged
    {
        

        public event PropertyChangedEventHandler PropertyChanged;
        private event EventHandler TryConnect;

        public AccelerIoT() : base()
        {
            _ws.MessageReceived += MessageReceived;
            _ws.Closed += MessageClosed;

            TryConnect += (ss, ee) =>
            {
                base.Connect();
            };
        }

        public void Start()
        {
            base.Connect();
        }

        public void End()
        {
            base.Disconnect();
        }

        private void MessageClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = 0; // UTF8は0

                    try
                    {
                        string read = reader.ReadString(reader.UnconsumedBufferLength);
                        this.Acceler = Acceler.FromJson(read);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch
            {
                if (null != TryConnect)
                {
                    _ws.Dispose();
                    _ws = null;

                    TryConnect(null, null);
                }
            }
            

        }

        private Acceler _acceler = null;

        public Acceler Acceler
        {
            get
            {
                return _acceler;
            }

            set
            {
                _acceler = value;

                if(null != _acceler)
                {
                    this.NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
