using System;
using UnityEditor;
using UnityEngine;

namespace Megingjord.Shared.Helpers {
    /// <summary>
    /// Warnings and errors for Megingjord and the option to throw,
    /// show a UI popup or print to the console
    /// </summary>
    public class MWarn {
        
        private const string ErrorTitle = "Error!";
        private const string WarnTitle = "Warning!";
        private const string Accept = "Ok";

        private readonly string _message;
        private readonly Scope _scope;
        
        private MWarn(string message, Scope scope) {
            _message = message;
            _scope = scope;
        }

        /// <summary>
        /// Shows a UI popup/dialogue
        /// </summary>
        public void Show() {
            EditorUtility.DisplayDialog(_scope == Scope.Error ? ErrorTitle : WarnTitle, _message, Accept);
        }

        /// <summary>
        /// Prints an error or a warning to the console
        /// </summary>
        public void Print() {
            if (_scope == Scope.Error) {
                Debug.LogError(_message);
            } else {
                Debug.LogWarning(_message);
            }
        }

        /// <summary>
        /// Throws an exception 
        /// </summary>
        /// <typeparam name="T">The exception to throw</typeparam>
        /// <exception cref="T"></exception>
        public void Throw<T>() where T : Exception {
             throw (T)Activator.CreateInstance(typeof(T), _message);
        }

        public static readonly MWarn AmbiguousPropertyName = new("Ambiguous call to set property, more than one with given name exists.", Scope.Error);
        public static readonly MWarn UnsuccessfulBoolParse = new("Unable to parse property to bool.", Scope.Error);
        public static readonly MWarn InvalidStringPropertyName = new("There is no string property with that name.", Scope.Error);
        public static readonly MWarn InvalidPropertyName = new("There is no property with that name.", Scope.Error);
        public static readonly MWarn InvalidPropertyType = new("Invalid property value entered.", Scope.Warning);
        public static readonly MWarn PropertyExists = new("A property with that name already exists!", Scope.Error);
        public static readonly MWarn CouldNotFindRoot = new("Could not find root directory for Megingjord, please reinstall the asset!", Scope.Error);
        public static readonly MWarn ActorNotSpecified = new("There is no actor specified, this may cause issues if you are using focusing in the current dialogue.", Scope.Warning);
        public static readonly MWarn PlayerNotFound = new("The player passed into the SetPlayer function was null.", Scope.Error);

        private enum Scope {
            Warning, Error
        }
        
    }
}