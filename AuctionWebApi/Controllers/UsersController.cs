using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Shared;

namespace AuctionWebApi.Controllers {

  public class BaseController : ApiController {
    protected static readonly Uri ServiceUri = new Uri("fabric:/FhBayStateful/AuctionService");

    protected static IAuctionService CreateServiceProxy(string mail) {
      return ServiceProxy.Create<IAuctionService>(ServiceUri, new ServicePartitionKey(mail.GetHashCode()));
    }
  }

  public class UsersController : BaseController {
    [HttpPost]
    [Route("api/users/{mail}")]
    public async Task<HttpResponseMessage> Post(string mail) {
      var srv = CreateServiceProxy(mail);
      try {
        var result = await srv.CreateUser(mail);
        return Request.CreateResponse(HttpStatusCode.Created);
      }
      catch (AggregateException e) when (e.InnerException is InvalidOperationException) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, e.InnerException.Message);
      }
      catch (Exception ex) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "User (may) already exists.");
      }
    }


    [HttpGet]
    [ResponseType(typeof(UserInfo))]
    [Route("api/users/{mail}")]
    public async Task<HttpResponseMessage> Get(HttpRequestMessage request, string mail) {
      var srv = CreateServiceProxy(mail);
      var result = await srv.GetUser(mail);

      return result == null ? 
        new HttpResponseMessage(HttpStatusCode.NotFound) : 
        request.CreateResponse(HttpStatusCode.OK, result);
    }
  }

  public class BidsController : BaseController {
    [HttpPost]
    [Route("api/articles/{sellerMail}/{articleId}/bids")]
    public async Task<HttpResponseMessage> Post(string sellerMail, string articleId, string bidderMail, decimal amount) {
      var srv = CreateServiceProxy(sellerMail);
      try {
        var result = await srv.PlaceBid(bidderMail, sellerMail, articleId, amount);
        return Request.CreateResponse(HttpStatusCode.Created, result);
      }
      catch (AggregateException ex) when (ex.InnerException is UserDoesntExistException) {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException.Message);
      }
      catch (AggregateException ex) when (ex.InnerException is FaultException<UserDoesntExistFault>) {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException);
      }
      catch (AggregateException ex) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.InnerException.Message);
      }
      catch (Exception ex) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message);
      }
    }
  }

  public class ArticlesController : BaseController {

    [ResponseType(typeof(ArticleInfo))]
    [HttpGet]
    public async Task<HttpResponseMessage> Get(HttpRequestMessage request, string sellerMail, string articleId) {
      var srv = CreateServiceProxy(sellerMail);
      try {
        var articleInfo = await srv.GetArticle(sellerMail, articleId);
        return articleInfo != null
          ? Request.CreateResponse(HttpStatusCode.OK, articleInfo)
          : Request.CreateResponse(HttpStatusCode.NotFound);
      }
      catch (AggregateException ex) when (ex.InnerException is UserDoesntExistException) {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException.Message);
      }
      catch (AggregateException ex) when (ex.InnerException is FaultException<UserDoesntExistFault>) {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException);
      }
      catch (Exception ex) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, ex.Message);
      }
    }

    [HttpPost]
    public async Task<HttpResponseMessage> Post(HttpRequestMessage request, string sellerMail, string articleName, DateTime expiryDate, decimal amount) {
      var srv = CreateServiceProxy(sellerMail);

      try {
        var result = await srv.OfferNewArticle(sellerMail, articleName, expiryDate, amount);
        var response = request.CreateResponse(HttpStatusCode.Created, result);
        return response;
      }
      catch (AggregateException ex) when (ex.InnerException is UserDoesntExistException){
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException.Message);
      }
      catch (AggregateException ex) when (ex.InnerException is FaultException<UserDoesntExistFault>){
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.InnerException);
      }
      catch (Exception) {
        return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Something unexpected happened. Try again.");
      }
    }
  }
}
