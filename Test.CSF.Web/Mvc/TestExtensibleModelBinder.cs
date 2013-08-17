using System;
using NUnit.Framework;
using System.Web.Mvc;
using CSF.Web.Mvc;
using Moq;
using System.Linq;

namespace Test.CSF.Web.Mvc
{
  [TestFixture]
  public class TestExtensibleModelBinder
  {
    #region test binding

    [Test]
    public void TestBindModel()
    {
      var modularBinder = new Mock<IModularModelBinder>(MockBehavior.Strict);
      var controllerContext = new Mock<ControllerContext>() { CallBase = true };
      var bindingContext = new Mock<ModelBindingContext>() { CallBase = true };

      bindingContext.Object.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(),
                                                              null,
                                                              null,
                                                              typeof(DateTime),
                                                              null);
      DateTime stubValue = DateTime.Today;

      modularBinder.Setup(x => x.CanBind(typeof(DateTime))).Returns(true);
      modularBinder.Setup(x => x.BindModel(controllerContext.Object, bindingContext.Object)).Returns(stubValue);

      var binder = new ExtensibleModelBinder();
      binder.RegisteredBinders = new IModularModelBinder[] { modularBinder.Object };

      var result = binder.BindModel(controllerContext.Object, bindingContext.Object);

      modularBinder.Verify(x => x.CanBind(typeof(DateTime)), Times.Once());
      modularBinder.Verify(x => x.BindModel(controllerContext.Object, bindingContext.Object), Times.Once());

      Assert.AreEqual(result, stubValue, "Correct return value");
    }

    [Test]
    public void TestBindModelFailure()
    {
      var modularBinder = new Mock<IModularModelBinder>(MockBehavior.Strict);
      var controllerContext = new Mock<ControllerContext>() { CallBase = true };
      var bindingContext = new Mock<ModelBindingContext>() { CallBase = true };

      bindingContext.Object.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(),
                                                              null,
                                                              null,
                                                              typeof(DateTime),
                                                              null);
      DateTime stubValue = DateTime.Today;

      modularBinder.Setup(x => x.CanBind(typeof(DateTime))).Returns(false);
      modularBinder.Setup(x => x.BindModel(controllerContext.Object, bindingContext.Object)).Returns(stubValue);

      var binder = new ExtensibleModelBinder();
      binder.RegisteredBinders = new IModularModelBinder[] { modularBinder.Object };

      var result = binder.BindModel(controllerContext.Object, bindingContext.Object);

      modularBinder.Verify(x => x.CanBind(typeof(DateTime)), Times.Once());
      modularBinder.Verify(x => x.BindModel(controllerContext.Object, bindingContext.Object), Times.Never());

      Assert.AreNotEqual(result, stubValue, "Correct return value");
    }

    #endregion

    #region test registration

    [Test]
    public void TestRegisterInstance()
    {
      var modularBinder1 = new Mock<IModularModelBinder>(MockBehavior.Strict);
      var modularBinder2 = new Mock<IModularModelBinder>(MockBehavior.Strict);

      var binder = new ExtensibleModelBinder();
      binder.Register(new IModularModelBinder[] { modularBinder1.Object, modularBinder2.Object });

      Assert.AreEqual(2, binder.RegisteredBinders.Count, "Count of binders");
      Assert.IsTrue(binder.RegisteredBinders.Contains(modularBinder1.Object), "Binder 1 registered");
      Assert.IsTrue(binder.RegisteredBinders.Contains(modularBinder2.Object), "Binder 2 registered");
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestRegisterDuplicateInstance()
    {
      var modularBinder1 = new Mock<IModularModelBinder>(MockBehavior.Strict);

      var binder = new ExtensibleModelBinder();
      binder.Register(new IModularModelBinder[] { modularBinder1.Object, modularBinder1.Object });
    }

    [Test]
    public void TestRegisterTypes()
    {
      var binder = new ExtensibleModelBinder();
      binder.Register(new Type[] { typeof(ModularBinder1), typeof(ModularBinder2) });

      Assert.AreEqual(2, binder.RegisteredBinders.Count, "Count of binders");
      Assert.IsTrue(binder.RegisteredBinders.Any(x => x.GetType() == typeof(ModularBinder1)), "Binder 1 registered");
      Assert.IsTrue(binder.RegisteredBinders.Any(x => x.GetType() == typeof(ModularBinder2)), "Binder 2 registered");
    }

    [Test]
    [Description("Diagnoses #4 - duplicate model binder types should be rejected.")]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestRegisterDuplicateTypes()
    {
      var binder = new ExtensibleModelBinder();
      binder.Register(new Type[] { typeof(ModularBinder1), typeof(ModularBinder1) });
    }

    #endregion

    #region contained types

    class ModularBinder1 : IModularModelBinder
    {
      public object BindModel(ControllerContext c, ModelBindingContext m) { throw new NotImplementedException(); }

      public bool CanBind(Type t) { throw new NotImplementedException(); }
    }

    class ModularBinder2 : IModularModelBinder
    {
      public object BindModel(ControllerContext c, ModelBindingContext m) { throw new NotImplementedException(); }

      public bool CanBind(Type t) { throw new NotImplementedException(); }
    }

    #endregion
  }
}

