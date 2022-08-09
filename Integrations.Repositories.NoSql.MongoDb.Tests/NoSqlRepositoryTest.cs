using Common.Utils.ExtensionMethods;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MongoDB.Driver;
using System.Collections.Generic;
using NoSql.MongoDb.Types;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using NoSql.MongoDb.ExtensionMethods;
using NoSql.MongoDb.Tests.ExtensionMethods;

namespace NoSql.MongoDb.Tests
{
    public class ProjectOut
    {
        public long OutNumber { get; set; }
    }

    public class NoSqlRepositoryTest : NoSqlBaseRepositoryTest<TestModel, ITestModelRepository>, IClassFixture<NoSqlBaseClassFixture>
    {
        private int TotalDoc => 1000;

        protected override bool DropCollection => true;

        public NoSqlRepositoryTest(ITestModelRepository repo, IOptions<MongoConnection> settings) : base(repo, settings)
        {
            FeedDatabase(TotalDoc);
        }

        protected string RandomPlaceHolder => $"{(Placeholder)new Random().Next(0, 2)}";

        protected override TestModel GenerateTestDocument(int counter = 0)
        {
            var testNumber = new Random().Next(50, 500);
            return new TestModel
            {
                TestString = $"feeding_test_data_{RandomPlaceHolder}_{Guid.NewGuid()}",
                TestNumber = testNumber,
                TestDate = DateTime.Now.AddDays(new Random().Next(1, 120)),
                TestTreeRoot = new InnerOne
                {
                    TestNumberInnerOne = new Random().Next(1, 10),
                    TestStringInnerOne = $"inner_one_{RandomPlaceHolder}_{Guid.NewGuid()}",
                    InnerTree = new InnerTwo
                    {
                        TestNumberInnerTwo = new Random().Next(11, 20),
                        TestStringInnerTwo = $"inner_two_{RandomPlaceHolder}_{Guid.NewGuid()}",
                    }
                },
                TestEnumerator = (TestEnum)new Random().Next(0, 2),
                TestIndex = counter,
                TestUniqueGuidIndex = Guid.NewGuid()
            };
        }

        protected override void FeedDatabase(int document_number)
        {
            var list = new List<TestModel>(document_number);
            for(var counter = 0; counter < document_number; counter++)
                list.Add(GenerateTestDocument(counter));

            Repository.GetDBContext().Collection.InsertMany(list, new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
        }

        [Fact]
        public void Get_Test()
        {
            // 1 - Get List with LINQ lambda expression
            var resultList = Repository.GetList(x => x.TestNumber > 50).ToList();
            Assert.NotNull(resultList);
            Assert.True(resultList.Count > 0);
            resultList.ForEach(e =>
            {
                Assert.NotNull(e.TestString);
                Assert.True(e.TestNumber > 50);
                Assert.True(e.TestDate > DateTime.Now.AddDays(-120));
            });

            // 2 - Get List with Dynamic LINQ query as string
            var resultDynamic = Repository.GetList($"{nameof(TestModel.TestNumber)} > 50").data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count == resultList.Count);
            _ = resultDynamic.Zip(resultList, (l, d) =>
            {
                Assert.Equal(l.TestString, d.TestString);
                Assert.Equal(l.TestNumber, d.TestNumber);
                Assert.Equal(l.TestDate, d.TestDate);
                return true;
            });

            // 3 - Single Get with _id
            var first = resultList.First();
            var resultGet = Repository.Get(first._id.ToString());
            Assert.NotNull(resultGet);
            Assert.Equal(first.TestString, resultGet.TestString);
            Assert.Equal(first.TestNumber, resultGet.TestNumber);
            Assert.Equal(first.TestDate, resultGet.TestDate);
        }

        [Fact]
        public void Add_Update_Delete_Test()
        {
            var testModel = new TestModel
            {
                TestString = "Adding Test",
                TestNumber = 15000,
                TestDate = DateTime.Now
            };

            Repository.Add(testModel);

            var result = Repository.GetList($"{nameof(TestModel.TestNumber)} = {testModel.TestNumber}").data.FirstOrDefault();
            Assert.Equal(testModel.TestString, result.TestString);
            Assert.Equal(testModel.TestNumber, result.TestNumber);
            Assert.Equal(testModel.TestDate.Date, result.TestDate.Date);

            result.TestString = "Updated";
            Repository.Update(result);

            var updated = Repository.Get(result._id.ToString());
            Assert.Equal(result.TestString, updated.TestString);
            Assert.Equal(result.TestNumber, updated.TestNumber);
            Assert.Equal(result.TestDate.Date, updated.TestDate.Date);

            Repository.Remove(updated);
            var removed = Repository.Get(updated._id.ToString());
            Assert.Null(removed);
        }

