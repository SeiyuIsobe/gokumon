using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace DzuoolIoT.DataModels
{
    public class BaseIoT
    {
        protected MessageWebSocket _ws = null;
        private string wsUri = "ws://sukekiyo.mybluemix.net/ws/accera";
        protected const int _messageBufferSize = 4096;
        protected DataWriter _messageWriter = null;

        public string WsUri
        {
            get
            {
                return wsUri;
            }

            set
            {
                wsUri = value;
            }
        }

        protected BaseIoT()
        {
            if (_ws == null)
            {
                _ws = new MessageWebSocket();
                _ws.Control.MessageType = SocketMessageType.Utf8;
            }
        }

        protected async void Connect()
        {
            try
            {
                await _ws.ConnectAsync(new Uri(WsUri));
            }
            catch (Exception ex) // For debugging
            {
                // Error happened during connect operation.
                _ws.Dispose();
                _ws = null;

                return;
            }

            _messageWriter = new DataWriter(_ws.OutputStream);
        }

        protected void Disconnect()
        {
            this.CloseSocket();
        }

        private void CloseSocket()
        {
            if (_messageWriter != null)
            {
                // In order to reuse the socket with another DataWriter, the socket's output stream needs to be detached.
                // Otherwise, the DataWriter's destructor will automatically close the stream and all subsequent I/O operations
                // invoked on the socket's output stream will fail with ObjectDisposedException.
                //
                // This is only added for completeness, as this sample closes the socket in the very next code block.
                _messageWriter.DetachStream();
                _messageWriter.Dispose();
                _messageWriter = null;
            }

            if (_ws != null)
            {
                try
                {
                    _ws.Close(1000, "Closed due to user request.");
                }
                catch (Exception ex)
                {}
                _ws = null;
            }
        }
    }
}
