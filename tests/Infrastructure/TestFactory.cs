using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;


namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure
{
    public class TestFactory
    {
        public static IEnumerable<object[]> Data()
        {
            return new List<object[]>
            {
                new object[] { "name", "Bill" },
                new object[] { "name", "Paul" },
                new object[] { "name", "Steve" }

            };
        }

        private static Dictionary<string, StringValues> CreateDictionary(string key, string value)
        {
            var qs = new Dictionary<string, StringValues>
            {
                { key, value }
            };
            return qs;
        }

        public static DefaultHttpRequest CreateHttpRequest(string queryStringKey, string queryStringValue)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Query = new QueryCollection(CreateDictionary(queryStringKey, queryStringValue))
            };
            return request;
        }
        
        public static DefaultHttpRequest CreateHttpRequest()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            return request;
        }


        public static DefaultHttpRequest CreateHttpRequest(TestUserToken testUser)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            request.Headers.Add("Authorization", new StringValues($"Bearer {testUser.Token}"));
            return request;
        }

        public static DefaultHttpRequest CreateHttpRequest(object postContent)
        {
            var jsonBody = JsonConvert.SerializeObject(postContent);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));

            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Body = ms
            };
            return request;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
