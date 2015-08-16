using Agilix.Shared;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurboTank
{
    public interface TankClient
    {
        dynamic Start();
        dynamic TakeAction(TankAction move);
    }

    public class HttpTankClient : TankClient
    {
        private HttpClient client;
        public string GameId;
        public string PlayerId;
        public string userId;

        public HttpTankClient(string server, int port, string gameId, string userId)
        {
            this.userId = userId;
            this.GameId = gameId;
            this.client = new HttpClient(server, port);
        }

        public dynamic Start()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Sm-Playermoniker", userId);

            dynamic joinResponse = client.GetJsonResponse("/game/" + GameId + "/join", "POST", "", headers);
            PlayerId = headers["X-Sm-Playerid"];

            return joinResponse;
        }

        public dynamic TakeAction(TankAction move)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("X-Sm-Playerid", PlayerId);

            return client.GetJsonResponse("/game/" + GameId + "/" + move.ToString().ToLower(), "POST", "", headers);
        }

    }

    public class TestTankClient : TankClient
    {
        private DynObject startResponse;
        private DynObject takeActionResponse;
        private TankAction expectedAction;

        public TestTankClient(DynObject startResponse, TankAction expectedAction, DynObject takeActionResponse = null)
        {
            this.expectedAction = expectedAction;
            this.startResponse = startResponse;

            if (takeActionResponse == null)
            {
                takeActionResponse = DynObject.Parse(@"{
                    'status': 'won',
                    'health': 200,
                    'energy': 10,
                    'orientation': 'north',
                    'grid': '________________________
___W_____WWWWWWWW_______
___W_W__________________
___W_W_______B__________
___W_W__________________
___W_W__________________
_WWWWWWWWW___L____O_____
_____W__________________
_____W_WWWWW____________
_________WWWWWWWW_______
________________________
___________WWWW_________
__X_____________________
________________________
____WWW_________________
________________________'
                }");
            }

            this.takeActionResponse = takeActionResponse;
        }

        public dynamic Start()
        {
            return startResponse;
        }

        public dynamic TakeAction(TankAction move)
        {
            if ((move & expectedAction) == 0) throw new Exception("Got: " + move + ", expected: " + expectedAction);

            return takeActionResponse;
        }
    }
}
