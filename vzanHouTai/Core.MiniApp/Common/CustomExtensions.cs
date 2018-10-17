using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Core.MiniApp.Common
{
    public static class CustomExtensions
    {
        /// <summary>
        /// 转换为泛型INT列表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="splitChar">分割符</param>
        /// <returns></returns>
        public static List<int> ConvertToIntList(this string self, char splitChar)
        {
            return self?.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(str => str.All(thisChar => char.IsDigit(thisChar)))
                        .Select(str => int.Parse(str))
                        .ToList();
        }

        public static IEnumerable<IEnumerable<T>> Cartesian<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> tempProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(tempProduct,
                                         (accumulator, sequence) =>
                                            from accseq in accumulator
                                            from item in sequence
                                            select accseq.Concat(new[] { item})
                                       );
        }
    }
}