        [Fact]
        public async Task Get_Async_Test()
        {
            string testStr = null;
            long testNmb = 0;

            // 1 - Get List with LINQ lambda expression
            var resultList = (await Repository.GetListAsync(x => x.TestNumber > 50)).ToList();
            Assert.NotNull(resultList);
            Assert.True(resultList.Count > 0);
            testStr = resultList.FirstOrDefault().TestString;
            testNmb = resultList.FirstOrDefault().TestNumber;
            resultList.ForEach(e =>
            {
                Assert.NotNull(e.TestString);
                Assert.True(e.TestNumber > 50);
                Assert.True(e.TestDate > DateTime.Now.AddDays(-120));
            });

            // 2 - Get List with Dynamic LINQ query as string
            // 2.1 - Get List with int number filter
            var resultDynamic = (await Repository.GetListAsync($"{nameof(TestModel.TestNumber)} > 50 AND {nameof(TestModel.TestNumber)} <= 999")).data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count == resultList.Count);
            _ = resultDynamic.Zip(resultList, (l, d) =>
              {
                  Assert.Equal(l.TestString, d.TestString);
                  Assert.Equal(l.TestNumber, d.TestNumber);
                  Assert.Equal(l.TestDate, d.TestDate);
                  return true;
              });

            // 2.2 - Get List with exact string and number
            resultDynamic = (await Repository.GetListAsync($"{nameof(TestModel.TestString)} = \"{testStr}\" AND {nameof(TestModel.TestNumber)} = {testNmb}")).data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count == 1);
            
