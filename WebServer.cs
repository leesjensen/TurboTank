using Agilix.Shared;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;

namespace TurboTank
{
    public class WebServer : IRunnableClass
    {
        private int port;
        private bool isSecureListener = false;

        private EventWaitHandle shutdownEvent = new AutoResetEvent(false);
        private HttpListener listener = new HttpListener();
        private List<RequestHandler> handlers = new List<RequestHandler>();

        private Dictionary<string, string> sessions = new Dictionary<string, string>();


        public WebServer(DynObject config)
        {
            port = (int)JsonUtil.Get(config, "port", 50000);
            listener = StartListener(string.Format("http://+:{0}/", port));
            SetHandlers(this);
        }


        public int Port
        {
            get { return port; }
        }


        private void SetHandlers(object handlerClass)
        {
            foreach (MethodInfo methodInfo in handlerClass.GetType().GetMethods())
            {
                foreach (Docs docAttribute in methodInfo.GetCustomAttributes(typeof(Docs)))
                {
                    WebServerDelegate requestHandlerMethod = (WebServerDelegate)methodInfo.CreateDelegate(typeof(WebServerDelegate), handlerClass);
                    handlers.Add(new RequestHandler(docAttribute, requestHandlerMethod));
                }
            }

            this.handlers.Add(new RequestHandler(GetDocs));
        }


        [Docs(
            name = "Get Root Node",
            description = "Gets the root node of the system.",
            method = "GET",
            requestPattern = "/",
            isInternal = true
            )]
        public void GetRootContent(WebServerRequest request)
        {
            request.ResponseFilename = "content/main.html";
        }


        [Docs(
            name = "Get HTML Documentation",
            description = "Gets the documentation.",
            method = "GET",
            requestPattern = "/docs.html",
            isInternal = true
            )]
        public void GetHtmlDocs(WebServerRequest request)
        {
            request.ResponseFilename = "content/docs.html";
        }


        [Docs(
            name = "Get Favicon",
            description = "Gets the favorite icon.",
            method = "GET",
            requestPattern = "/favicon.ico",
            isInternal = true
            )]
        public void GetFavIcon(WebServerRequest request)
        {
            request.ContentType = "image/png";
            request.ResponseFilename = "content/favicon.png";
        }


        [Docs(
            name = "Get Content",
            description = "Gets content files.",
            method = "GET",
            requestPattern = "/content/{path:.*}",
            isInternal = true
            )]
        public void GetContent(WebServerRequest request)
        {
            request.ResponseFilename = "content/" + request.GetParam("path");
        }


        [Docs(
            name = "Get Documentation",
            description = "Gets the documentation.",
            method = "GET",
            requestPattern = "/docs",
            requestExample = "/docs",
            isInternal = true
            )]
        public void GetDocs(WebServerRequest request)
        {
            DynArray result = new DynArray();

            foreach (RequestHandler handler in handlers)
            {
                if (!handler.Documentation.isInternal)
                {
                    result.Add(handler.Documentation.Serialize());
                }
            }

            request.ResponseText = result.ToString();
        }

        public enum Move
        {
            Left,
            Right,
            Fire,
            Move,
            Noop
        }

        public enum Orientation
        {
            North,
            East,
            South,
            West
        }

        public enum Status
        {
            Running,
            Won,
            Lost,
            Draw
        }

        public class Position
        {
            public int X;
            public int Y;
        }

        public class Game
        {
            public string GameId;
            public string PlayerId;

            private HttpClient client;
            private DynObject config;

            private Status status;
            private int health;
            private int energy;
            private Orientation orientation;
            private char[,] grid = new char[24, 16];

            public Position MyPostion = new Position();

            public override string ToString()
            {
                return DynObject.FromPairs("gameId", GameId, "playerId", PlayerId, "status", status).ToString();
            }

            //max_energy;
            //laser_distance
            //health_loss
            //battery_power
            //battery_health
            //laser_energy
            //connect_back_timeout
            //max_health
            //laser_damage
            //turn_timeout


            public Game(string server, int port, string gameId)
            {
                this.GameId = gameId;
                this.client = new HttpClient(server, port);

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("X-Sm-Playermoniker", "Lee");

                dynamic joinResponse = client.GetJsonResponse("/game/" + gameId + ":screen/join", "POST", "", headers);
                ParseStatus(joinResponse);

                PlayerId = headers["X-Sm-Playerid"];
                config = joinResponse.config;
            }

            public bool IsRunning()
            {
                return (status == Status.Running);
            }

            public void Move(Move move)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("X-Sm-Playerid", PlayerId);

                dynamic moveResponse = client.GetJsonResponse("/game/" + GameId + ":screen/" + move.ToString().ToLower(), "POST", "", headers);

                ParseStatus(moveResponse);
            }

            private void ParseStatus(dynamic moveResponse)
            {
                Enum.TryParse(moveResponse.status, true, out status);
                Enum.TryParse(moveResponse.orientation, true, out orientation);
                health = moveResponse.health;
                energy = moveResponse.energy;

                int gridY = 0;
                foreach (string gridLine in moveResponse.grid.Split('\n'))
                {
                    int gridX = 0;
                    foreach (char gridChar in gridLine)
                    {
                        grid[gridX, gridY] = gridChar;

                        if (gridChar == 'X')
                        {
                            MyPostion.X = gridX;
                            MyPostion.Y = gridY;
                        }

                        gridX++;
                    }
                    gridY++;
                }
            }
        }


