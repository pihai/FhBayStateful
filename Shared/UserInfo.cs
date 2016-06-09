using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Shared {
  [DataContract]
  public class UserInfo {
    [DataMember] public readonly string Mail;

    [DataMember]
    public IEnumerable<string> ItemsBidding { get; private set; }

    public UserInfo(string mail) : this(mail, ImmutableList<string>.Empty) {}

    private UserInfo(string mail, ImmutableList<string> itemsBidding) {
      Mail = mail;
      ItemsBidding = itemsBidding;
    }

    // instead of mutating the object, return a new one
    public UserInfo AddItemBidding(string itemId) {
      return ItemsBidding.Contains(itemId)
        ? this
        : new UserInfo(Mail, ((ImmutableList<string>)ItemsBidding).Add(itemId));
    }

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context) {
      ItemsBidding = ItemsBidding.ToImmutableList();
    }
  }
}