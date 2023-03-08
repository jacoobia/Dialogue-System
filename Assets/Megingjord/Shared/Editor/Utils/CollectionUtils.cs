using System.Collections.Generic;
using System.Linq;

namespace Megingjord.Shared.Editor.Utils {
    /// <summary>
    /// Utilities involving collections to be used by the Megingjord tools
    /// </summary>
    public static class CollectionUtils {
        
        /// <summary>
        /// Get all elements AND their index from a collection
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) {
            return source.Select((item, index) => (item, index));
        }
    }
}