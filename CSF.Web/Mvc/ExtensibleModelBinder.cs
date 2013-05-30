using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;

namespace CSF.Web.Mvc
{
  /// <summary>
  /// Extensible model binder; attempts to bind a model using one of many model-binder extensions registered with this
  /// type.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This implementation of a model binder does not contain any functionality for directly binding models, rather it is
  /// designed to be extended by registering types that implement <see cref="IModularModelBinder"/>.  These modular
  /// binders are then responsible for binding the actual models.
  /// </para>
  /// <para>
  /// The benefit of this solution over the regular ASP.NET MVC model binding mechanism is that logic may be specified
  /// within each extensible binder to determine whether that binder may bind a model of any given type or not.  This
  /// allows (for example) a single binder to bind many model types that all match an 'open generic' type.
  /// </para>
  /// <para>
  /// Credit where due, this work is based heavily upon
  /// http://lostechies.com/jimmybogard/2009/03/18/a-better-model-binder/
  /// </para>
  /// </remarks>
  public class ExtensibleModelBinder : DefaultModelBinder
  {
    #region fields

    private ICollection<IModularModelBinder> _binders;

    #endregion

    #region properties

    /// <summary>
    /// Gets or sets a collection of the registered modular model binders.
    /// </summary>
    /// <value>
    /// The registered binders.
    /// </value>
    public ICollection<IModularModelBinder> RegisteredBinders
    {
      get {
        return _binders;
      }
      set {
        if(value == null)
        {
          throw new ArgumentNullException("value");
        }

        _binders = value;
      }
    }

    #endregion

    #region methods

    /// <summary>
    /// Performs the model binding operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method first attempts to find an appropriate <see cref="IModularModelBinder"/> instance, which has been
    /// previously registered with this type.  If such a modular binder is found then it is used to bind the model and
    /// the result is returned.  If no such binder is found then a null reference is returned, to indicate that this
    /// model binder cannot bind the desired model type.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An object created via the model-binding process.
    /// </returns>
    /// <param name='controllerContext'>
    /// The controller context.
    /// </param>
    /// <param name='bindingContext'>
    /// The binding context.
    /// </param>
    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      object output;
      IModularModelBinder binder = this.SelectBinder(bindingContext.ModelType);

      if(binder != null)
      {
        output = binder.BindModel(controllerContext, bindingContext);
      }
      else
      {
        output = null;
      }

      return output;
    }

    /// <summary>
    /// Register the specified binders with this instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each of the instances that implement <see cref="IModularModelBinder"/> are added to
    /// <see cref="RegisteredBinders"/>.  As this is done, null and duplicate-checking is performed, throwing exceptions
    /// if appropriate.
    /// </para>
    /// </remarks>
    /// <param name='binders'>
    /// A collection of the modular binders to register.
    /// </param>
    public void Register(IEnumerable<IModularModelBinder> binders)
    {
      if(binders == null)
      {
        throw new ArgumentNullException("binders");
      }

      foreach(IModularModelBinder binder in binders)
      {
        if(binder == null)
        {
          throw new ArgumentException("Binders collection must not include null references.", "binders");
        }
        else if(this.RegisteredBinders.Contains(binder))
        {
          throw new InvalidOperationException("Binders collection must not contain instances that are already " +
                                              "registered, including duplicate instances of the same extensible " +
                                              "binder type.");
        }

        this.RegisteredBinders.Add(binder);
      }
    }

    /// <summary>
    /// Enumerates a collection of <c>System.Type</c> and registers each of them within this instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each of the types given within the <paramref name="binderTypes"/> collection must implement
    /// <see cref="IModularModelBinder"/> and must have a default parameterless constructor, or an exception will be
    /// thrown.
    /// </para>
    /// <para>
    /// This method instantiates each of those types and registers each instance as a modular model binder within the
    /// current instance.
    /// </para>
    /// </remarks>
    /// <param name='binderTypes'>
    /// A collection of types, which must all implement <see cref="IModularModelBinder"/>.
    /// </param>
    public void Register(IEnumerable<Type> binderTypes)
    {
      ICollection<IModularModelBinder> binders = new List<IModularModelBinder>();

      if(binderTypes == null)
      {
        throw new ArgumentNullException("binderTypes");
      }

      foreach(Type type in binderTypes)
      {
        IModularModelBinder binder = (IModularModelBinder) Activator.CreateInstance(type);
        binders.Add(binder);
      }

      this.Register(binders);
    }

    /// <summary>
    /// Selects an appropriate modular model binder to deal with a given model type.
    /// </summary>
    /// <returns>
    /// A single <see cref="IModularModelBinder"/> instance, or a null reference if no suitable binder could be found.
    /// </returns>
    /// <param name='modelType'>
    /// The type of model to bind.
    /// </param>
    private IModularModelBinder SelectBinder(Type modelType)
    {
      return this.RegisteredBinders.FirstOrDefault(x => x.CanBind(modelType));
    }

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CSF.Web.Mvc.ExtensibleModelBinder"/> class.
    /// </summary>
    public ExtensibleModelBinder ()
    {
      this.RegisteredBinders = new List<IModularModelBinder>();
    }

    #endregion
  }
}
