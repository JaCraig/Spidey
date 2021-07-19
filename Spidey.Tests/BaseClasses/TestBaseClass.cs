using Mecha.Core;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Spidey.Tests.BaseClasses
{
    /// <summary>
    /// Test base class
    /// </summary>
    public abstract class TestBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBaseClass"/> class.
        /// </summary>
        protected TestBaseClass()
        {
            _ = Mech.Default;
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        protected abstract Type? ObjectType { get; }

        /// <summary>
        /// Attempts to break the object.
        /// </summary>
        /// <returns>The async task.</returns>
        [Fact]
        public Task BreakType()
        {
            return ObjectType is null ? Task.CompletedTask : Mech.BreakAsync(ObjectType);
        }
    }

    /// <summary>
    /// Test base class
    /// </summary>
    /// <typeparam name="TTestObject">The type of the test object.</typeparam>
    public abstract class TestBaseClass<TTestObject> : TestBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestBaseClass{TTestObject}"/> class.
        /// </summary>
        protected TestBaseClass()
        {
            ObjectType = null;
        }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        protected override Type? ObjectType { get; }

        /// <summary>
        /// Gets or sets the test object.
        /// </summary>
        /// <value>The test object.</value>
        protected TTestObject? TestObject { get; set; }

        /// <summary>
        /// Attempts to break the object.
        /// </summary>
        /// <returns>The async task.</returns>
        [Fact]
        public Task BreakObject()
        {
            return TestObject is null ? Task.CompletedTask : Mech.BreakAsync(TestObject);
        }
    }
}