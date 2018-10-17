using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.MiniApp;

namespace Core.MiniApp
{
    public class EnumCore
    {
        public static bool CheckLockState(int state, OperateState op)
        {
            if (state > 0)
            {
                OperateState np = (OperateState)state;
                return (np & op) == op;
            }
            return true;//如果是0，默认配置所有权限
        }

        public static int GetLockEnum(bool IsLockPost, bool IsLockComment, bool IsLockPraise, bool IsLockReward)
        {
            OperateState op = OperateState.none;
            if (IsLockPost)
                op = op|OperateState.post;
            if (IsLockComment)
                op = op | OperateState.comment;
            if (IsLockPraise)
                op = op | OperateState.praise;
            if (IsLockReward)
                op = op | OperateState.reward;
            return (int)op;
        }
    }
}
