using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lymm37.PotionCraft.RecipeMapPlayback
{
    class ReflectionHelper
    {
        // Helper functions to access private fields, properties, and methods

        public static object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method is not null)
            {
                return method.Invoke(instance, parameters);
            }
            return null;
        }

        public static T GetPrivateField<T>(object instance, string fieldName)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)prop.GetValue(instance);
        }

        public static T GetInternalField<T>(object instance, string fieldName)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.Default);
            return (T)prop.GetValue(instance);
        }

        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(instance, value);
        }

        public static T GetPrivateProperty<T>(object instance, string propertyName)
        {
            var prop = instance.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)prop.GetValue(instance);
        }
    }
}
