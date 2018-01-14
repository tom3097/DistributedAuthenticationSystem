using DistributedAuthSystem.Requests;
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

        #endregion

        #region methods

        public SynchroController(ISynchronizationsRepository synchronizationRepository,
            IClientsRepository clientsRepository)
        {
            _synchronizationsRepository = synchronizationRepository;
            _clientsRepository = clientsRepository;
        }

        [Route("fat")]
        [HttpPost]
        public HttpResponseMessage FatSynchronization([FromBody] FatSynchronizationReq request)
        {
            return Request.CreateResponse(HttpStatusCode.ServiceUnavailable);
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
