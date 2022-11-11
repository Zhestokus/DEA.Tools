using DEA.Core;
using DEA.Tools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DEA.Tools.Helpers
{
    public class EventWrapper
    {
        private readonly bool _singleton;

        private readonly Type _objectType;
        private readonly Object _syncLock;

        private readonly Object[] _emptyObjArray;

        private Object _objInstance;

        private ISet<String> _eventNames;

        private ILookup<String, MethodInfo> _methodsLp;
        private IDictionary<String, Type> _objectsTypes;

        public EventWrapper(Type objectType, bool singleton)
        {
            _objectType = objectType;
            _singleton = singleton;

            _syncLock = new Object();

            _emptyObjArray = new Object[0];

            _objectsTypes = new Dictionary<String, Type>();
        }

        public bool Singleton => _singleton;

        public void Initialize()
        {
            lock (_syncLock)
            {
                if (_methodsLp != null)
                    return;

                var methods = _objectType.GetMethods();

                var methodsQuery = (from n in methods
                                    where Attribute.IsDefined(n, typeof(DeaEventAttribute))
                                    let name = CreateMethodKey(n)
                                    select new
                                    {
                                        Key = name,
                                        Val = n,
                                    });

                var paramsQuery = (from n in methods
                                   where Attribute.IsDefined(n, typeof(DeaEventAttribute))
                                   let name = CreateMethodKey(n)
                                   from p in n.GetParameters()
                                   select new
                                   {
                                       Key = name,
                                       Val = p,
                                   });

                _methodsLp = methodsQuery.ToLookup(n => n.Key, n => n.Val);

                _eventNames = _methodsLp.Select(n => n.Key).ToHashSet();

                //foreach (var grp in _methodsLp)
                //{
                //    if (grp.Count() > 1)
                //        throw new Exception($"Event {_objectType.FullName}.{grp.Key} have more then one handler");
                //}
            }
        }

        public ISet<String> GetEvents()
        {
            var @set = _eventNames.ToHashSet();
            return @set;
        }

        public bool ContainsEvent(String eventName)
        {
            Initialize();

            return _methodsLp[eventName].Any();
        }

        public Object InvokeEvent(String eventName)
        {
            return InvokeEvent(eventName, _emptyObjArray);
        }
        public Object InvokeEvent(String eventName, Object[] @params)
        {
            Initialize();

            var objInstance = GetObjectInstance();
            //var paramValues = GetParamValues(@params);

            var methodInfos = _methodsLp[eventName];
            foreach (var methodInfo in methodInfos)
            {
                if (!IsParametersEquals(methodInfo, @params))
                    continue;

                var obj = methodInfo.Invoke(objInstance, @params);
                return obj;
            }

            throw new MissingMethodException(eventName);
        }

        public IEnumerable<ParameterInfo[]> GetEventParams(String eventName)
        {
            var methodInfos = _methodsLp[eventName];
            foreach (var methodInfo in methodInfos)
            {
                var parameters = methodInfo.GetParameters();
                yield return parameters;
            }
        }

        private bool IsParametersEquals(MethodInfo methodInfo, Object[] @params)
        {
            var parameters = methodInfo.GetParameters();
            if (parameters.Length != @params.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramInfo = parameters[i];
                var paramValue = @params[i];

                if (paramValue == null)
                {
                    if (!IsNullable(paramInfo.ParameterType))
                        return false;
                }
                else
                {
                    var underlyingType = GetUnderlyingType(paramInfo.ParameterType);
                    if (!underlyingType.Equals(paramValue.GetType()))
                        return false;
                }
            }

            return true;
        }

        //private Object[] GetParamValues(Object[] @params)
        //{
        //    if (@params == null)
        //        return null;

        //    if (@params.Length == 0)
        //        return new Object[0];

        //    var values = new Object[@params.Length];

        //    for (var i = 0; i < @params.Length; i++)
        //    {
        //        values[i] = @params[i];

        //        if (values[i] is NullOf)
        //            values[i] = null;
        //    }

        //    return values;
        //}
        //private Type[] GetGenericTypes(String[] generics)
        //{
        //    if (generics == null)
        //        return null;

        //    if (generics.Length == 0)
        //        return new Type[0];

        //    var values = new Type[generics.Length];

        //    for (var i = 0; i < generics.Length; i++)
        //        values[i] = FindType(generics[i]);

        //    return values;
        //}

        private Type FindType(String fullName)
        {
            lock (_objectsTypes)
            {
                if (!_objectsTypes.TryGetValue(fullName, out var type))
                {
                    var query = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                 from tp in asm.GetTypes()
                                 where tp.FullName == fullName
                                 select tp);

                    type = query.FirstOrDefault();
                    if (type != null)
                        _objectsTypes[fullName] = type;
                }

                return type;
            }

        }

        private String CreateMethodKey(MethodInfo methodInfo)
        {
            var name = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";

            var attr = methodInfo.GetCustomAttribute<DeaEventAttribute>();
            if (!String.IsNullOrEmpty(attr.EventName))
                name = attr.EventName;

            return name;
        }

        private Object GetObjectInstance()
        {
            lock (_syncLock)
            {
                if (_singleton)
                {
                    if (_objInstance == null)
                        _objInstance = Activator.CreateInstance(_objectType);

                    return _objInstance;
                }

                var instance = Activator.CreateInstance(_objectType);
                return instance;

            }
        }

        private Type GetUnderlyingType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                return underlyingType;

            return type;
        }

        private bool IsNullable(Type type)
        {
            return (Nullable.GetUnderlyingType(type) != null);
        }

    }
}
