using System.Reflection;
using System.Windows;
using DependencyPropertyWeaver.Tests.Models;
using NUnit.Framework;

namespace DependencyPropertyWeaver.Tests
{
    public class WeavingTests
    {
        [Test]
        public void Poco_object_gets_DependencyObject_base_added()
        {
            Assert.IsTrue(new Simple() is DependencyObject);
        }

        [Test]
        public void Object_that_is_preexisting_DependencyObject_is_unmodified()
        {
            Assert.IsTrue(new SimpleDP() is DependencyObject);
            Assert.IsNotEmpty(typeof(SimpleDP).GetFields(BindingFlags.Public | BindingFlags.Static));
        }

        [Test]
        public void Abstract_base_class_inherits_from_DependencyObject()
        {
            Assert.IsTrue(typeof(DependencyObject).IsAssignableFrom(typeof(Abstract)));
        }
    }
}