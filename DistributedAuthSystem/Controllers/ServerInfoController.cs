using DistributedAuthSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("server")]
    public class ServerInfoController : ApiController
    {
        #region fields

        private readonly IServerInfoRepository _serverInfoRepository;

        #endregion

        #region methods

        public ServerInfoController(IServerInfoRepository serverInfoRepository)
        {
            _serverInfoRepository = serverInfoRepository;
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetServerId()
        {
            var serverId = _serverInfoRepository.GetServerId();
            return Request.CreateResponse(HttpStatusCode.OK, serverId);
        }

        [Route("{id}")]
        [HttpGet] /* tylko tymczasowo GET - normalnie PUT */
        public HttpResponseMessage PutServerId([FromUri] string id)
        {
            var success = _serverInfoRepository.PutServerId(id);
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.MethodNotAllowed;
            return Request.CreateResponse(success);
        }

        #endregion
    }
}
