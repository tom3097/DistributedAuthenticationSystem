using DistributedAuthSystem.Models;
using DistributedAuthSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("protected/neighbours")]
    public class NeighboursController : ApiController
    {
        #region fields

        private static readonly NeighboursRepository _repository = new NeighboursRepository();

        #endregion

        #region methods

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllNeighbours()
        {
            var neighbours = _repository.GetAllNeighbours();
            return Request.CreateResponse(HttpStatusCode.OK, neighbours);
        }

        [Route("{id:int}")]
        [HttpGet]
        public HttpResponseMessage GetSingleNeighbour([FromUri] int id)
        {
            var neighbour = _repository.GetSingleNeighbour(id);
            var statusCode = neighbour == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, neighbour);
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostNeighbour([FromBody] Neighbour neighbour)
        {
            var success = _repository.PostNeighbour(neighbour);
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.Conflict;
            return Request.CreateResponse(statusCode, neighbour);
        }

        [Route("{id:int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteNeighbour([FromUri] int id)
        {
            var success = _repository.DeleteNeighbour(id);
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/special")]
        [HttpPut]
        public HttpResponseMessage SetSpecialNeighbour([FromUri] int id, [FromBody] bool isSpecial)
        {
            var success = _repository.SetSpecialNeighbour(id, isSpecial);
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
