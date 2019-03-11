using NUnit.Framework;
using AutoPrintr.Core.Models;
using Newtonsoft.Json;
using PusherClient;

namespace AutoPrintr.Tests
{

    [TestFixture]
    public class JobServiceTests
    {
        private PusherClient.Pusher _pusherClient;
        private PusherServer.Pusher _pusherServer;
        public dynamic message = null;
        public void _pusher_ReadResponse(dynamic message)
        {
            this.message = message;
            return;
        }

        private void _pusher_Error(object sender, PusherException error)
        {
            return;
        }

        private void _pusher_ConnectionStateChanged(object sender, ConnectionState state)
        {
            return;
        }

        [Test]
        public void CreateNewJob()
        {
            //To test with the service you will need to configure the below settings.
            //You also need to set the pusherApplicationKey in AppSettings.cs of the AutoPrintr.Core project and ensure that the code is using the same channel as defined below
            var pusherApplicationKey = "";
            var appId = "";
            var appSecret = "";
            var appChannel = "";
            var appEventName = "";

            if (pusherApplicationKey == "" || appId == "" || appSecret == "" || appChannel == "" || appEventName == "")
            {
                Assert.Fail("Pusher application configuration required for test to function.");
            }

            //Note that your app must be in zone mt1 on Pusher.com or you will need to specify the 2nd options argument
            _pusherClient = new PusherClient.Pusher(pusherApplicationKey);//, clientOptions);
            _pusherClient.Error += _pusher_Error;
            _pusherClient.ConnectionStateChanged += _pusher_ConnectionStateChanged;
            _pusherClient.Subscribe(appChannel).Bind(appEventName, this._pusher_ReadResponse);
            _pusherClient.Connect();

            int waitCount = 0;
            while (_pusherClient.State != PusherClient.ConnectionState.Connected)
            {
                //Wait for client to connect
                System.Threading.Thread.Sleep(300);
                waitCount++;
                if (waitCount > 10)
                {
                    Assert.Fail("Error connecting pusher client.");
                }
            }

            //Your app must be in zone mt1 on Pusher.com or you will need to specify the 4th options argument
            _pusherServer = new PusherServer.Pusher(
                appId,
                pusherApplicationKey,
                appSecret
              );

            var document = new Document();
            var message = JsonConvert.SerializeObject(document);

            _pusherServer.TriggerAsync(
              appChannel,
              appEventName,
              new { message });

            waitCount = 0;
            while (this.message == null)
            {
                System.Threading.Thread.Sleep(300);
                waitCount++;
                if (waitCount > 10)
                {
                    Assert.Fail("Error receiving message from server.");
                }
            }

            return;
        }
    }
}
