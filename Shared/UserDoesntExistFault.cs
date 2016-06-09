using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Shared {
  //[Serializable]
  //public class UserDoesntExistExceptionV1 : Exception {
  //  public string Mail { get; set; }

  //  public UserDoesntExistExceptionV1(string mail) :
  //      base($"User {mail} doesn't exist.") {
  //    Mail = mail;
  //  }
  //}

  [Serializable]
  public class UserDoesntExistException : Exception {
    public UserDoesntExistException() : base("User doesn't exist") { }
    public UserDoesntExistException(string message) : base(message) { }
    public UserDoesntExistException(string message, Exception inner) : base(message, inner) { }
    protected UserDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  [DataContract]
  public class UserDoesntExistFault {
    [DataMember]
    public string Mail { get; set; }
    public UserDoesntExistFault(string mail) {
      Mail = mail;
    }
  }





  //[DataContract]
  //public class UserDoesntExistFault  {
  //  [DataMember]
  //  public string Mail { get; set; }

  //  public UserDoesntExistFault(string mail) {
  //    Mail = mail;
  //  }
  //}
  //[Serializable]
  //public class UserDoesntExistException : Exception {
  //  public UserDoesntExistException() {
  //  }

  //  protected UserDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context) {
      
  //  }

  //}
  //[DataContract]
  //public class UserDoesntExistFault  {
  //  [DataMember]
  //  public string Mail { get; }

  //  public UserDoesntExistFault(string mail) :
  //    base($"User {mail} doesn't exist.") {
  //    Mail = mail;
  //  }
  //}
}