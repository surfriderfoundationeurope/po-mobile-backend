using Microsoft.Extensions.Logging;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class SurfriderBaseFunctionTest
    {
        protected readonly ILogger Logger = TestFactory.CreateLogger();
    }
}