using System;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Logging;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class SurfriderBaseFunctionTest
    {
        protected readonly ILogger Logger = TestFactory.CreateLogger();
        public string TestUserId => Guid.NewGuid().ToString("N");
        public string TestUserEmail => "ut-po@surfrider.eu";
        public DateTime TokenDuration => DateTime.UtcNow.AddDays(1);

        public string TokenSignatureKey =>
            "DnthuckWC9i9aEXCSirVyyIiTDhZ0jFNEsSVvsxowx6cGaEze2V89WBFWr3c4CvKpSIjEELsUrR1CpxqXYHYobT5Je9mCue3uYmPWrKwbr1PpnNcd8mwrJtgdorjakc9VLcIo1ycC1eErRWFu2AwpQjb9o91aMl1dfdL1UarCiWUrlzIssueorWovidxrQtZwpTGaBG46IdQDHltrtFH2zsdMNmTbGQysTrbRyHSngb9DqFZYrXeI8doIPpvCjAmHRo5YLEnB0ZDkiajep0uX0Nid3C0AVWAF7JVoTsAbXm9yQONjtm9YmniS55pqVum5nSn5aiJfyGB6eMmPR9ATQ5XazanJLWT4bN1rDtAd6DEtjcjLaqWkLeSXwuc33OHP350B89VtNZqxHDTn2a9K8kXiQ5RTVqXqeegYjRM4ArGQ5VBmnI5c4aMfZwulJsHFafV551mkgsbcEggQLDwxEnVFu1JVOnLxRDqkyK4M9FurZemdSCiGiC66wSut7Pz";
    }
}