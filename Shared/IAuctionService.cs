using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Shared {
  public interface IAuctionService : IService {
    Task<UserInfo> CreateUser(string mail);
    Task<UserInfo> GetUser(string mail);
    Task<string> OfferNewArticle(string sellerMail, string articleName, DateTime expiryDate, decimal amount);
    Task<ArticleInfo> GetArticle(string sellerMail, string articleId);
    Task<Bid[]> PlaceBid(string bidderMail, string sellerMail, string articleId, decimal amount);

    // only used internally
    // could be seperted into it's own interface
    Task<Bid[]> AddBidToArticle(string sellerMail, string articleId, Bid bid);
  }
}