using Autofac;
using Laser.Orchard.SEO.Exceptions;
using Laser.Orchard.SEO.Models;
using Laser.Orchard.SEO.Services;
using NUnit.Framework;
using Orchard.Data;
using Orchard.Tests.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.SEO.Tests.Services {
    [TestFixture]
    public class RedirectServiceTests : DatabaseEnabledTestsBase {
        private IRedirectService _redirectService;
        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<RedirectService>().As<IRedirectService>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
        }

        public override void Init() {
            base.Init();
            _redirectService = _container.Resolve<IRedirectService>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(RedirectRule),
                };
            }
        }

        private void PopulateTable(int numberOfRecords) {
            for (int i = 0; i < numberOfRecords; i++) {
                _redirectService.Add(new RedirectRule {
                    SourceUrl = "sourceUrl" + i.ToString(),
                    DestinationUrl = "destinationUrl" + i.ToString(),
                    IsPermanent = i % 2 == 0
                });
            }
        }

        [Test]
        public void RedirectsAreCreatedCorrectly() {
            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(0));
            var role = new RedirectRule {
                SourceUrl = "sourceUrl",
                DestinationUrl = "destinationUrl",
                IsPermanent = false
            };
            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(0));

            PopulateTable(6);
            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(6));

            var created = _redirectService.GetRedirects().ToArray();

            Assert.That(created.Length, Is.EqualTo(6));

            var sameObjects = true;
            for (int i = 0; i < created.Length; i++) {
                sameObjects &= created[i].SourceUrl == ("sourceUrl" + i.ToString()) &&
                    created[i].DestinationUrl == ("destinationUrl" + i.ToString()) &&
                    created[i].IsPermanent == (i % 2 == 0);
            }

            Assert.That(sameObjects);
        }


        [Test]
        public void GetRedirectsTotalCountReturnsCorrectNumber() {
            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(0));

            PopulateTable(6);

            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(6));
            Assert.That(_redirectService.GetRedirects().Count(), Is.EqualTo(6));
        }

        [Test]
        public void GetRedirectsShouldPaginate() {
            PopulateTable(6);

            var page_2_3 = _redirectService.GetRedirects(2, 3);
            Assert.That(page_2_3.Count(), Is.EqualTo(3));
            Assert.That(page_2_3.First().SourceUrl, Is.EqualTo("sourceUrl2"));

            var page_5_3 = _redirectService.GetRedirects(5, 3);
            Assert.That(page_5_3.Count(), Is.EqualTo(1));
            Assert.That(page_5_3.First().SourceUrl, Is.EqualTo("sourceUrl5"));
        }

        [Test]
        public void GetRedirectsShouldIgnoreNegativeStartIndex() {
            PopulateTable(6);
            var page = _redirectService.GetRedirects(-1);
            Assert.That(page.Count(), Is.EqualTo(6));
            Assert.That(page.First().SourceUrl, Is.EqualTo("sourceUrl0"));
        }

        [Test]
        public void GetRedirectsShouldIgnoreNegativePageSize() {

            PopulateTable(6);
            var page = _redirectService.GetRedirects(0, -2);
            Assert.That(page.Count(), Is.EqualTo(6));
            page = _redirectService.GetRedirects(2, -2);
            Assert.That(page.Count(), Is.EqualTo(4));
        }

        [Test]
        public void UpdateHappensCorrectly() {
            PopulateTable(1);
            var rule = _redirectService.GetRedirects().First();
            rule.SourceUrl += "x";
            rule.DestinationUrl += "x";
            rule.IsPermanent = false;

            var updated = _redirectService.Update(rule);

            Assert.That(updated.SourceUrl, Is.EqualTo("sourceUrl0x"));
            Assert.That(updated.DestinationUrl, Is.EqualTo("destinationUrl0x"));
            Assert.That(updated.IsPermanent, Is.EqualTo(false));
        }

        [Test]
        public void DeleteCorrectRule() {
            PopulateTable(10);
            var rules = _redirectService.GetRedirects();

            _redirectService.Delete(rules.First().Id);

            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(9));
            rules = _redirectService.GetRedirects();
            Assert.That(rules.First().SourceUrl, Is.EqualTo("sourceUrl1"));

            _redirectService.Delete(rules.First().Id);

            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(8));
            rules = _redirectService.GetRedirects();
            Assert.That(rules.First().SourceUrl, Is.EqualTo("sourceUrl2"));
        }

        [Test]
        public void CannotCreateRedirectRulesWithSameSourceUrl() {
            PopulateTable(1);
            Assert.Throws<RedirectRuleDuplicateException>(() => PopulateTable(1));
            Assert.That(_redirectService.GetTable().Count(), Is.EqualTo(1));
        }

        [Test]
        public void CannotEditRedirectRuleToHaveSameSourceUrl() {
            PopulateTable(2);

            var rule = _redirectService.GetRedirects().First();
            rule.SourceUrl = "sourceUrl1";

            Assert.Throws<RedirectRuleDuplicateException>(() => _redirectService.Update(rule));
            Assert.That(_redirectService.GetRedirects().Count(rr => rr.SourceUrl == "sourceUrl1"), Is.EqualTo(1));

            rule = _redirectService.GetTable().FirstOrDefault(x=>x.Id == 1);
            rule.SourceUrl = "sourceUrl1";

            Assert.Throws<RedirectRuleDuplicateException>(() => _redirectService.Update(rule));
            Assert.That(_redirectService.GetRedirects().Count(rr => rr.SourceUrl == "sourceUrl1"), Is.EqualTo(1));
        }

        [Test]
        public void CannotGetNonExistingRulById() {
            Assert.That(_redirectService.GetTable().FirstOrDefault(x => x.Id == 5), Is.EqualTo(null));
            Assert.That(_redirectService.GetTable().FirstOrDefault(x => x.Id == -5), Is.EqualTo(null));
        }

        [Test]
        public void CannotGetNonExistingRuleByPath() {
            Assert.That(_redirectService.GetTable().FirstOrDefault(x => x.SourceUrl == "source"), Is.EqualTo(null));
            Assert.That(_redirectService.GetTable().FirstOrDefault(x => x.SourceUrl == ""), Is.EqualTo(null));
        }

    }
}
