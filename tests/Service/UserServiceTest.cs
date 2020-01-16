using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests.Service
{
    public class UserServiceTest
    {
        [Fact]
        public void ctor_shoud_create()
        {
            //
            var userStore = new InMemoryUserStore();
            var service = new UserService(userStore);
            Assert.NotNull(service);
        }

        [Fact]
        public async void register_should_work()
        {
            IUserService service = GetServiceTestInstance();

            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";

            var user = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            Assert.NotNull(user);
            Assert.Equal(newUserLastName, user.LastName);
            Assert.Equal(newUserFirstName, user.FirstName);
            Assert.Equal(newUserBirthYear, user.BirthYear);
            Assert.NotNull(user.AuthToken);
        }

        [Fact]
        public async void registerandretrieve_should_work()
        {
            IUserService service = GetServiceTestInstance();

            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";

            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            var user = await service.GetUserFromId(registeredUser.Id);

            Assert.NotNull(user);
            Assert.Equal(registeredUser.Id, user.Id);
            Assert.Equal(registeredUser.LastName, user.LastName);
        }

        [Fact]
        public async void login_with_correctAccount_ShouldReturnTrue()
        {
            IUserService service = GetServiceTestInstance();
            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";
            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            bool areCorrect = await service.CheckUserCredentials(newUserEmail, newUserPassword);

            Assert.True(areCorrect);
        }

        [Fact]
        public async void login_with_incorrectAccount_ShouldReturnFalse()
        {
            IUserService service = GetServiceTestInstance();
            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";
            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            bool areCorrect = await service.CheckUserCredentials("demouser123@gmail.com", newUserPassword);

            Assert.False(areCorrect);
        }

        [Fact]
        public async void login_with_incorrectPassword_ShouldReturnFalse()
        {
            IUserService service = GetServiceTestInstance();
            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";
            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            bool areCorrect = await service.CheckUserCredentials(newUserEmail, "UnPassQuiNexistePas");

            Assert.False(areCorrect);
        }

        [Fact]
        public async void generate_token_should_return_token()
        {
            IUserService service = GetServiceTestInstance();
            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";
            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            var token = await service.GenerateTokenFromPassword(newUserEmail, newUserPassword);
            
            Assert.NotNull(token);
        }

        private UserService GetServiceTestInstance()
        {
            return new UserService(new InMemoryUserStore());
        }
    }
}
