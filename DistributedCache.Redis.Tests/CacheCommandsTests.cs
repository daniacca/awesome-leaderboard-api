
using Common.DistribuitedCache.Interfaces;
using Common.DistribuitedCache.Manager.Commands;
using Microsoft.Extensions.Caching.Distributed;
using System;
using Xunit;

namespace Common.DistributedCache.Tests
{
    public class CacheCommandsTests : IClassFixture<CacheCommandTestFixture>
    {
        private IDistributedCacheManager CacheManager { get; }

        public CacheCommandsTests(IDistributedCacheManager cache)
        {
            CacheManager = cache;
        }

        private TCommand CommandBuilder<TCommand, TPayload>(TPayload data) where TCommand : class, ICommand<IDistributedCache>
        {
            return (TCommand) Activator.CreateInstance(typeof(TCommand), new object[] { data });
        }

        private void CleanCacheFromTestData(string key)
        {
            var delCommand = CommandBuilder<RemoveCommand, string>(key);
            CacheManager.Execute(delCommand);
        }

        [Fact]
        public void Test_Get_Command_With_Random_Key()
        {
            var getCommand = CommandBuilder<GetCommand, string>(Guid.NewGuid().ToString());
            CacheManager.Execute(getCommand);
            Assert.Null(getCommand.Result);
        }

        [Fact]
        public void Test_Set_And_Get_Command_With_Json_Obj()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = new TestingClass();

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand<TestingClass>, SetCommandPayload<TestingClass>>(new SetCommandPayload<TestingClass> { Data = testData, Key = testKey });
                var getCommand = CommandBuilder<GetCommand<TestingClass>, string>(testKey);

                // 1 - execute Get Command before setting => should return NULL
                CacheManager.Execute(getCommand);
                Assert.Null(getCommand.Result);

                // 2 - execute Set Command
                CacheManager.Execute(setCommand);

                // 3 - verify added object with Get command again
                CacheManager.Execute(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public void Test_Set_And_Get_Command_With_String()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid()}";

            try
            {
                // Generate command
                var getCommand = CommandBuilder<GetCommand, string>(testKey);
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> 
                { 
                    Data = testData, 
                    Key = testKey 
                });

                // 1 - execute Get Command before setting => should return NULL
                CacheManager.Execute(getCommand);
                Assert.Null(getCommand.Result);

                // 2 - execute Set Command
                CacheManager.Execute(setCommand);

