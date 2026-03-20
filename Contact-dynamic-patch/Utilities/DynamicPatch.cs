using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ContactApi.Utilities
{
    public sealed class PatchOptions
    {
        public ISet<string> Blocked { get; init; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Id", "CreatedAt", "UpdatedAt" };

        public ISet<string>? Allowed { get; init; } = null;
        public bool RejectUnknown { get; init; } = false;
        public bool CaseInsensitive { get; init; } = true;
        public bool RespectJsonPropertyAttribute { get; init; } = true;

        // 🔹 New: whether to remove items not present in patch
        public bool SyncCollections { get; init; } = false;
    }

    public static class DynamicPatch
    {
        public static void Apply(object target, JObject patch, PatchOptions? options = null)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (patch is null) throw new ArgumentNullException(nameof(patch));

            options ??= new PatchOptions();
            ApplyObject(target, patch, options, parentPath: "");
        }

        private static void ApplyObject(object target, JObject patch, PatchOptions options, string parentPath)
        {
            var type = target.GetType();
            var props = GetPropertyMap(type, options);

            foreach (var jprop in patch.Properties())
            {
                var jsonName = jprop.Name;
                var fullPath = string.IsNullOrEmpty(parentPath) ? jsonName : $"{parentPath}.{jsonName}";

                if (options.Blocked.Contains(jsonName) || options.Blocked.Contains(fullPath)) continue;
                if (options.Allowed is not null && !options.Allowed.Contains(jsonName) && !options.Allowed.Contains(fullPath)) continue;

                if (!props.TryGetValue(jsonName, out var pi))
                {
                    if (options.RejectUnknown) throw new InvalidOperationException($"Unknown field: {fullPath}");
                    continue;
                }

                if (!pi.CanWrite) continue;

                var propType = pi.PropertyType;
                var underlying = Nullable.GetUnderlyingType(propType) ?? propType;
                var token = jprop.Value;

                if (token.Type == JTokenType.Null)
                {
                    if (Nullable.GetUnderlyingType(propType) != null || !propType.IsValueType)
                        pi.SetValue(target, null);
                    continue;
                }

                // 🔹 Handle collections (e.g. Addresses)
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying) &&
                    underlying.IsGenericType)
                {
                    ApplyCollection(pi, target, token, options, fullPath);
                    continue;
                }

                // 🔹 Nested object
                if (token is JObject childObj && !IsSimple(underlying))
                {
                    var current = pi.GetValue(target);
                    if (current == null)
                    {
                        current = Activator.CreateInstance(underlying)!;
                        pi.SetValue(target, current);
                    }
                    ApplyObject(current, childObj, options, fullPath);
                    continue;
                }

                // 🔹 Simple value or enum
                object? value;
                if (underlying.IsEnum)
                {
                    if (token.Type == JTokenType.String &&
                        Enum.TryParse(underlying, token.Value<string>()!, true, out var parsed))
                        value = parsed;
                    else
                        value = token.ToObject(underlying);
                }
                else
                {
                    value = token.ToObject(underlying);
                }

                pi.SetValue(target, value);
            }
        }

        private static void ApplyCollection(PropertyInfo pi, object target, JToken token, PatchOptions options, string fullPath)
        {
            if (token is not JArray arr) return;

            var itemType = pi.PropertyType.GetGenericArguments()[0];
            var currentList = pi.GetValue(target) as System.Collections.IList;
            if (currentList == null)
            {
                var listType = typeof(List<>).MakeGenericType(itemType);
                currentList = (System.Collections.IList)Activator.CreateInstance(listType)!;
                pi.SetValue(target, currentList);
            }

            var newIds = new HashSet<int>();

            foreach (var jitem in arr.Children<JObject>())
            {
                var idProp = jitem.Property("id", StringComparison.OrdinalIgnoreCase);
                if (idProp != null && idProp.Value.Type == JTokenType.Integer)
                {
                    var id = idProp.Value.Value<int>();
                    newIds.Add(id);

                    var existing = currentList.Cast<object>()
                        .FirstOrDefault(x =>
                        {
                            var idPi = x.GetType().GetProperty("Id");
                            return idPi != null && (int)idPi.GetValue(x)! == id;
                        });

                    if (existing != null)
                    {
                        ApplyObject(existing, jitem, options, fullPath + $"[{id}]");
                        continue;
                    }
                }

                // Add new item
                var newItem = Activator.CreateInstance(itemType)!;
                ApplyObject(newItem, jitem, options, fullPath + "[]");
                currentList.Add(newItem);
            }

            // Remove missing if SyncCollections is true
            if (options.SyncCollections)
            {
                var toRemove = currentList.Cast<object>()
                    .Where(x =>
                    {
                        var idPi = x.GetType().GetProperty("Id");
                        if (idPi == null) return false;
                        var id = (int)idPi.GetValue(x)!;
                        return !newIds.Contains(id);
                    })
                    .ToList();

                foreach (var item in toRemove)
                    currentList.Remove(item);
            }
        }

        private static bool IsSimple(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
                || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(TimeSpan);
        }

        private static Dictionary<string, PropertyInfo> GetPropertyMap(Type type, PatchOptions options)
        {
            var dict = new Dictionary<string, PropertyInfo>(
                options.CaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

            foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!pi.CanRead) continue;

                var jsonName = pi.Name;

                if (options.RespectJsonPropertyAttribute)
                {
                    var attr = pi.GetCustomAttribute<JsonPropertyAttribute>();
                    if (!string.IsNullOrWhiteSpace(attr?.PropertyName))
                        jsonName = attr.PropertyName!;
                }

                if (!dict.ContainsKey(jsonName))
                    dict.Add(jsonName, pi);
            }
            return dict;
        }
    }
}
