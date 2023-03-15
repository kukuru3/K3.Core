using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace K3 {
    static public class FactoryUtility {
        public abstract class Factory<TKey,TValue> {
            protected Dictionary<TKey, Func<TValue>> alreadyCached = new();

            public TValue ProduceInstance(TKey key) {
                if (!alreadyCached.TryGetValue(key, out var factoryMethod)) {
                    factoryMethod = GenerateFactoryMethod(key);
                    alreadyCached.Add(key, factoryMethod);
                }
                return factoryMethod();
            }
            protected abstract Func<TValue> GenerateFactoryMethod(TKey key);
        }

        public class LocatorFactory<TValue> : Factory<Type, TValue> {
            protected override Func<TValue> GenerateFactoryMethod(Type type) {
                var ctor = type.GetConstructor(Array.Empty<Type>());
                var ctorExpression = Expression.New(ctor);
                var lambda = Expression.Lambda<Func<TValue>>(ctorExpression);
                var factoryMethod = lambda.Compile();
                return factoryMethod;
            }
        }

        public static Func<T> GenerateConstructorInvoker<T>() {            
            var ctor = typeof(T).GetConstructor(Array.Empty<Type>());
            var ctorExpression = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<T>>(ctorExpression);
            var constructorInvoker = lambda.Compile();
            return constructorInvoker;
        }

        public static Func<object> GenerateConstructorInvoker(Type type) {
            var ctor = type.GetConstructor(Array.Empty<Type>());
            var ctorExpression = Expression.New(ctor);
            var lambda = Expression.Lambda<Func<object>>(ctorExpression);
            var constructorInvoker = lambda.Compile();
            return constructorInvoker;
        }
    }
}
