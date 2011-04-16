using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using NUnit.Framework;

namespace DependencyPropertyWeaver.Tests
{
    public abstract class WpfTestBase<T> where T : Window, new()
    {
        private readonly T _window;

        protected T Window
        {
            get { return _window; }
        }

        protected WpfTestBase()
        {
            _window = new T();
        }

        protected T CreateWindow()
        {
            return new T();
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _window.Show();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}