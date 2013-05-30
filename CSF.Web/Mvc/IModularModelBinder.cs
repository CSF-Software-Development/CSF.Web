using System;

namespace CSF.Web.Mvc
{
  /// <summary>
  /// Interface for a 'modular' model binder - a model binder that may bind model instances based upon their type.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This interface is designed to be used in conjunction with the <see cref="ExtensibleModelBinder"/>, it provides
  /// all of the functionality of a regular MVC model binder.  Additionally, the <see cref="CanBind"/> method allows the
  /// inclusion of logic (which may be as simple or complex as required) that determines whether or not this binder may
  /// be used to bind a model of a given type.
  /// </para>
  /// </remarks>
  public interface IModularModelBinder : System.Web.Mvc.IModelBinder
  {
    /// <summary>
    /// Determines whether this instance can bind a model of the specified type.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance can bind the specified model-type; otherwise, <c>false</c>.
    /// </returns>
    /// <param name='modelType'>
    /// The model type that is to be bound.
    /// </param>
    bool CanBind(Type modelType);
  }
}

