using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DncZeus.Api.Utils
{
    public static class DecimalRangeHelper
    {
        /// <summary>
        /// 是否有交集
        /// </summary>
        /// <param name="currentRange"></param>
        /// <param name="otherRange"></param>
        /// <returns></returns>
        public static bool IsIntersectionWith(this DecimalRange currentRange, DecimalRange otherRange)
        {
            if (currentRange.Min == currentRange.Max)
            {
                return true;
            }
            if (otherRange.Min == otherRange.Max)
            {
                return true;
            }
            if (otherRange.Min == currentRange.Max)
            {
                return true;
            }
            else
            {
                return currentRange.Min.In(otherRange.Min, otherRange.Max) || currentRange.Max.In(otherRange.Min, otherRange.Max);
            }
        }

        /// <summary>
        /// 判断金额区间存在交集
        /// </summary>
        /// <param name="currentRanges"></param>
        /// <returns></returns>
        public static bool ExistsIntersectionRange(this List<DecimalRange> currentRanges)
        {
            return currentRanges.Any(p => currentRanges.Where(q => !object.ReferenceEquals(p, q)).Any(z => p.IsIntersectionWith(z)));
        }
    }

    /// <summary>
    /// 金额区间对应类 
    /// </summary>
    public class DecimalRange
    {
        /// <summary>
        /// 最大
        /// </summary>
        private decimal max;

        /// <summary>
        /// 最小值
        /// </summary>
        public decimal Min { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public decimal Max
        {
            get
            {
                return (max == 0) ? Decimal.MaxValue : max;
            }
            set
            {
                max = value;
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Min.ToString() + "-" + Max.ToString();
        }
    }

    /// <summary>
    /// 金额帮助类
    /// </summary>
    public static class DecimalHelper
    {
        /// <summary>
        /// 判断指定金额是否在指定金额范围内
        /// </summary>
        public static readonly Func<decimal, decimal, decimal, bool> IsInDecimalPeriodByMomney = (current, min, max) => min <= current && max > current;

        /// <summary>
        /// 判断指定金额是否在指定金额范围内
        /// </summary>
        public static bool In(this decimal current, decimal min, decimal max)
        {
            return IsInDecimalPeriodByMomney(current, min, max);
        }

        /// <summary>
        /// 判断指定金额范围是否包含指定金额范围内(max=0时表示不限制)
        /// </summary>
        public static bool InSpecial(this decimal currentMin, decimal currentMax, decimal min, decimal max)
        {
            if (max == 0)
            {
                max = Decimal.MaxValue;
            }
            if (currentMax == 0)
            {
                currentMax = Decimal.MaxValue;
            }
            return currentMin.In(min, max);
        }
    }
}
