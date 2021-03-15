using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace K3 {
    static public class ReflectionUtility {

        static List<Assembly> gameAssemblies;

        static List<Assembly> overriddenAssemblies;
        // mostly used for testing.
        static public void OverrideAssemblyList(IEnumerable<Assembly> assemblies) {
            overriddenAssemblies = assemblies.ToList();
        }

        static public IEnumerable<Assembly> GetGameAssemblies(bool proprietaryOnly = true) {
            if (Application.isEditor) gameAssemblies = null; // special, we want to avoid having this cached if at all possible

            if (overriddenAssemblies != null) gameAssemblies = overriddenAssemblies; // ... but if an override is specified we want to use that regardless.

            if (gameAssemblies == null) {
                gameAssemblies = new List<Assembly>();
                // gameAssemblies.Add(Assembly.GetExecutingAssembly());
                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies()) {
                    if (proprietaryOnly && !IsProprietaryAssembly(ass)) continue;
                    gameAssemblies.Add(ass);
                }

                //var names = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
                //names = names.Where(ass => ass.Name.Contains("Panzerwehr") || ass.Name == "Assembly-CSharp").ToList();
                //foreach (var refass in names) gameAssemblies.Add(Assembly.Load(refass.ToString()));
            }
            return gameAssemblies;
        }

        static private bool IsProprietaryAssembly(Assembly ass) {
            var n = ass.FullName;
            if (n.Contains("Assembly-CSharp")) return true;
            if (n.Contains("System.")) return false;
            if (n.Contains("VisualStudio")) return false;
            if (n == "mscorlib" || n == "netstandard" || n == "Boo.Lang") return false;
            if (n.Contains("Microsoft.")) return false;
            if (n.Contains("UnityEditor")) return false;
            if (n.Contains("UnityEngine")) return false;
            if (n.Contains("UnityScript")) return false;
            return true;
        }

        static public Assembly GetUnityAssembly() => typeof(UnityEngine.Rigidbody).Assembly;

        public static IEnumerable<(T attribute, Type type)> GetTypeAttributesInProject<T>() where T : Attribute {
            var allAssemblies = GetGameAssemblies();
            foreach (var ass in allAssemblies) {
                foreach (var type in ass.GetTypes()) {
                    var attribs = type.GetCustomAttributes(typeof(T), true);
                    if (attribs.Length > 0) {
                        foreach (var attrib in attribs) yield return ((T)attrib, type);
                    }
                }
            }
        }

        public static IEnumerable<(T attr, MethodInfo info)> ListMethodAttributes<T>(Type sourceType) where T : Attribute {
            var allMethods = sourceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var m in allMethods) {
                var attr = m.GetCustomAttribute<T>();
                if (attr != null) yield return (attr, m);
            }
        }

        public static T GetMember<T>(object obj, string fieldName) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var field = obj.GetType().GetField(fieldName, flags);
            var value = field?.GetValue(obj);
            if (value != null) return (T)value;
            var property = obj.GetType().GetProperty(fieldName, flags);
            var val = property.GetValue(obj);
            return (T)val;


        }

        public static IEnumerable<Type> FindImplementingTypesInAssembly(Type t, Assembly ass, bool concreteOnly) {
            foreach (var type in ass.GetTypes()) {
                if (concreteOnly && type.IsAbstract) continue;
                if (t.IsAssignableFrom(type)) yield return type;
            }
        }

        public static IEnumerable<Type> FindImplementingTypesInProject(Type t, bool concreteOnly) {
            foreach (var ass in GetGameAssemblies()) {
                foreach (var type in ass.GetTypes()) {
                    if (concreteOnly && type.IsAbstract) continue;
                    if (t.IsAssignableFrom(type)) yield return type;
                }
            }
        }

        public static IEnumerable<Type> FindImplementingTypesInProject<T>(bool concreteOnly) => FindImplementingTypesInProject(typeof(T), concreteOnly);

        /// <remarks>https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class</remarks>
        static public bool IsSubclassOfRawGeneric(this Type toCheck, Type genericType) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (genericType == cur) return true;
                toCheck = toCheck.BaseType;
            }
            return false;
        }
        /// <summary>If you have a class and you're interested whether it inherits a generic (say, List[]) but are not concerned with generic parameter </summary>
        static public Type GetTypeInInheritanceHierarchyThatIsImplementationOfRawGeneric(this Type toCheck, Type genericType) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (genericType == cur) return toCheck;
                toCheck = toCheck.BaseType;
            }
            return null;
        }
    }
}