            // 2.3 - Get List with contains filter
            resultDynamic = (await Repository.GetListAsync($"{nameof(TestModel.TestString)}.Contains('feeding_test_data_{RandomPlaceHolder}')")).data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count > 0);

            // 2.4 - Get List with contains filter on nested props
            resultDynamic = (await Repository.GetListAsync($"{nameof(TestModel.TestTreeRoot)}.{nameof(TestModel.TestTreeRoot.TestStringInnerOne)}.Contains('inner_one_{RandomPlaceHolder}')")).data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count > 0);
            
            // 2.5 - Get List with datetime
            resultDynamic = (await Repository.GetListAsync($"{nameof(TestModel.TestDate)} >= DateTime({DateTime.Now.Year},{DateTime.Now.Month},{DateTime.Now.Day})")).data.ToList();
            Assert.NotNull(resultDynamic);
            Assert.True(resultDynamic.Count > 0);

            // 3 - Single Get with _id
            var first = resultList.First();
            var resultGet = await Repository.GetAsync(first._id.ToString());
            Assert.NotNull(resultGet);
            Assert.Equal(first.TestString, resultGet.TestString);
            Assert.Equal(first.TestNumber, resultGet.TestNumber);
            Assert.Equal(first.TestDate, resultGet.TestDate);
        }

        [Fact]
        public async Task Add_Update_Delete_Async_Test()
        {
            var testModel = new TestModel
            {
                TestString = "Adding Test",
                TestNumber = 15000,
                TestDate = DateTime.Now
            };

            await Repository.AddAsync(testModel);

            var result = (await Repository.GetListAsync($"{nameof(TestModel.TestNumber)} = {testModel.TestNumber}")).data.FirstOrDefault();
            Assert.Equal(testModel.TestString, result.TestString);
            Assert.Equal(testModel.TestNumber, result.TestNumber);
            Assert.Equal(testModel.TestDate.Date, result.TestDate.Date);

            result.TestString = "Updated";
            await Repository.UpdateAsync(result);

            var updated = await Repository.GetAsync(result._id.ToString());
            Assert.Equal(result.TestString, updated.TestString);
            Assert.Equal(result.TestNumber, updated.TestNumber);
            Assert.Equal(result.TestDate.Date, updated.TestDate.Date);

            await Repository.RemoveAsync(updated);
            var removed = await Repository.GetAsync(updated._id.ToString());
            Assert.Null(removed);
        }

        [Fact]
        public async Task Bulk_Add_Update_Delete_Async_Test()
        {
            var testsModel = Enumerable.Repeat(0, 1000).Select(_ => GenerateTestDocument()).ToList();
            Assert.True(await Repository.AddAsync(testsModel));
            Assert.All(testsModel, model => Assert.True(model._id.ToString() != ObjectId.Empty.ToString()));
            Assert.All(testsModel, model => Assert.True(model.Hash != 0));

            testsModel.ToList().ForEach(t => t.TestString = "Bulk_Update");
            Assert.True(await Repository.UpdateAsync(testsModel));

            var updated = await Repository.GetListAsync(model => model.TestString == "Bulk_Update");
            Assert.True(updated.ToList().Count == testsModel.Count);
            Assert.All(updated, u => Assert.Equal("Bulk_Update", u.TestString));

            Assert.True(await Repository.RemoveAsync(updated));
            var removed = await Repository.GetListAsync(model => model.TestString == "Bulk_Update");
            Assert.True(removed.ToList().Count == 0);
        }

        [Fact]
        public void Bulk_Add_Update_Delete_Test()
        {
            var testsModel = Enumerable.Repeat(0, 1000).Select(_ => GenerateTestDocument()).ToList();
            Assert.True(Repository.Add(testsModel));
            Assert.All(testsModel, model => Assert.True(model._id.ToString() != ObjectId.Empty.ToString()));
            Assert.All(testsModel, model => Assert.True(model.Hash != 0));

            testsModel.ToList().ForEach(t => t.TestString = "Bulk_Update");
            Assert.True(Repository.Update(testsModel));

            var updated = Repository.GetList(model => model.TestString == "Bulk_Update");
            Assert.True(updated.ToList().Count == testsModel.Count);
            Assert.All(updated, u => Assert.Equal("Bulk_Update", u.TestString));

            Assert.True(Repository.Remove(updated));
            var removed = Repository.GetList(model => model.TestString == "Bulk_Update");
            Assert.True(removed.ToList().Count == 0);
        }

        [Fact]
        public async Task Search_Pipeline_Async_Test()
        {
            var match = new BsonDocument("$match", new BsonDocument
            {
                {
                    $"{nameof(TestModel.TestNumber).ToCamelCase()}",
                    new BsonDocument { { "$gt", 60 } }
                }
            });

            var sort = new BsonDocument("$sort", new BsonDocument
            {
                { $"{nameof(TestModel.TestNumber).ToCamelCase()}", 1 }
            });

            var result = await Repository.GetListAsync<TestModel>(new BsonDocument[] { match, sort });

            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.Hash != 0));
            Assert.All(result, r => Assert.True(r.TestNumber > 60));
            Assert.All(result, r => Assert.StartsWith("feeding_test_data_", r.TestString));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].TestNumber <= chunk[1].TestNumber); });
        }

        [Fact]
        public void Search_Pipeline_Test()
        {
            // Arrange
            var match = new BsonDocument("$match", new BsonDocument
            {
                {
                    $"{nameof(TestModel.TestNumber).ToCamelCase()}",
                    new BsonDocument { { "$gt", 60 } }
                }
            });

            var sort = new BsonDocument("$sort", new BsonDocument
            {
                { $"{nameof(TestModel.TestNumber).ToCamelCase()}", 1 }
            });
            
            var pipeline = new BsonDocument[] { match, sort };

            // Act
            var result = Repository.GetList<TestModel>(pipeline);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.Hash != 0));
            Assert.All(result, r => Assert.True(r.TestNumber > 60));
            Assert.All(result, r => Assert.StartsWith("feeding_test_data_", r.TestString));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].TestNumber <= chunk[1].TestNumber); });
        }

        [Fact]
        public async Task Fluent_Pipeline_Async_Tests()
        {
            // Arrange - define and build stages
            IEnumerable<AbsAggregationPipelineStage> stages = new List<AbsAggregationPipelineStage>()
                .Match<TestModel>(item => item.TestNumber > 100)
                .Sort(Builders<TestModel>.Sort.Ascending(nameof(TestModel.TestNumber)));

            // Act
            var result = await Repository.GetListAsync<TestModel>(stages);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.Hash != 0));
            Assert.All(result, r => Assert.True(r.TestNumber > 100));
            Assert.All(result, r => Assert.StartsWith("feeding_test_data_", r.TestString));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].TestNumber <= chunk[1].TestNumber); });
        }

        [Fact]
        public void Fluent_Pipeline_Tests()
        {
            // Arrange - define and build stages
            IEnumerable<AbsAggregationPipelineStage> stages = new List<AbsAggregationPipelineStage>()
                .Match<TestModel>(item => item.TestNumber > 100)
                .Sort(Builders<TestModel>.Sort.Ascending(nameof(TestModel.TestNumber)));

            // Act
            var result = Repository.GetList<TestModel>(stages);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.Hash != 0));
            Assert.All(result, r => Assert.True(r.TestNumber > 100));
            Assert.All(result, r => Assert.StartsWith("feeding_test_data_", r.TestString));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].TestNumber <= chunk[1].TestNumber); });
        }

        [Fact]
        public async Task Fluent_Pipeline_Project_Async_Tests()
        {
            // Arrange - define and build stages
            IEnumerable<AbsAggregationPipelineStage> stages = new List<AbsAggregationPipelineStage>()
                .Match<TestModel>(item => item.TestNumber > 100)
                .SortBy<TestModel>(item => item.TestNumber)
                .Project<TestModel, ProjectOut>(item => new ProjectOut { OutNumber = item.TestNumber });

            // Act
            var result = await Repository.GetListAsync<ProjectOut>(stages);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.OutNumber > 100));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].OutNumber <= chunk[1].OutNumber); });
        }

        [Fact]
        public void Fluent_Pipeline_Project_Tests()
        {
            // Arrange - define and build stages
            IEnumerable<AbsAggregationPipelineStage> stages = new List<AbsAggregationPipelineStage>()
                .Match<TestModel>(item => item.TestNumber > 100)
                .SortBy<TestModel>(item => item.TestNumber)
                .Project<TestModel, ProjectOut>(item => new ProjectOut { OutNumber = item.TestNumber });

            // Act
            var result = Repository.GetList<ProjectOut>(stages);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Any());
            Assert.All(result, r => Assert.True(r.OutNumber > 100));
            Assert.All(result.ChunksOf(2), chunk => { if (chunk.Count > 1) Assert.True(chunk[0].OutNumber <= chunk[1].OutNumber); });
        }

        [Fact]
        public void Update_With_Insert_Test()
        {
            // Arrange
            var addTestModel1 = GenerateTestDocument();
            addTestModel1.TestString = "TO add 1";

            var addTestModel2 = GenerateTestDocument();
            addTestModel2.TestString = "TO add 2";

            var testModel2Update = Repository.GetList(d => d.TestNumber > 50);
            var updatedList = testModel2Update.ToList();
            updatedList.ForEach(d => d.TestString = $"Updated - {d.TestString}");
            updatedList.Add(addTestModel1);
            updatedList.Add(addTestModel2);

            // Act
            var result = Repository.Update(updatedList);

            // Assert
            Assert.True(result);
            Assert.NotNull(Repository.GetList(d => d.TestString == "TO add 1").FirstOrDefault());
            Assert.NotNull(Repository.GetList(d => d.TestString == "TO add 2").FirstOrDefault());

            var updatedModel = Repository.GetList(d => d.TestString.Contains("Updated - "));
            Assert.NotNull(updatedList);
            Assert.Equal(updatedList.Count - 2, updatedModel.Count());
        }

        [Fact]
        public async Task Update_With_Insert_Test_Async()
        {
            // Arrange
            var addTestModel1 = GenerateTestDocument();
            addTestModel1.TestString = "TO add 1";

            var addTestModel2 = GenerateTestDocument();
            addTestModel2.TestString = "TO add 2";

            var testModel2Update = await Repository.GetListAsync(d => d.TestNumber > 50);
            var updatedList = testModel2Update.ToList();
            updatedList.ForEach(d => d.TestString = $"Updated - {d.TestString}");
            updatedList.Add(addTestModel1);
            updatedList.Add(addTestModel2);

            // Act
            var result = await Repository.UpdateAsync(updatedList);

            // Assert
            Assert.True(result);
            Assert.NotNull((await Repository.GetListAsync(d => d.TestString == "TO add 1")).FirstOrDefault());
            Assert.NotNull((await Repository.GetListAsync(d => d.TestString == "TO add 2")).FirstOrDefault());

            var updatedModel = await Repository.GetListAsync(d => d.TestString.Contains("Updated - "));
            Assert.NotNull(updatedList);
            Assert.Equal(updatedList.Count - 2, updatedModel.Count());
        }
    }
}
