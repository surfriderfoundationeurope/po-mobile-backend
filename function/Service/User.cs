using System;
using System.Runtime.Serialization;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class User
    {
        public User(string id, string lastName, string firstName, string birthYear)
        {
            Id = id;
            LastName = lastName;
            FirstName = firstName;
            BirthYear = birthYear;
        }

        public string Id { get; }
        public string LastName { get; }
        public string FirstName { get; }
        public string BirthYear { get; }

        [IgnoreDataMember]
        public string AuthToken { get; set;  }
        [IgnoreDataMember]
        public DateTime AuthTokenExpiration { get; set; }
    }
}