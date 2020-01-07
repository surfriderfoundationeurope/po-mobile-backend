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
            var newUserBirthYear = 1990;
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";

            var user = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            Assert.NotNull(user);
            Assert.Equal(newUserLastName, user.LastName);
            Assert.Equal(newUserFirstName, user.FirstName);
            Assert.Equal(newUserBirthYear, user.BirthYear);
        }

        [Fact]
        public async void registerandretrieve_should_work()
        {
            IUserService service = GetServiceTestInstance();

            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = 1990;
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";

            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);

            var user = await service.GetUserFromId(registeredUser.Id);

            Assert.NotNull(user);
            Assert.Equal(registeredUser.Id, user.Id);
            Assert.Equal(registeredUser.LastName, user.LastName);
        }







        private UserService GetServiceTestInstance()
        {
            return new UserService(new InMemoryUserStore());
        }
    }
}
