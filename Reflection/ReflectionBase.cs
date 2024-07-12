using System;

namespace Seion.Iof.Reflection
{
    internal class ReflectionBase
    {
        public object ReflectedInstance { get; set; }
        public Type ReflectedType { get; set; }
        public T GetFieldValue<T>(string name)
        {
            return ReflectedInstance.GetFieldValue<T>(name);
        }

        public object GetFieldValue(string name)
        {
            return ReflectedInstance.GetFieldValue(name);
        }

        public T GetPropertyValue<T>(string name)
        {
            return ReflectedInstance.GetPropertyValue<T>(name);
        }

        public object GetPropertyValue(string name)
        {
            return ReflectedInstance.GetPropertyValue(name);
        }

        public T InvokeMethod<T>(string name, object[] args = null, Type[] methodArgTypes = null)
        {
            return ReflectedInstance.InvokeMethod<T>(name, args, methodArgTypes);
        }

        public object InvokeMethod(string name, object[] args = null, Type[] methodArgTypes = null)
        {
            return ReflectedInstance.InvokeMethod(name, args, methodArgTypes);
        }
    }
}
