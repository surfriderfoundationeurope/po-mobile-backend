using Microsoft.Azure.WebJobs.Host.Config;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth
{
    public class AccessTokenExtensionProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            var provider = new AccessTokenBindingProvider();
            var rule = context.AddBindingRule<AccessTokenAttribute>().Bind(provider);
        }
    }
}