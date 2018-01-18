using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Responses;
using DistributedAuthSystem.Services;
using DistributedAuthSystem.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("public/clients")]
    public class ClientsController : ApiController
    {
        #region fields

        private readonly IClientsRepository _clientsRepository;

        private readonly INeighboursRepository _neighboursRepository;

        private readonly ISynchronizationsRepository _synchronizationsRepository;

        private readonly IServerInfoRepository _serverInfoRepository;

        private readonly JavaScriptSerializer _serializer;

        private const int _timeout = 30000;

        private const string _fatEndpoint = "private/synchro/fat";

        #endregion

        #region methods

        public ClientsController(IClientsRepository clientsRepository, INeighboursRepository neighboursRepository,
            ISynchronizationsRepository synchronizationsRepository, IServerInfoRepository serverInfoRepository)
        {
            _clientsRepository = clientsRepository;
            _neighboursRepository = neighboursRepository;
            _synchronizationsRepository = synchronizationsRepository;
            _serverInfoRepository = serverInfoRepository;
            _serializer = new JavaScriptSerializer();
        }

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

        private void FatCallback(IAsyncResult ar)
        {
            RequestState requestState = (RequestState)ar.AsyncState;
            WebRequest request = requestState.Request;
            WebResponse response = request.EndGetResponse(ar);

            FatSynchronizationRes fatResponse;

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                fatResponse = _serializer.Deserialize<FatSynchronizationRes>(responseString);
            }

            if (fatResponse.Type != SynchroResultType.CONFLICT)
            {
                _synchronizationsRepository.UpdateTime(fatResponse.SenderId, fatResponse.SynchroTimestamp);
                _synchronizationsRepository.UpdateSynchroTimes(fatResponse.SynchroTimes, fatResponse.SenderId,
                    fatResponse.SynchroTimestamp);
            }
        }

        private void SendFatRequest()
        {
            var neighbours = _neighboursRepository.GetAllNeighbours();
            foreach (var neigh in neighbours)
            {
                Dictionary<string, long> synchroTimesCopy;
                _synchronizationsRepository.GetSynchroTimesCopy(out synchroTimesCopy);

                var fatRequest = new FatSynchronizationReq
                {
                    SenderId = _serverInfoRepository.GetServerId(),
                    History = _clientsRepository.GetHistorySince(synchroTimesCopy[neigh.Id]),
                    SynchroTimes = synchroTimesCopy
                };

                var data = _serializer.Serialize(fatRequest);
                byte[] bytes = Encoding.UTF8.GetBytes(data);

                WebRequest wreq = WebRequest.Create(neigh.Url + _fatEndpoint);
                wreq.Method = "POST";
                wreq.ContentType = "application/json";
                wreq.ContentLength = bytes.Length;

                using (var streamWriter = new StreamWriter(wreq.GetRequestStream()))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var requestState = new RequestState
                {
                    Request = wreq
                };

                IAsyncResult result = wreq.BeginGetResponse(new AsyncCallback(FatCallback), requestState);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                    new WaitOrTimerCallback(TimeoutCallback), wreq, _timeout, true);
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

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostClient([FromBody] PostClientReq request)
        {
            var success = _clientsRepository.PostClient(request.Id, request.Pin);
            if (success)
            {
                SendFatRequest();
            }
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.Conflict;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClient([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var success = _clientsRepository.DeleteClient(id, pin, out notFound);
            if (success)
            {
                SendFatRequest();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpPut]
        public HttpResponseMessage ChangeClientPin([FromUri] int id, [FromBody] ChangeClientPinReq request)
        {
            bool notFound;
            var success = _clientsRepository.ChangeClientPin(id, request.CurrentPin, request.NewPin, out notFound);
            if (success)
            {
                SendFatRequest();
            }
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
        [HttpPut]
        public HttpResponseMessage AuthorizeOperation([FromUri] int id, [FromBody] AuthorizaOperationReq request)
        {
            bool notFound;
            var success = _clientsRepository.AuthorizeOperation(id, request.Pin, request.OneTimePassword, out notFound);
            if (success)
            {
                SendFatRequest();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/activatelist")]
        [HttpPut]
        public HttpResponseMessage ActivateNewPassList([FromUri] int id, [FromBody] ActivateNewPassListReq request)
        {
            bool notFound;
            var success = _clientsRepository.ActivateNewPassList(id, request.Pin, request.OneTimePassword, out notFound);
            if (success)
            {
                SendFatRequest();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
