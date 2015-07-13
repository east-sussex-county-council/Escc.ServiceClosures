using System;
using NUnit.Framework;

namespace Escc.ServiceClosures.Tests
{
    [TestFixture]
    public class ServiceTests
    {
        [Test]
        public void ClosuresTodayReturnsOneResult()
        {
            var service = new Service();
            service.Closures.Add(new Closure() { StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(-1), Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure() { StartDate = DateTime.Today, EndDate = DateTime.Today, Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure() { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(1), Status = ClosureStatus.Closed });

            var closures = service.CheckForClosuresToday();

            Assert.AreEqual(1, closures.Count);
        }

        [Test]
        public void ClosuresTomorrowReturnsOneResult()
        {
            var service = new Service();
            service.Closures.Add(new Closure() { StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(-1), Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure() { StartDate = DateTime.Today, EndDate = DateTime.Today, Status = ClosureStatus.Closed });
            service.Closures.Add(new Closure() { StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(1), Status = ClosureStatus.Closed });

            var closures = service.CheckForClosuresTomorrow();

            Assert.AreEqual(1, closures.Count);
        }
    }
}
