﻿using System;
using NUnit.Framework;

namespace Escc.ServiceClosures.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        [Test]
        public static void ClosuresTodayReturnsOneResult()
        {
            var service = new Service();
            service.Closures.Add(new Closure { StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(-1), Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure { StartDate = DateTime.Today, EndDate = DateTime.Today, Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(1), Status = ClosureStatus.Closed });

            var closures = service.CheckForClosures(DateTime.Today);

            Assert.AreEqual(1, closures.Count);
        }
    }
}
