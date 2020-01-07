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
            var service = new UserService();
            Assert.NotNull(service);
        }
    }
}
