using System;
using System.Runtime.Serialization;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class User : BaseEntity
    {
        public string Id => GetValue(nameof(Id), string.Empty);
        public string LastName => GetValue(nameof(LastName), string.Empty);
        public string FirstName => GetValue(nameof(FirstName), string.Empty);
        public string BirthYear => GetValue(nameof(BirthYear), string.Empty);
        public string PasswordHash => GetValue(nameof(PasswordHash), string.Empty);
        public string Email => GetValue(nameof(Email), string.Empty);

        [IgnoreDataMember]
        public string AuthToken { get; set; }
        [IgnoreDataMember]
        public DateTime AuthTokenExpiration { get; set; }

        public User()
        { }

        public User(string id, string lastName, string firstName, string birthYear, string passwordHash, string email)
        {
            SetValue(nameof(Id), id);
            SetValue(nameof(LastName), lastName);
            SetValue(nameof(FirstName), firstName);
            SetValue(nameof(BirthYear), birthYear);
            SetValue(nameof(PasswordHash), passwordHash);
            SetValue(nameof(Email), email);
        }

        protected override string GetEntityKey()
        {
            return Id;
        }
    }
}