        [Docs(
            name = "Start Game",
            description = "Starts a new game by connected with the Tank server with the provided game ID.",
            method = "POST",
            requestExample = "/game/tankyou?server=127.0.0.1&port=8080",
            requestPattern = "/game/{gameId}"
            )]
        public void StartGame(WebServerRequest request)
        {
            string server = ((string)request.GetParam("server") ?? "127.0.0.1");
            if (!int.TryParse(request.GetParam("port"), out port)) { port = 8080; }
            string gameId = request.GetParam("gameId");

            Game game = new Game(server, port, gameId);

            ThreadUtil.RunWorkerThread((state) =>
            {
                while (game.IsRunning())
                {
                    game.Move(Move.Left);
                }
            });

            request.ResponseText = game.ToString();
        }




        public void Run()
        {
            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((workItem) =>
                        {
                            var listenerContext = workItem as HttpListenerContext;
                            try
                            {
                                if (!isSecureListener || (isSecureListener && listenerContext.Request.IsSecureConnection))
                                {
                                    ProcessRequest(listenerContext.Request, listenerContext.Response);
                                }
                                else
                                {
                                    RedirectToSecureConnection(listenerContext);
                                }
                            }
                            catch { }
                            finally
                            {
                                listenerContext.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch { }
            });

            shutdownEvent.WaitOne();
        }


        public void RunAsync()
        {
            Thread thread = new Thread(Run);
            thread.Start();
        }


        public void Stop()
        {
            listener.Stop();
            listener.Close();
            shutdownEvent.Set();
        }


        private static void RedirectToSecureConnection(HttpListenerContext listenerContext)
        {
            listenerContext.Response.StatusCode = (int)HttpStatusCode.Redirect;
            listenerContext.Response.RedirectLocation = string.Format("https://{0}", listenerContext.Request.Url.Host);
        }


        private static HttpListener StartListener(params string[] prefixes)
        {
            HttpListener listener = new HttpListener();
            foreach (string prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }
            listener.Start();

            return listener;
        }


        private void ProcessRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] responseBuffer = null;

            response.Headers["Access-Control-Allow-Origin"] = "*";

            DynObject logEntry = new DynObject();
            logEntry["action"] = "reqEnd";
            logEntry["httpMethod"] = request.HttpMethod;
            logEntry["url"] = request.RawUrl;

            Stopwatch requestTimer = Stopwatch.StartNew();

            try
            {
                WebServerRequest webRequest = CallHandler(request, response, logEntry);
                if (webRequest != null)
                {
                    responseBuffer = ProcessResponse(response, webRequest);
                }
                else
                {
                    throw new HttpException(HttpStatusCode.NotFound, "Unknown request made [{0}] {1}", request.HttpMethod, request.RawUrl);
                }
            }
            catch (Exception exception)
            {
                responseBuffer = ProcessError(response, exception, logEntry);
            }
            finally
            {
                logEntry["responseTime"] = (double)requestTimer.ElapsedMilliseconds;
                logEntry["httpStatus"] = response.StatusCode;

                if (logEntry["level"] == null) Logger.Info(logEntry); else Logger.Error(logEntry);
            }

            if (responseBuffer != null)
            {
                response.ContentLength64 = responseBuffer.Length;
                response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
            }
        }


        private static byte[] ProcessError(HttpListenerResponse response, Exception exception, DynObject logEntry)
        {
            response.ContentType = "application/json";
            response.StatusCode = (int)(exception is HttpException ? ((HttpException)exception).StatusCode : HttpStatusCode.InternalServerError);

            logEntry["level"] = "Error";
            logEntry["message"] = exception.Message;
            logEntry["stack"] = exception.StackTrace;

            return IOUtil.Utf8Encoding.GetBytes(logEntry.ToString());
        }


        private WebServerRequest CallHandler(HttpListenerRequest request, HttpListenerResponse response, DynObject logEntry)
        {
            WebServerRequest webRequest = null;

            foreach (RequestHandler handler in handlers)
            {
                if (handler.MethodRegex.IsMatch(request.HttpMethod))
                {
                    var match = handler.ResourceRegex.Match(request.Url.AbsolutePath);
                    if (match.Success)
                    {
                        logEntry["cmd"] = handler.Documentation.name;

                        var parameters = new Dictionary<string, string>();
                        foreach (string groupName in handler.ResourceRegex.GetGroupNames())
                        {
                            parameters.Add(groupName, match.Groups[groupName].Value);
                        }

                        webRequest = new WebServerRequest(parameters, request, response, logEntry);

                        handler.Callback(webRequest);
                        break;
                    }
                }
            }

            return webRequest;
        }

        private static byte[] ProcessResponse(HttpListenerResponse response, WebServerRequest webRequest)
        {
            byte[] buffer = null;

            response.ContentType = webRequest.ContentType;
            response.StatusCode = (int)webRequest.StatusCode;

            if (webRequest.ResponseText != null)
            {
                buffer = IOUtil.Utf8Encoding.GetBytes(webRequest.ResponseText);
            }
            else if (webRequest.ResponseFilename != null)
            {
                buffer = File.ReadAllBytes(Path.Combine("./", webRequest.ResponseFilename));
            }

            return buffer;
        }

    }

}
