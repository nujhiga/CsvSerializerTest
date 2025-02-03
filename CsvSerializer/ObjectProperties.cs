using CsvSerializer.Filtering;

using System.Collections.ObjectModel;
using System.Reflection;

namespace CsvSerializer;



internal sealed class ObjectProperty(PropertyInfo pinfo, int index, bool? isKey = null!, bool? ignore = null!)
{
    public readonly PropertyInfo Pinfo = pinfo;
    public readonly Type Ptype = Nullable.GetUnderlyingType(pinfo.PropertyType) ?? pinfo.PropertyType;
    public readonly string Pname = pinfo.Name;
    public readonly int Pindex = index;
    public readonly bool Filter = ignore is not null;
    public readonly bool Ignore = ignore is not null && ignore.Value;

    public override string ToString()
    {
        return $"{Pname}, {Ptype}, {Pindex}, {Filter}, {Ignore}";
    }
}

internal sealed class ObjectProperties : Collection<ObjectProperty>
{
    internal static ObjectProperties CreateProperties<T>(SerializationOptions options, char delimiter) where T : class
    {
        Type type = typeof(T);
        IEnumerable<ObjectProperty> properties = CreateProperties(type, options);
        bool requireFormat = delimiter is ',' && RequireFormatting(properties);
        
        ObjectProperties oProperties = new ObjectProperties(type.Name, requireFormat);

        foreach (var p in properties)
            oProperties.Items.Add(p);

        return oProperties;
    }
    private static IEnumerable<ObjectProperty> CreateProperties(Type type, SerializationOptions options)
    {
        var baseProps = type.GetProperties(options.Flags);

        for (int i = 0; i < baseProps.Length; i++)
        {
            var bProp = baseProps[i];

            if (options.Filter.IsNone)
            {
                yield return new ObjectProperty(bProp, i);
            }               
            else
            {
                string pName = bProp.Name;
                Type pType = bProp.PropertyType;

                bool filterFlag = options.Filter.Scope is FilterScope.Name ?
                    options.FilterNames.Contains(pName) : options.FilterTypes.Contains(pType);

                bool ignore;

                if (options.Filter.Mode is FilterMode.Ignore)
                {
                    ignore = filterFlag;
                }
                else
                {
                    ignore = !filterFlag;
                }

                yield return new ObjectProperty(bProp, i, ignore);
            }
        }
    }
    private ObjectProperties(string objTypeName, bool requireFormat)
    {
        _objTypeName = objTypeName;
        RequireFormat = requireFormat;
    }

    private readonly string _objTypeName;
    public readonly bool RequireFormat;
    public int GetCount()
    {
        int cnt = 0;
        foreach(var p in Items)
        {
            if (p.Ignore) continue;
            cnt++;
        }
        return cnt;
    }
    public IEnumerable<ObjectProperty> AsFiltered()
    {
        foreach(var p in Items)
        {
            if (p.Filter && !p.Ignore)
                yield return p;
        }
    }

    public string[] AsHeaders()
    {
        string[] headers = new string[Items.Count];

        for (int i = 0; i < headers.Length; i++)
            headers[i] = Items[i].Pname;

        return [.. headers];
    }

    public ObjectProperty this[string pname]
    {
        get
        {
            foreach (var p in Items)
                if (p.Pname == pname)
                    return p;

            return null!;
        }
    }

    public Type GetPropType(int idx)
    {
        return this[idx].Ptype;
    }
    public Type GetPropType(string pname)
    {
        return this[pname].Ptype;
    }

    public PropertyInfo GetPropInfo(int idx)
    {
        return this[idx].Pinfo;
    }
    public PropertyInfo GetPropInfo(string pname)
    {
        return this[pname].Pinfo;
    }
    public int GetPropIndex(string pname)
    {
        return this[pname].Pindex;
    }

    private static bool RequireFormatting(IEnumerable<ObjectProperty> properties)
    {
        foreach (var prop in properties)
        {
            if (prop.Ptype == typeof(decimal) ||
                prop.Ptype == typeof(double) ||
                prop.Ptype == typeof(float))
            {
                return true;
            }
        }
        return false;
    }
}



//internal sealed class ObjectProperties : Collection<PropertyInfo>
//{
//    internal static ObjectProperties CreateProperties<T>(SerializationOptions options, char delimiter) where T : class
//    {
//        Type type = typeof(T);
//        IEnumerable<PropertyInfo> properties = type.GetProperties(options.Flags);

//        //if (!options.Filter.IsNone)
//        //    properties = properties.Where(GetFilterPredicate(options));

//        //bool requireFormat = delimiter is ',' && RequireFormatting(properties);

//        //ObjectProperties objProperties = new(requireFormat, type.Name);

//        //foreach (var p in properties)
//        //    objProperties.Items.Add(p);

//        return new ObjectProperties(type.Name, properties, delimiter, options);
//    }

