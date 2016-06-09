using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace Shared {
  [DataContract]
  public class ArticleInfo {
    [DataMember] public readonly string Id;
    [DataMember] public readonly string Name;
    [DataMember] public readonly DateTime ExpiryDate;
    [DataMember] public readonly decimal StartPrice;
    [DataMember]
    public IEnumerable<Bid> Bids { get; private set; }

    public ArticleInfo(string id, string name, DateTime expiryDate, decimal startPrice) : 
      this(id, name, expiryDate, startPrice, ImmutableList<Bid>.Empty){
    }

    private ArticleInfo(string id, string name, DateTime expiryDate, decimal startPrice, ImmutableList<Bid> bids) {
      Id = id;
      Name = name;
      ExpiryDate = expiryDate;
      StartPrice = startPrice;
      Bids = bids;
    }

    public ArticleInfo AddBid(Bid bid) {
      return new ArticleInfo(Id, Name, ExpiryDate, StartPrice, ((ImmutableList<Bid>)Bids).Add(bid));
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context) {
      Bids = Bids.ToImmutableList();
    }
  }
}