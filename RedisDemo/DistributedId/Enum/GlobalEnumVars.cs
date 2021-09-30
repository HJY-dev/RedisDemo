using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedId.Enum
{
    public class GlobalEnumVars
    {
        /// <summary>
        /// 订单编号类型大全
        /// </summary>
        public enum SerialNumberType
        {
            [Description("订单编号")]
            订单编号 = 1,
            [Description("支付单编号")]
            支付单编号 = 2,
            [Description("商品编号")]
            商品编号 = 3,
            [Description("货品编号")]
            货品编号 = 4,
            [Description("售后单编号")]
            售后单编号 = 5,
            [Description("退款单编号")]
            退款单编号 = 6,
            [Description("退货单编号")]
            退货单编号 = 7,
            [Description("发货单编号")]
            发货单编号 = 8,
            [Description("提货单号")]
            提货单号 = 9,
            [Description("服务订单编号")]
            服务订单编号 = 10,
            [Description("服务券兑换码")]
            服务券兑换码 = 11,
        }

        /// <summary>
        /// 来源
        /// 订单来源[对应CoreCmsOrder表source字段]
        /// </summary>
        public enum Source
        {
            [Description("PC页面")]
            PC页面 = 1,
            [Description("H5页面")]
            H5页面 = 2,
            [Description("微信小程序")]
            微信小程序 = 3,
            [Description("支付宝小程序")]
            支付宝小程序 = 4,
            [Description("APP")]
            APP = 5,
            [Description("头条系小程序")]
            头条 = 6
        }
    }
}
