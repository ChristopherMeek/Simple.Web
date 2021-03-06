﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Web.Ninject.Tests
{
    using CodeGeneration;
    using Xunit;
    using global::Ninject.Modules;

    public class HandlerFactoryBuilderTests
    {
        [Fact]
        public void CreatesInstanceOfType()
        {
            var startup = new TestStartup();
            startup.Run(SimpleWeb.Configuration, SimpleWeb.Environment);
            var target = new HandlerBuilderFactory(SimpleWeb.Configuration);
            var actualFunc = target.BuildHandlerBuilder(typeof(TestHandler));
            var actual = (TestHandler)actualFunc(new Dictionary<string, string> { { "TestProperty", "Foo" } }).Handler;
            Assert.Equal(Status.OK, actual.Get());
            Assert.Equal("Foo", actual.TestProperty);
        }

        [Fact]
        public void DisposesInstances()
        {
            var startup = new TestStartup();
            startup.Run(SimpleWeb.Configuration, SimpleWeb.Environment);
            var target = new HandlerBuilderFactory(SimpleWeb.Configuration);
            var actualFunc = target.BuildHandlerBuilder(typeof(TestHandler));

            TestHandler handler;
            using (var scopedHandler = actualFunc(new Dictionary<string, string>()))
            {
                handler = (TestHandler)scopedHandler.Handler;
                Assert.Equal(false, handler.IsDisposed);
            }
            Assert.Equal(true, handler.IsDisposed);
        }
    }

    public class TestStartup : NinjectStartupBase
    {
        protected override IEnumerable<INinjectModule> CreateModules()
        {
            yield return new TestModule();
        }
    }

    public class TestModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IResult>().To<OkResult>();
        }
    }

    public class TestHandler : IGet, IDisposable
    {
        private readonly IResult _result;
        public bool IsDisposed { get; set; }

        public TestHandler(IResult result)
        {
            _result = result;
        }

        public Status Get()
        {
            return _result.Result;
        }

        public string TestProperty { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public interface IResult
    {
        Status Result { get; }
    }

    public class OkResult : IResult
    {
        public Status Result { get { return Status.OK; }}
    }
}
