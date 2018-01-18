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
            long maxSyncTime;
            var result = _clientsRepository.UpdateHistory(request.History, out maxSyncTime);
            if (result == SynchroResultType.FIXED)
            {
                _synchronizationsRepository.FixSynchroTimes(maxSyncTime);
            }

            if (result != SynchroResultType.CONFLICT)
            {
                _synchronizationsRepository.UpdateSynchroTimes(request.SynchroTimes, request.SenderId,
                    request.History.Last().Timestamp);
            }

            var statusCode = result == SynchroResultType.CONFLICT || result == SynchroResultType.U2OLD ?
                HttpStatusCode.SeeOther : HttpStatusCode.OK;

            Dictionary<string, long> synchroTimesCopy;
            _synchronizationsRepository.GetSynchroTimesCopy(out synchroTimesCopy);

            var fatResponse = new FatSynchronizationRes
            {
                Type = result,
                SynchroTimestamp = request.History.Last().Timestamp,
                SenderId = _serverInfoRepository.GetServerId(),
                SynchroTimes = synchroTimesCopy
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
