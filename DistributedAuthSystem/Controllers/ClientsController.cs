using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("public/clients")]
    public class ClientsController : ApiController
    {
        #region fields

        private readonly IClientsRepository _clientsRepository;

        private readonly INeighboursRepository _neighboursRepository;

        private readonly ISynchronizationsRepository _synchronizationsRepository;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private const int DefaultTimeout = 1 * 30 * 1000; // 30 sekund

        #endregion

        #region methods

        public ClientsController(IClientsRepository clientsRepository, INeighboursRepository neighboursRepository,
            ISynchronizationsRepository synchronizationsRepository)
        {
            _clientsRepository = clientsRepository;
            _neighboursRepository = neighboursRepository;
            _synchronizationsRepository = synchronizationsRepository;
        }

        // Abort the request if the timer fires.
        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllClients()
        {
            var clients = _clientsRepository.GetAllClients();
            return Request.CreateResponse(HttpStatusCode.OK, clients);
        }

        [Route("{id:int}")]
        [HttpGet]
        public HttpResponseMessage GetSingleClient([FromUri] int id)
        {
            var client = _clientsRepository.GetSingleClient(id);
            var statusCode = client == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, client);
        }

        private static void RespCallback(IAsyncResult ar)
        {
            int a = 4;
            WebRequest myWebRequest = (WebRequest)ar.AsyncState;
            WebResponse resp = myWebRequest.EndGetResponse(ar);

            using (Stream stream = resp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
            }

        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostClient([FromBody] PostClientReq request)
        {
            var success = _clientsRepository.PostClient(request.Id, request.Pin);
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.Conflict;
            if (success)
            {
                var neighbours = _neighboursRepository.GetAllNeighbours();
                foreach (var neigh in neighbours)
                {
                    Dictionary<int, long> synchroTimesCopy;
                    _synchronizationsRepository.GetSynchroTimesCopy(out synchroTimesCopy);
                    var history = _clientsRepository.GetHistorySince(synchroTimesCopy[neigh.Id]);
                    int senderId = 345;

                    var fatRequest = new FatSynchronizationReq
                    {
                        SenderId = senderId,
                        History = history,
                        SynchroTimes = synchroTimesCopy
                    };

                    // tu normalnie wysylanie na fat request
                    WebRequest wreq = WebRequest.Create(neigh.Url + "public/clients");
                    IAsyncResult result = wreq.BeginGetResponse(new AsyncCallback(RespCallback), wreq);

                    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), wreq, DefaultTimeout, true);

                    //dokmyslnie GET
                    //wreq.Method =
                    /* request przygotowany, teraz tylko wyslac */
                }
                // wysylamy wiadomosc do innych (fat requesta)
                // synchronizedTo = _synchronizationRepo.get(serwerId);
                // clientRepo.GetOperations(from=synchronizedTo);
                // prepare FatRequest
                // sendFatRequestAsynchronous
            }
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClient([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var success = _clientsRepository.DeleteClient(id, pin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpPut]
        public HttpResponseMessage ChangeClientPin([FromUri] int id, [FromBody] ChangeClientPinReq request)
        {
            bool notFound;
            var success = _clientsRepository.ChangeClientPin(id, request.CurrentPin, request.NewPin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/authenticate")]
        [HttpGet]
        public HttpResponseMessage AuthenticateClient([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var success = _clientsRepository.AuthenticateClient(id, pin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/passlist")]
        [HttpGet]
        public HttpResponseMessage GetClientPassList([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var passwordList = _clientsRepository.GetClientPassList(id, pin, out notFound);
            var statusCode = passwordList == null ? notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, passwordList);
        }

        [Route("{id:int}/authorize")]
        [HttpGet]
        public HttpResponseMessage AuthorizeOperation([FromUri] int id, [FromBody] AuthorizaOperationReq request)
        {
            bool notFound;
            var success = _clientsRepository.AuthorizeOperation(id, request.Pin, request.OneTimePassword, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/activatelist")]
        [HttpGet]
        public HttpResponseMessage ActivateNewPassList([FromUri] int id, [FromBody] ActivateNewPassListReq request)
        {
            bool notFound;
            var success = _clientsRepository.ActivateNewPassList(id, request.Pin, request.OneTimePassword, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
