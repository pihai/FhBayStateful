using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Shared;

namespace AuctionService {
  /// <summary>
  /// An instance of this class is created for each service replica by the Service Fabric runtime.
  /// </summary>
  internal sealed class AuctionService : StatefulService, IAuctionService {
    private static readonly Uri ServiceUri = new Uri("fabric:/FhBayStateful/AuctionService");

    public AuctionService(StatefulServiceContext context)
      : base(context) {
    }

    /// <summary>
    /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
    /// </summary>
    /// <remarks>
    /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
    /// </remarks>
    /// <returns>A collection of listeners.</returns>
    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() {
      yield return new ServiceReplicaListener(this.CreateServiceRemotingListener);
    }

    public async Task<UserInfo> CreateUser(string mail) {
      var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("users");
      using (var tx = StateManager.CreateTransaction()) {
        try {
          var userInfo = new UserInfo(mail);
          await userDictionary.AddAsync(tx, mail, userInfo);
          await tx.CommitAsync();
          return userInfo;
        }
        catch (Exception ex) {
          throw new InvalidOperationException($"User '{mail}' already exists.", ex);
        }
      }
    }

    public async Task<UserInfo> GetUser(string mail) {
      var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("users");
      using (var tx = StateManager.CreateTransaction()) {
        var userInfo = await userDictionary.TryGetValueAsync(tx, mail);
        return userInfo.Value;
      }
    }

    public async Task<string> OfferNewArticle(string sellerMail, string itemName, DateTime expiryDate, decimal amount) {
      using (var tx = StateManager.CreateTransaction()) {
        await EnsureUserExists(sellerMail, tx);

        var userArticleDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, ArticleInfo>>("Articles-" + sellerMail);

        var article = new ArticleInfo(Guid.NewGuid().ToString("n"), itemName, expiryDate, amount);

        await userArticleDictionary.AddAsync(tx, article.Id, article);

        // TODO unexpired items

        await tx.CommitAsync();

        return article.Id;
      }
    }

    public async Task<ArticleInfo> GetArticle(string sellerMail, string articleId) {
      using (var tx = StateManager.CreateTransaction()) {
        await EnsureUserExists(sellerMail, tx);
        var userArticleDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, ArticleInfo>>("Articles-" + sellerMail);
        var result = await userArticleDictionary.TryGetValueAsync(tx, articleId);
        return result.Value;
      }
    }

    private async Task<UserInfo> EnsureUserExists(string mail, ITransaction tx) {
      var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("users");
      var userInfo = await userDictionary.TryGetValueAsync(tx, mail);

      if (!userInfo.HasValue) throw new UserDoesntExistException();
      //if (!userInfo.HasValue) throw new FaultException<UserDoesntExistFault>(new UserDoesntExistFault(mail), "You suck!");
      return userInfo.Value;
    }

    public async Task<Bid[]> PlaceBid(string bidderMail, string sellerMail, string articleId, decimal amount) {
      var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>("users");
      using (var tx = StateManager.CreateTransaction()) {
          // Ensure bidder exists
        var bidderUserInfo = await EnsureUserExists(bidderMail, tx);
          // Ensure seller exists
        await EnsureUserExists(sellerMail, tx);

        // Add item bidding on
        var updatedBidderInfo = bidderUserInfo.AddItemBidding(articleId);

        // reference comparison
        // if user wasn't already bidding on this article
        if (!updatedBidderInfo.Equals(bidderUserInfo)) {
          await userDictionary.SetAsync(tx, bidderMail, updatedBidderInfo);
        }

        await tx.CommitAsync();
      }

      // if the method fails here, the user thinks he is bidding on this item, but in actually is isn't.
      // Because he will not see his bid in the article page, he can bid again.

      try {
        var proxy = ServiceProxy.Create<IAuctionService>(ServiceUri, new ServicePartitionKey(sellerMail.GetHashCode()));
        return await proxy.AddBidToArticle(sellerMail, articleId, new Bid(bidderMail, amount, DateTime.Now));
      }
      catch (AggregateException e) {
        throw e.InnerException;
      }
    }

    public async Task<Bid[]> AddBidToArticle(string sellerMail, string articleId, Bid bid) {
      using (var tx = StateManager.CreateTransaction()) {
        var userArticleDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, ArticleInfo>>("Articles-" + sellerMail);
        var article = await userArticleDictionary.TryGetValueAsync(tx, articleId);
        if(!article.HasValue)
          throw new InvalidOperationException($"Article '{articleId}' doesn't exist.");

        if(bid.Date > article.Value.ExpiryDate)
          throw new InvalidOperationException("Auction has already ended.");

        var updatedArticle = article.Value.AddBid(bid);
        await userArticleDictionary.SetAsync(tx, articleId, updatedArticle);
        await tx.CommitAsync();
        return updatedArticle.Bids.ToArray();
      }
    }
  }
}
