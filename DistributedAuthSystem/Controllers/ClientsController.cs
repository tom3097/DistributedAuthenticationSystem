using DistributedAuthSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("public/clients")]
    public class ClientsController : ApiController
    {
        #region fields

        private static readonly ClientsRepository _repository = new ClientsRepository();

        #endregion

        #region methods

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllClients()
        {
            var clients = _repository.GetAllClients();
            return Request.CreateResponse(HttpStatusCode.OK, clients);
        }

        public HttpResponseMessage GetSingleClient([FromUri] int id)
        {
            var client = _repository.GetSingleClient(id);
            var statusCode = client == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, client);
        }

        #endregion
    }
}
