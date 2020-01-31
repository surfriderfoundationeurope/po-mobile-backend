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
        
        public static HttpRequest CreateHttpRequest()
        {
            var ctx = new DefaultHttpContext();
            return ctx.Request;
        }


        public static HttpRequest CreateHttpRequest(TestUserToken testUser)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Add("Authorization", new StringValues($"Bearer {testUser.Token}"));
            return ctx.Request;
        }

        public static HttpRequest CreateHttpRequest(object postContent)
        {
            var jsonBody = JsonConvert.SerializeObject(postContent);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));

            var ctx = new DefaultHttpContext();
            ctx.Request.Method = HttpMethods.Post;
            ctx.Request.EnableBuffering();
            ctx.Request.Body = ms;
            return ctx.Request;
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


    //public class DefaultHttpRequest : HttpRequest
    //{
    //    public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
    //    {
    //        throw new System.NotImplementedException();
    //    }

    //    public override Stream Body { get; set; }
    //    public override long? ContentLength { get; set; }
    //    public override string ContentType { get; set; }
    //    public override IRequestCookieCollection Cookies { get; set; }
    //    public override IFormCollection Form { get; set; }
    //    public override bool HasFormContentType { get; }
    //    public override IHeaderDictionary Headers { get; }
    //    public override HostString Host { get; set; }
    //    public override HttpContext HttpContext { get; }
    //    public override bool IsHttps { get; set; }
    //    public override string Method { get; set; }
    //    public override PathString Path { get; set; }
    //    public override PathString PathBase { get; set; }
    //    public override string Protocol { get; set; }
    //    public override IQueryCollection Query { get; set; }
    //    public override QueryString QueryString { get; set; }
    //    public override string Scheme { get; set; }
    //}

}
