﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using ZappChat.Core.Socket.Requests;

namespace ZappChat.Core.Socket
{
    static class AppWebSocketEventManager
    {
        private static readonly WebSocket _webSocket;
        public static LoginWindow Login { get; set; }
        public static MainWindow MainWindow { get; set; }

        public static ConnectionStatus Connection { get; private set; }
        public static bool IsChat { get; set; }


        static AppWebSocketEventManager()
        {
            _webSocket = new WebSocket("ws://zappchat.ru:7778");
            _webSocket.Opened += OpenedConnection;
            _webSocket.Closed += ClosedConnection;
            _webSocket.Error += SocketErrorEvent;
            _webSocket.MessageReceived += MessageReceivedEvent;
            _webSocket.AutoSendPingInterval = 120;

            Connection = ConnectionStatus.Disconnect;
        }
        private static void MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var responseJson = (JObject)JsonConvert.DeserializeObject(e.Message);
                if(responseJson["action"] == null)
                    throw new Exception("JSON object don't containt action field!");
                switch ((string)responseJson["action"])
                {
                    case "client/auth":
                        HandlingAuthorizationResponce(responseJson);
                        break;
                    case "client/token":
                        HandlingTokenResponce(responseJson);
                        break;
                    case "client/pong":
                        HandlingPongResponce(responseJson);
                        break;
                        //@TODO
                }
            }
            catch (Exception exception)
            {
                //@TODO
                throw exception;
            }
        }

        private static void SocketErrorEvent(object sender, ErrorEventArgs e)
        {
            Connection = ConnectionStatus.Error;
        }

        private static void ClosedConnection(object sender, EventArgs e)
        {
            if(Connection == ConnectionStatus.Connect)
                _webSocket.Close();
            else 
                //@TODO - normal reaction on disconnect
                throw new Exception("Нет подключения!");
        }

        private static void OpenedConnection(object sender, EventArgs e)
        {
            Connection = ConnectionStatus.Connect;
            var token = FileDispetcher.GetSetting("token");
            if(token == null) return;
            var requestToken = new AuthorizationTokenRequest
            {
                token = token
            };
            var requestTokenToJson = JsonConvert.SerializeObject(requestToken);
            SendObject(requestTokenToJson);
        }

        public static void OpenWebSocket()
        {
            Connection = ConnectionStatus.Disconnect;
            try
            {
                if (_webSocket.State == WebSocketState.Closed || _webSocket.State == WebSocketState.None)
                    _webSocket.Open();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void SendObject(string jsonString)
        {
            _webSocket.Send(jsonString);
        }

        private static void HandlingAuthorizationResponce(JObject json)
        {
            CrossThreadOperationWithOneString(Login.Dispatcher, Login.AuthorizationResult,
                (string) json["status"]);
            if ((string) json["token"] != null) FileDispetcher.SaveSettings("token", (string) json["token"]);
        }

        private static void HandlingTokenResponce(JObject json)
        {
            switch ((string)json["status"])
            {
                case "ok":
                    CrossThreadOperationWithEventArrgs(Login.Dispatcher, AppEventManager.AuthorizationEvent,Login,"ok");
                    break;
                case "fail":
                    FileDispetcher.DeleteSetting("token");
                    break;
                case "error":
                    //@TODO what is it 0_o
                    break;
                default:
                    //@TODO Will do owner exсaption
                    throw new Exception("Ошибка на сервере!");
            }
        }

        private static void HandlingPongResponce(JObject responseJson)
        {
            if ((string) responseJson["status"] == "ok")
                App.PingRequestSuccses = true;
        }

        private static void CrossThreadOperationWithoutParams(Dispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        private static void CrossThreadOperationWithOneString(Dispatcher dispatcher, Action<string> action, string firstParam)
        {
            if (dispatcher.CheckAccess())
                action(firstParam);
            else
                dispatcher.Invoke(action, firstParam);
        }
        private static void CrossThreadOperationWithEventArrgs(Dispatcher dispatcher, Action<object,string> action,object sender, string firstParam)
        {
            if (dispatcher.CheckAccess())
                action(sender,firstParam);
            else
                dispatcher.Invoke(action, sender, firstParam);
        }
        
    }
}
