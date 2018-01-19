﻿using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
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
                _requestsMaker.SendFatRequestsToAll();
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
                _requestsMaker.SendFatRequestsToAll();
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
                _requestsMaker.SendFatRequestsToAll();
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
                _requestsMaker.SendFatRequestsToAll();
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
                _requestsMaker.SendFatRequestsToAll();
            }
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
