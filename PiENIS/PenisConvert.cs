using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiENIS
{
    public static class PenisConvert
    {
        //TODO Make config class
        public static bool IgnoreCase { get; set; }

        private static readonly Type[] PrimitiveTypes = new[]
        {
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(bool),
            typeof(string)
        };

        public static string SerializeObject(object obj)
        {
            return Lexer.Unlex(Parser.Unparse(GetAtoms(obj).NotNull()));
        }

        private static IEnumerable<IAtom> GetAtoms(object o)
        {
            if (o is Array arr)
            {
                foreach (var item in arr)
                {
                    yield return GetAtom(null, item, arr.GetType().GetElementType());
                }

                yield break;
            }

            foreach (var member in o.GetType().GetPropsAndFields())
            {
                yield return GetAtom(member.Name, member.GetValue(o), member.GetMemberType());
            }
        }

        internal static IAtom GetAtom(string name, object value, Type typeIfNull = null)
        {
            var type = value?.GetType() ?? typeIfNull;

            if (type == null)
                throw new InvalidOperationException("Tried to serialize null.");

            if (value == null)
                return null;

            if (PrimitiveTypes.Contains(type))
            {
                return new KeyValueAtom(name, value);
            }
            else
            {
                return new ContainerAtom(name, type.IsArray, GetAtoms(value).NotNull().ToList());
            }
        }

        public static T DeserializeObject<T>(string str)
        {
            return (T)DeserializeObject(typeof(T), str);
        }

        public static object DeserializeObject(Type type, string str)
        {
            var lex = Lexer.Lex(str.SplitLines());
            var parsed = Parser.Parse(lex);

            return DeserializeObject(parsed, type);
        }

        internal static object DeserializeObject(IEnumerable<IAtom> atoms, Type type)
        {
            var instance = Activator.CreateInstance(type);
            var atomsList = new List<IAtom>(atoms);

            foreach (var member in GetPropsAndFields(type))
            {
                var atom = atomsList.SingleOrDefault(o => o.Key?.Equals(member.Name,
                        IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) ?? false);

                if (atom != null)
                {
                    atomsList.Remove(atom);

                    member.SetValue(instance, ConvertAtom(atom, member.GetMemberType()));
                }
            }

            return instance;
        }

        internal static object ConvertAtom(IAtom atom, Type targetType)
        {
            if (atom is KeyValueAtom kva)
            {
                if (targetType.IsEnum && kva.Value is string str)
                {
                    return Enum.Parse(targetType, str, IgnoreCase);
                }

                return kva.Value;
            }
            else if (atom is ContainerAtom container)
            {
                if (container.IsList)
                {
                    if (!targetType.IsArray)
                        throw new ConvertException("Found list in file but target field isn't an array.");

                    var itemType = targetType.GetElementType();
                    var items = container.Atoms.Select(o => ConvertAtom(o, itemType));

                    var array = (Array)Activator.CreateInstance(targetType, new object[] { container.Atoms.Count });

                    Array.Copy(items.ToArray(), array, array.Length);

                    return array;
                }
                else
                {
                    return DeserializeObject(container.Atoms, targetType);
                }
            }

            throw new ParserException("Invalid atom found.");
        }

        internal static void SetValue(this MemberInfo member, object on, object value)
        {
            if (member is PropertyInfo pi)
                pi.SetValue(on, value);
            else if (member is FieldInfo fi)
                fi.SetValue(on, value);
            else
                throw new Exception("Invalid member found");
        }

        internal static object GetValue(this MemberInfo member, object on)
        {
            if (member is PropertyInfo pi)
                return pi.GetValue(on);
            else if (member is FieldInfo fi)
                return fi.GetValue(on);
            else
                throw new Exception("Invalid member found");
        }

        internal static Type GetMemberType(this MemberInfo member)
        {
            return member is PropertyInfo pi ? pi.PropertyType :
                   member is FieldInfo fi ? fi.FieldType :
                   throw new Exception("Invalid member found");
        }

        internal static MemberInfo[] GetPropsAndFields(this Type type)
            => type.GetProperties().Where(o => o.CanWrite).Cast<MemberInfo>()
                  .Concat(type.GetFields().Cast<MemberInfo>())
                  .Where(o => o.GetCustomAttribute<DontPenisAttribute>() == null).ToArray();
    }
}
