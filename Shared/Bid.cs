using System;
using System.Runtime.Serialization;

namespace Shared {
  [DataContract]
  public class Bid {
    [DataMember] public readonly string BidderMail;
    [DataMember] public readonly decimal Amount;
    [DataMember] public readonly DateTime Date;

    public Bid(string bidderMail, decimal amount, DateTime date) {
      BidderMail = bidderMail;
      Amount = amount;
      Date = date;
    }
  }
}