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

        [Route("{id:int}")]
        [HttpGet]
        public HttpResponseMessage GetSingleClient([FromUri] int id)
        {
            var client = _repository.GetSingleClient(id);
            var statusCode = client == null ? HttpStatusCode.NotFound : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, client);
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage PostClient([FromBody] PostClientReq request)
        {
            var success = _repository.PostClient(request.Id, request.Pin);
            var statusCode = success ? HttpStatusCode.Created : HttpStatusCode.Conflict;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteClient([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var success = _repository.DeleteClient(id, pin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}")]
        [HttpPut]
        public HttpResponseMessage ChangeClientPin([FromUri] int id, [FromBody] ChangeClientPinReq request)
        {
            bool notFound;
            var success = _repository.ChangeClientPin(id, request.CurrentPin, request.NewPin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/authenticate")]
        [HttpGet]
        public HttpResponseMessage AuthenticateClient([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var success = _repository.AuthenticateClient(id, pin, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/passlist")]
        [HttpGet]
        public HttpResponseMessage GetClientPassList([FromUri] int id, [FromBody] int pin)
        {
            bool notFound;
            var passwordList = _repository.GetClientPassList(id, pin, out notFound);
            var statusCode = passwordList == null ? notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized : HttpStatusCode.OK;
            return Request.CreateResponse(statusCode, passwordList);
        }

        [Route("{id:int}/authorize")]
        [HttpGet]
        public HttpResponseMessage AuthorizeOperation([FromUri] int id, [FromBody] AuthorizaOperationReq request)
        {
            bool notFound;
            var success = _repository.AuthorizeOperation(id, request.Pin, request.OneTimePassword, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        [Route("{id:int}/activatelist")]
        [HttpGet]
        public HttpResponseMessage ActivateNewPassList([FromUri] int id, [FromBody] ActivateNewPassListReq request)
        {
            bool notFound;
            var success = _repository.ActivateNewPassList(id, request.Pin, request.OneTimePassword, out notFound);
            var statusCode = success ? HttpStatusCode.OK : notFound ? HttpStatusCode.NotFound : HttpStatusCode.Unauthorized;
            return Request.CreateResponse(statusCode);
        }

        #endregion
    }
}
