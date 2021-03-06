﻿using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Services;
using System.Net;
using System.Net.Http;
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

        private readonly IServerInfoRepository _serverInfoRepository;

        private readonly RequestsMaker _requestsMaker;

        #endregion

        #region methods

        public ClientsController(IClientsRepository clientsRepository, INeighboursRepository neighboursRepository,
            ISynchronizationsRepository synchronizationsRepository, IServerInfoRepository serverInfoRepository)
        {
            _clientsRepository = clientsRepository;
            _neighboursRepository = neighboursRepository;
            _synchronizationsRepository = synchronizationsRepository;
            _serverInfoRepository = serverInfoRepository;
            _requestsMaker = new RequestsMaker(_clientsRepository, _neighboursRepository,
                _synchronizationsRepository, _serverInfoRepository);
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllClients()
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            var clients = _clientsRepository.GetAllClients();
            return Request.CreateResponse(HttpStatusCode.OK, clients);
        }

        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetSingleClient([FromUri] string id)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            var client = _clientsRepository.GetSingleClient(id);
            var statusCode = client == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, client);
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostClient([FromBody] PostClientReq request)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            var success = _clientsRepository.PostClient(request.Id, request.Pin);
            if (success)
            {
                _requestsMaker.SendFatRequestsToAll();
            }
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClient([FromUri] string id, [FromBody] string pin)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            var success = _clientsRepository.DeleteClient(id, pin, out notFound);
            if (success)
            {
                _requestsMaker.SendFatRequestsToAll();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}")]
        [HttpPut]
        public HttpResponseMessage ChangeClientPin([FromUri] string id, [FromBody] ChangeClientPinReq request)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            var success = _clientsRepository.ChangeClientPin(id, request.CurrentPin, request.NewPin, out notFound);
            if (success)
            {
                _requestsMaker.SendFatRequestsToAll();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}/authenticate")]
        [HttpPost]
        public HttpResponseMessage AuthenticateClient([FromUri] string id, [FromBody] string pin)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            var success = _clientsRepository.AuthenticateClient(id, pin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}/passlist")]
        [HttpPost]
        public HttpResponseMessage GetClientPassList([FromUri] string id, [FromBody] string pin)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            var passwordList = _clientsRepository.GetClientPassList(id, pin, out notFound);
            var statusCode = passwordList == null ? notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, passwordList);
        }

        [Route("{id}/checkpass")]
        [HttpPost]
        public HttpResponseMessage CheckCurrentPassword([FromUri] string id, [FromBody] AuthPasswordReq request)
        {
            bool notFound;
            var success = _clientsRepository.CheckCurrentPassword(id, request.Pin,
                request.OneTimePassword, out notFound);

            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}/authorize")]
        [HttpPost]
        public HttpResponseMessage AuthorizeOperation([FromUri] string id, [FromBody] AuthPasswordReq request)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            HttpStatusCode statusCode;
            var canAuthorize = _clientsRepository.CheckCurrentPassword(id, request.Pin,
                request.OneTimePassword, out notFound);
            if (!canAuthorize)
            {
                statusCode = notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
                return Request.CreateResponse(statusCode);
            }
            canAuthorize = _requestsMaker.CheckCurrentPassword(id, request.Pin, request.OneTimePassword);
            if (!canAuthorize)
            {
                statusCode = notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
                return Request.CreateResponse(statusCode);
            }

            var success = _clientsRepository.AuthorizeOperation(id, request.Pin, request.OneTimePassword, out notFound);
            if (success)
            {
                _requestsMaker.SendFatRequestsToAll();
            }
            statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}/activatelist")]
        [HttpPost]
        public HttpResponseMessage ActivateNewPassList([FromUri] string id, [FromBody] AuthPasswordReq request)
        {
            if (_serverInfoRepository.GetServerState() != ServerState.IS_OK)
            {
                return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
            }

            bool notFound;
            HttpStatusCode statusCode;
            var canAuthorize = _clientsRepository.CheckCurrentPassword(id, request.Pin,
                request.OneTimePassword, out notFound);
            if (!canAuthorize)
            {
                statusCode = notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
                return Request.CreateResponse(statusCode);
            }
            canAuthorize = _requestsMaker.CheckCurrentPassword(id, request.Pin, request.OneTimePassword);
            if (!canAuthorize)
            {
                statusCode = notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
                return Request.CreateResponse(statusCode);
            }

            var success = _clientsRepository.ActivateNewPassList(id, request.Pin, request.OneTimePassword, out notFound);
            if (success)
            {
                _requestsMaker.SendFatRequestsToAll();
            }
            statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
