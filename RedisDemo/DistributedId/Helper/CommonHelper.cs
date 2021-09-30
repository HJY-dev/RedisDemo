using DistributedId.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedId.Helper
{
    public class CommonHelper
    {
        #region 获取多种数据编号
        /// <summary>
        /// 获取多种数据编号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSerialNumberType(int type)
        {
            var str = string.Empty;
            Random rand = new Random();
            switch (type)
            {
                case (int)GlobalEnumVars.SerialNumberType.订单编号:         //订单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.支付单编号:         //支付单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.商品编号:         //商品编号
                    str = 'G' + Msectime() + rand.Next(0, 5);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.货品编号:         //货品编号
                    str = 'P' + Msectime() + rand.Next(0, 5);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.售后单编号:         //售后单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.退款单编号:         //退款单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.退货单编号:         //退货单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.发货单编号:         //发货单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.服务订单编号:         //服务订单编号
                    str = type + Msectime() + rand.Next(0, 9);
                    break;
                case (int)GlobalEnumVars.SerialNumberType.提货单号:         //提货单号
                    //str = 'T' + type + msectime() + rand.Next(0, 5);
                    var charsStr = new[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '2', '3', '4', '5', '6', '7', '8', '9' };
                    var charsLen = charsStr.Length - 1;
                    //    shuffle($chars);
                    str = "";
                    for (int i = 0; i < 6; i++)
                    {
                        str += charsStr[rand.Next(0, charsLen)];
                    }
                    break;
                case (int)GlobalEnumVars.SerialNumberType.服务券兑换码:         //服务券兑换码
                    var charsStr2 = new[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '2', '3', '4', '5', '6', '7', '8', '9' };
                    var charsLen2 = charsStr2.Length - 1;
                    //    shuffle($chars);
                    str = "";
                    for (int i = 0; i < 6; i++)
                    {
                        str += charsStr2[rand.Next(0, charsLen2)];
                    }
                    break;
                default:
                    str = 'T' + Msectime() + rand.Next(0, 9);
                    break;
            }
            return str;
        }

        #endregion

        #region 返回当前的毫秒时间戳

        /// <summary>
        /// 返回当前的毫秒时间戳
        /// </summary>
        public static string Msectime()
        {
            long timeTicks = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            return timeTicks.ToString();
        }

        #endregion

    }
}
