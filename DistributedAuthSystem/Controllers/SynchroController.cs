using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Responses;
using DistributedAuthSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("private/synchro")]
    public class SynchroController : ApiController
    {
        #region fields

        private readonly ISynchronizationsRepository _synchronizationsRepository;

        private readonly IClientsRepository _clientsRepository;

        private readonly IServerInfoRepository _serverInfoRepository;

        #endregion

        #region methods

        public SynchroController(ISynchronizationsRepository synchronizationRepository,
            IClientsRepository clientsRepository, IServerInfoRepository serverInfoRepository)
        {
            _synchronizationsRepository = synchronizationRepository;
            _clientsRepository = clientsRepository;
            _serverInfoRepository = serverInfoRepository;
        }

        [Route("fat")]
        [HttpPost]
        public HttpResponseMessage FatSynchronization([FromBody] FatSynchronizationReq request)
        {
            var success = _clientsRepository.UpdateHistory(request.History);
            var statusCode = success == SynchroResultType.OK ? HttpStatusCode.OK : HttpStatusCode.Forbidden;

            var fatResponse = new FatSynchronizationRes
            {
                Type = success,
                SynchroTimestamp = request.History.Last().Timestamp,
                SenderId = _serverInfoRepository.GetServerId()
            };

            return Request.CreateResponse(HttpStatusCode.OK, fatResponse);
        }

        [Route("thin")]
        [HttpPost]
        public HttpResponseMessage ThinSynchronization([FromBody] ThinSynchronizationReq request)
        {
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
        }

        #endregion
    }
}
