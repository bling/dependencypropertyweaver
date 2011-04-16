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
            Assert.IsTrue(new SimpleDependencyObject() is DependencyObject);
            Assert.IsNotEmpty(typeof(SimpleDependencyObject).GetFields(BindingFlags.Public | BindingFlags.Static));
        }

        [Test]
        public void Abstract_base_class_inherits_from_DependencyObject()
        {
            Assert.IsTrue(typeof(DependencyObject).IsAssignableFrom(typeof(Abstract)));
        }

        [Test]
        public void Static_dependency_properties_exist_on_base_and_subclass()
        {
            Assert.IsNotNull(Abstract.ADependencyProperty);
            Assert.IsNotNull(Subclass.BDependencyProperty);
        }

        [Test]
        public void Static_properties_are_ignored()
        {
            Assert.IsEmpty(typeof(Static).GetFields(BindingFlags.Public | BindingFlags.Static));
        }
    }
}