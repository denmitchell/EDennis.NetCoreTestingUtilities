using System.Collections.Generic;
using System.Dynamic;

namespace EDennis.NetCoreTestingUtilities.Extensions {

    /// <summary>
    /// This class can be used to convert... 
    /// <list type="bullet">
    /// <item>A dynamic object to a Dictionary of property values</item>
    /// <item>An IEnumerable of dynamic objects to a List of Dictionaries of property values</item>
    /// </list>
    /// The converted values can, in turn, be deserialized into (perhaps partially filled) objects 
    /// </summary>
    public static class DynamicConverter {

        public static Dictionary<string, object> ToPropertyDictionary(dynamic obj) {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)expando;

            foreach (var property in obj.GetType().GetProperties())
                try {
                    dictionary.Add(property.Name, property.GetValue(obj));
                } catch { }
            return new Dictionary<string, object>(expando);
        }

        public static List<Dictionary<string, object>> ToPropertyDictionaryList(IEnumerable<dynamic> list) {
            var dlist = new List<Dictionary<string, object>>();
            foreach (var item in list) {
                dlist.Add(ToPropertyDictionary(item));
            }
            return dlist;
        }

    }
}
