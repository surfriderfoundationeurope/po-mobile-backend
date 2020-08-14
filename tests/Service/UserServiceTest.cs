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

            var service = GetServiceTestInstance();
            Assert.NotNull(service);
        }

        //[Fact]
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

        //[Fact]
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

        //[Fact]
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

        //[Fact]
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

        //[Fact]
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

        //[Fact]
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

        //[Fact]
        public async void update_password_should_accept_new_password()
        {
            IUserService service = GetServiceTestInstance();
            var newUserLastName = "nom";
            var newUserFirstName = "prenom";
            var newUserBirthYear = "1990";
            var newUserEmail = "nom.prenom@gmail.com";
            var newUserPassword = "Compl3xp@ssw0rd";
            var registeredUser = await service.Register(newUserLastName, newUserFirstName, newUserBirthYear, newUserEmail, newUserPassword);
            var newPassword = "coucou123";

            var result = await service.UpdatePassword(new JwtTokenContent() { UserId = registeredUser.Id }, newPassword);

            Assert.True(result);
        }

        private UserService GetServiceTestInstance()
        {
            var settings = new InMemoryConfigurationService();
            settings.SetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey, "4P%)TC6M_BvfWN8#69r4/e*uAVke.v/R&jNTM4}RBTK65XRrUX2ZG47G$+]@uGGuS2n!)q3uty+;dd?wJpyb_=Y?n.7da(RP_NW8;_---gUGe&vq#BwHi.HSkFE8zt)5f?%MhqWxX*,f2D$?jM3A/5g2=V.#quZ*BEPfL*yCp$MWb%K{B]KDEaEW2jL#8C%}KgB;M9{(=ZMw(6KuT-i@gHQp$WF=uvA{BwKHC2vJRca}5}79MmiBiaM,}TcfUVq}F,:?@.Z}xvb/vaza!rGJfnWR$e;#Vgv8mKE?mkE6yB?w4wWEdi2@mSt#%S7J{bm}=*iBXKEJNf8n/Y3u&xah9C;JLk5][k--)TQDnS=xzuyiQkTxtatnA{hjRG}z(MRpXznZZeR&[qYrp9{nzSFB@xJf??bi5WR@4kL+hG7kV=;zYx5X)qB-J&YS=4m]vHBT7{#6PjZfQwe.2RGu3P88}Ji8#VR!28BHQEk6JK$3xQ/eE$Pu.NPvzNTF6DNF[e(3");
            return new UserService(new InMemoryUserStore(), settings);
        }

    }
}
