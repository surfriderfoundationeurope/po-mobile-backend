using System;
using Microsoft.Azure.WebJobs.Description;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class AccessTokenAttribute : Attribute
    {
    }
}