                // 3 - verify added object with Get command again
                CacheManager.Execute(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public void Test_Set_And_Remove_Command()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid().ToString()}";

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> 
                { 
                    Data = testData, 
                    Key = testKey 
                });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);
                var delCommand = CommandBuilder<RemoveCommand, string>(testKey);

                // 1 - execute Set Command
                CacheManager.Execute(setCommand);

                // 2 - execute Remove Command
                CacheManager.Execute(delCommand);

                // 3 - verify that the element is not present anymore on cache
                CacheManager.Execute(getCommand);
                Assert.Null(getCommand.Result);
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public void Test_Set_Command_With_Options()
        {
            // Generate new test data
            var testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid()}";
            var testOptions = new DistributedCacheEntryOptions();
            testOptions.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(3));

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey, Options = testOptions });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);

                // 1 - execute Set Command
                CacheManager.Execute(setCommand);

                // 2 - verify added object with Get command again - immediately
                CacheManager.Execute(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));

                // 3 - wait for 6 seconds and verify that the object was removed from cache
                System.Threading.Thread.Sleep(3500);
                CacheManager.Execute(getCommand);
                Assert.Null(getCommand.Result);
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public void Test_Refresh_Command_With_Options()
        {
            // Generate new test data
            var testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid().ToString()}";
            var testOptions = new DistributedCacheEntryOptions();
            testOptions.SetSlidingExpiration(TimeSpan.FromSeconds(3));

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey, Options = testOptions });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);
                var refCommand = CommandBuilder<RefreshCommand, string>(testKey);

                // 1 - execute Set Command
                CacheManager.Execute(setCommand);

                // 2 - wait for 2 seconds and refresh object
                System.Threading.Thread.Sleep(2000);
                CacheManager.Execute(refCommand);

                // 3 - wait again for 2 seconds and verify that the object is still present
                System.Threading.Thread.Sleep(2000);
                CacheManager.Execute(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public async void Test_Get_Command_With_Random_Key_Async()
        {
            var getCommand = CommandBuilder<GetCommand, string>(Guid.NewGuid().ToString());
            await CacheManager.ExecuteAsync(getCommand);
            Assert.Null(getCommand.Result);
        }

        [Fact]
        public async void Test_Set_And_Get_Command_With_Json_Obj_Async()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = new TestingClass();

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand<TestingClass>, SetCommandPayload<TestingClass>>(new SetCommandPayload<TestingClass> { Data = testData, Key = testKey });
                var getCommand = CommandBuilder<GetCommand<TestingClass>, string>(testKey);

                // 1 - execute Get Command before setting => should return NULL
                await CacheManager.ExecuteAsync(getCommand);
                Assert.Null(getCommand.Result);

                // 2 - execute Set Command
                await CacheManager.ExecuteAsync(setCommand);

                // 3 - verify added object with Get command again
                await CacheManager.ExecuteAsync(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public async void Test_Set_And_Get_Command_With_String_Async()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid()}";

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);

                // 1 - execute Get Command before setting => should return NULL
                await CacheManager.ExecuteAsync(getCommand);
                Assert.Null(getCommand.Result);

                // 2 - execute Set Command
                await CacheManager.ExecuteAsync(setCommand);

                // 3 - verify added object with Get command again
                await CacheManager.ExecuteAsync(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public async void Test_Set_And_Remove_Command_Async()
        {
            // Generate new test data
            string testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid().ToString()}";

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);
                var delCommand = CommandBuilder<RemoveCommand, string>(testKey);

                // 1 - execute Set Command
                await CacheManager.ExecuteAsync(setCommand);

                // 2 - execute Remove Command
                await CacheManager.ExecuteAsync(delCommand);

                // 3 - verify that the element is not present anymore on cache
                await CacheManager.ExecuteAsync(getCommand);
                Assert.Null(getCommand.Result);
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public async void Test_Set_Command_With_Options_Async()
        {
            // Generate new test data
            var testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid()}";
            var testOptions = new DistributedCacheEntryOptions();
            testOptions.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds(3));

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey, Options = testOptions });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);

                // 1 - execute Set Command
                await CacheManager.ExecuteAsync(setCommand);

                // 2 - verify added object with Get command again - immediately
                await CacheManager.ExecuteAsync(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));

                // 3 - wait for 6 seconds and verify that the object was removed from cache
                System.Threading.Thread.Sleep(3500);
                await CacheManager.ExecuteAsync(getCommand);
                Assert.Null(getCommand.Result);
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }

        [Fact]
        public async void Test_Refresh_Command_With_Options_Async()
        {
            // Generate new test data
            var testKey = Guid.NewGuid().ToString();
            var testData = $"Test String {Guid.NewGuid()}";
            var testOptions = new DistributedCacheEntryOptions();
            testOptions.SetSlidingExpiration(TimeSpan.FromSeconds(3));

            try
            {
                // Generate command
                var setCommand = CommandBuilder<SetCommand, SetCommandPayload<string>>(new SetCommandPayload<string> { Data = testData, Key = testKey, Options = testOptions });
                var getCommand = CommandBuilder<GetCommand, string>(testKey);
                var refCommand = CommandBuilder<RefreshCommand, string>(testKey);

                // 1 - execute Set Command
                await CacheManager.ExecuteAsync(setCommand);

                // 2 - wait for 2 seconds and refresh object
                System.Threading.Thread.Sleep(2000);
                await CacheManager.ExecuteAsync(refCommand);

                // 3 - wait again for 2 seconds and verify that the object is still present
                System.Threading.Thread.Sleep(2000);
                await CacheManager.ExecuteAsync(getCommand);
                Assert.NotNull(getCommand.Result);
                Assert.True(testData.Equals(getCommand.Result));
            }
            finally
            {
                CleanCacheFromTestData(testKey);
            }
        }
    }
}
