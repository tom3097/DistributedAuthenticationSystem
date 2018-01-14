using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DistributedAuthSystem.Controllers
{
    [RoutePrefix("protected/neighbours")]
    public class NeighboursController : ApiController
    {
        #region fields

        private readonly INeighboursRepository _neighboursRepository;

        private readonly ISynchronizationsRepository _synchronizationsRepository;

        #endregion

        #region methods

        public NeighboursController(INeighboursRepository neighboursRepository,
            ISynchronizationsRepository synchronizationsRepository)
        {
            _neighboursRepository = neighboursRepository;
            _synchronizationsRepository = synchronizationsRepository;
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAllNeighbours()
        {
            var neighbours = _neighboursRepository.GetAllNeighbours();
            return Request.CreateResponse(HttpStatusCode.OK, neighbours);
        }

        [Route("{id}")]
        [HttpGet]
        public HttpResponseMessage GetSingleNeighbour([FromUri] string id)
        {
            var neighbour = _neighboursRepository.GetSingleNeighbour(id);
            var statusCode = neighbour == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, neighbour);
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostNeighbour([FromBody] PostNeighbourReq request)
        {
            var success = _neighboursRepository.PostNeighbour(request.Id, request.Url);
            if (success)
            {
                _synchronizationsRepository.RegisterServer(request.Id);
            }
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.Conflict;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteNeighbour([FromUri] string id)
        {
            var success = _neighboursRepository.DeleteNeighbour(id);
            if (success)
            {
                _synchronizationsRepository.UnregisterServer(id);
            }
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id}/special")]
        [HttpPut]
        public HttpResponseMessage SetSpecialNeighbour([FromUri] string id, [FromBody] bool isSpecial)
        {
            var success = _neighboursRepository.SetSpecialNeighbour(id, isSpecial);
            var statusCode = success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
