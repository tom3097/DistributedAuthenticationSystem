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

        private readonly IClientsRepository _repository;

        #endregion

        #region methods

        public SynchroController(IClientsRepository repository)
        {
            _repository = repository;
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