//    private IEnumerable<PropertyInfo> FilterProperties(IEnumerable<PropertyInfo> properties, SerializationOptions options)
//    {
//        FilterIndexes = [];
//        FilterNames = [];
//        var propsArray = properties.ToArray();

//        for (int i = 0; i < propsArray.Length; i++)
//        {
//            string pName = propsArray[i].Name;
//            Type pType = propsArray[i].PropertyType;

//            bool filterFlag = options.Filter.Scope is FilterScope.Name ?
//                options.FilterNames.Contains(pName) : options.FilterTypes.Contains(pType);

//            if (options.Filter.Mode is FilterMode.Ignore && filterFlag)
//            {
//                FilterIndexes.Add(i);
//                FilterNames.Add(pName);
//                continue;
//            }
//            else if (options.Filter.Mode is FilterMode.Exclusive && !filterFlag)
//            {
//                FilterIndexes.Add(i);
//                FilterNames.Add(pName);
//                continue;
//            }

//            yield return propsArray[i];
//            //if (options.Filter.Scope is FilterScope.Name)
//            //{
//            //    filterFlag = options.FilterNames.Contains(pName);
//            //    if (options.Filter.Mode is FilterMode.Ignore && filterFlag)
//            //    {
//            //        FilterIndexes.Add(i);
//            //        FilterNames.Add(pName);
//            //        continue;
//            //    }
//            //    else if (options.Filter.Mode is FilterMode.Exclusive && !filterFlag)
//            //    {
//            //        FilterIndexes.Add(i);
//            //        FilterNames.Add(pName);
//            //        continue;
//            //    }
//            //}
//            //else
//            //{
//            //    filterFlag = options.FilterTypes.Contains(pType);
//            //    if (options.Filter.Mode is FilterMode.Ignore && filterFlag)
//            //    {
//            //        FilterIndexes.Add(i);
//            //        FilterNames.Add(pName);
//            //        continue;
//            //    }
//            //    else if (options.Filter.Mode is FilterMode.Exclusive && !filterFlag)
//            //    {
//            //        FilterIndexes.Add(i);
//            //        FilterNames.Add(pName);
//            //        continue;
//            //    }
//            //}
//        }
//    }

//    private ObjectProperties(string objTypeName, IEnumerable<PropertyInfo> properties, char delimiter, SerializationOptions options)
//    {
//        _objTypeName = objTypeName;

//        var objProperties = options.Filter.IsNone ?
//            properties : FilterProperties(properties, options);

//        { }
//        foreach (var p in objProperties)
//            Items.Add(p);

//        RequireFormat = delimiter is ',' && RequireFormatting(objProperties);
//    }

//    public PropertyInfo this[string propName]
//    {
//        get
//        {
//            foreach (var prop in Items)
//                if (prop.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
//                    return prop;

//            throw new ArgumentException($"Property '{propName}' not found in type {_objTypeName}.");
//        }
//    }

//    public readonly bool RequireFormat;
//    private readonly string _objTypeName;

//    public Collection<int> FilterIndexes { get; private set; } = null!;
//    public Collection<string> FilterNames { get; private set; } = null!;

//    public Type GetPropType(int index)
//    {
//        var prop = this[index];
//        return Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
//    }
//    public Type GetPropType(string propName)
//    {
//        var prop = this[propName];
//        return Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
//    }
//    public string GetPropName(int index) => this[index].Name;
//    public string[] AsHeaders()
//    {
//        string[] headers = new string[Items.Count];

//        for (int i = 0; i < headers.Length; i++)
//            headers[i] = Items[i].Name;

//        return headers;
//    }
//    public int GetPropIndex(string propName)
//    {
//        for (int i = 0; i < Items.Count; i++)
//            if (this[i].Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
//                return i;

//        return -1;
//    }
//    private static Func<PropertyInfo, bool> GetFilterPredicate(SerializationOptions options)
//    {
//        return (options.Filter.Scope, options.Filter.Mode) switch
//        {
//            (FilterScope.Name, FilterMode.Ignore) =>
//                p => !options.FilterNames!.Contains(p.Name),

//            (FilterScope.Name, FilterMode.Exclusive) =>
//                p => options.FilterNames!.Contains(p.Name),

//            (FilterScope.Type, FilterMode.Ignore) =>
//                p => !options.FilterTypes!.Contains(p.PropertyType),

//            (FilterScope.Type, FilterMode.Exclusive) =>
//                p => options.FilterTypes!.Contains(p.PropertyType),

//            _ => throw new NotSupportedException("Unsupported filter configuration.")
//        };
//    }
//    private static bool RequireFormatting(IEnumerable<PropertyInfo> properties)
//    {
//        foreach (var prop in properties)
//        {
//            if (prop.PropertyType == typeof(decimal) ||
//                prop.PropertyType == typeof(double) ||
//                prop.PropertyType == typeof(float))
//            {
//                return true;
//            }
//        }
//        return false;
//    }
//}
