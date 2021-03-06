﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public abstract class WebApiTestsBase
    {
        protected IODataClient _client;

        protected WebApiTestsBase(ODataClientSettings settings)
        {
            _client = new ODataClient(settings);
        }

        [TearDown]
		public void TearDown()
        {
            if (_client != null)
            {
                AsyncContext.Run(async () =>
                {
                    await DeleteTestData();
                });
            }
        }

		private async Task DeleteTestData()
        {
            var products = await _client.FindEntriesAsync("Products");
            foreach (var product in products)
            {
                if (product["Name"].ToString().StartsWith("Test"))
                    await _client.DeleteEntryAsync("Products", product);
            }

            var workTaskModels = await _client.FindEntriesAsync("WorkTaskModels");
            foreach (var workTaskModel in workTaskModels)
            {
                if (workTaskModel["Code"].ToString().StartsWith("Test"))
                    await _client.DeleteEntryAsync("workTaskModels", workTaskModel);
            }
        }

        [Test]
        public void GetProductsCount()
        {
            AsyncContext.Run(async () =>
            {
                var products = await _client
                    .For("Products")
                    .FindEntriesAsync();

                Assert.AreEqual(5, products.Count());
            }); 
        }

		[Test]
        public void InsertProduct()
	    {
            AsyncContext.Run(async () =>
            {
                var product = await _client
                    .For("Products")
                    .Set(new Entry() { { "Name", "Test1" }, { "Price", 18m } })
                    .InsertEntryAsync();

                Assert.AreEqual("Test1", product["Name"]);
            });
        }

		[Test]
        public void UpdateProduct()
        {
            AsyncContext.Run(async () =>
            {
                var product = await _client
                     .For("Products")
                     .Set(new { Name = "Test1", Price = 18m })
                     .InsertEntryAsync();

                product = await _client
                    .For("Products")
                    .Key(product["ID"])
                    .Set(new { Price = 123m })
                    .UpdateEntryAsync();

                Assert.AreEqual(123m, product["Price"]);
            });
        }

		[Test]
        public void DeleteProduct()
        {
            AsyncContext.Run(async () =>
            {
                var product = await _client
                    .For("Products")
                    .Set(new { Name = "Test1", Price = 18m })
                    .InsertEntryAsync();

                await _client
                    .For("Products")
                    .Key(product["ID"])
                    .DeleteEntryAsync();

                product = await _client
                    .For("Products")
                    .Filter("Name eq 'Test1'")
                    .FindEntryAsync();

                Assert.Null(product);
            });
        }

		[Test]
        public void InsertWorkTaskModel()
        {
            AsyncContext.Run(async () =>
            {
                var workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                    .InsertEntryAsync();

                Assert.AreEqual("Test1", workTaskModel["Code"]);
            });
        }

		[Test]
        public void UpdateWorkTaskModel()
        {
            AsyncContext.Run(async () =>
            {
                var workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                    .InsertEntryAsync();

                workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Key(workTaskModel["Id"])
                    .Set(new { Code = "Test2" })
                    .UpdateEntryAsync();

                Assert.AreEqual("Test2", workTaskModel["Code"]);
            });
        }

		[Test]
        public void UpdateWorkTaskModelWithEmptyLists()
        {
            AsyncContext.Run(async () =>
            {
                var workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                    .InsertEntryAsync();

                workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Key(workTaskModel["Id"])
                    .Set(new Entry() { { "Code", "Test2" }, { "Attachments", new List<IDictionary<string, object>>() }, { "WorkActivityReports", null } })
                    .UpdateEntryAsync();

                Assert.AreEqual("Test2", workTaskModel["Code"]);
            });
        }

		[Test]
        public void UpdateWorkTaskModelWholeObject()
        {
            AsyncContext.Run(async () =>
            {
                var workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                    .InsertEntryAsync();

                workTaskModel["Code"] = "Test2";
                workTaskModel["Attachments"] = new List<IDictionary<string, object>>();
                workTaskModel["WorkActivityReports"] = null;
                workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Key(workTaskModel["Id"])
                    .Set(workTaskModel)
                    .UpdateEntryAsync();

                Assert.AreEqual("Test2", workTaskModel["Code"]);

                workTaskModel["Code"] = "Test3";
                workTaskModel["Attachments"] = null;
                workTaskModel["WorkActivityReports"] = new List<IDictionary<string, object>>();
                workTaskModel = await _client
                    .For("WorkTaskModels")
                    .Key(workTaskModel["Id"])
                    .Set(workTaskModel)
                    .UpdateEntryAsync();

                Assert.AreEqual("Test3", workTaskModel["Code"]);
            });
        }
    }

	[TestFixture]
    public class WebApiTests : WebApiTestsBase
    {
        public WebApiTests()
            : base(new ODataClientSettings("http://va-odata-integration.azurewebsites.net/odata/open"))
        {
        }
    }

	[TestFixture]
    public class WebApiWithAuthenticationTests : WebApiTestsBase
    {
        private const string _user = "tester";
        private const string _password = "tester123";

        public WebApiWithAuthenticationTests()
            : base(new ODataClientSettings()
            {
                UrlBase = "http://va-odata-integration.azurewebsites.net/odata/secure",
                Credentials = new NetworkCredential(_user, _password)
            })
        {
        }
    }
}
