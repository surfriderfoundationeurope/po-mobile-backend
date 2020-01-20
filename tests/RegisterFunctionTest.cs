using Microsoft.AspNetCore.Mvc;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;
using Xunit;
using Surfrider.PlasticOrigins.Backend.Mobile;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class RegisterFunctionTest : SurfriderBaseFunctionTest
    {
        [Fact]
        public async void Register_should_return_id()
        {
            var request = TestFactory.CreateHttpRequest(new
            {
                lastName = "Montmirail",
                firstName = "Eudes",
                birthYear = 1969,
                email = "eudes.montpirail@corp.com",
                password = "Compl1catedP@ssw0rd"
            });

            var settings = new InMemoryConfigurationService();
            settings.SetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey, "4P%)TC6M_BvfWN8#69r4/e*uAVke.v/R&jNTM4}RBTK65XRrUX2ZG47G$+]@uGGuS2n!)q3uty+;dd?wJpyb_=Y?n.7da(RP_NW8;_---gUGe&vq#BwHi.HSkFE8zt)5f?%MhqWxX*,f2D$?jM3A/5g2=V.#quZ*BEPfL*yCp$MWb%K{B]KDEaEW2jL#8C%}KgB;M9{(=ZMw(6KuT-i@gHQp$WF=uvA{BwKHC2vJRca}5}79MmiBiaM,}TcfUVq}F,:?@.Z}xvb/vaza!rGJfnWR$e;#Vgv8mKE?mkE6yB?w4wWEdi2@mSt#%S7J{bm}=*iBXKEJNf8n/Y3u&xah9C;JLk5][k--)TQDnS=xzuyiQkTxtatnA{hjRG}z(MRpXznZZeR&[qYrp9{nzSFB@xJf??bi5WR@4kL+hG7kV=;zYx5X)qB-J&YS=4m]vHBT7{#6PjZfQwe.2RGu3P88}Ji8#VR!28BHQEk6JK$3xQ/eE$Pu.NPvzNTF6DNF[e(3");
            var userFunctionsHost = new UserFunctions(new UserService(new InMemoryUserStore(), settings), settings);
            
            var response = (OkObjectResult) await userFunctionsHost.RunRegister(request, Logger);
            
            Assert.Equal(200, response.StatusCode);

            // TODO: Should return token
            // TODO: Should return expiration 
        }
    }
}
