using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace User.MiniApp.Comment
{
    public class ExpressionModifier : ExpressionVisitor
    {
        public ExpressionModifier(Expression newExpression, Expression oldExpression)
        {
            this.newExpression = newExpression;
            this.oldExpression = oldExpression;
        }

        private Expression newExpression;
        private Expression oldExpression;

        public static Expression Replace(Expression e, Expression oldExpression, Expression newExpression)
        {
            return new ExpressionModifier(newExpression, oldExpression).Replace(e);
        }

        public Expression Replace(Expression e)
        {
            return Visit(e);
        }

        public override Expression Visit(Expression node)
        {
            if (node == oldExpression)
                return base.Visit(newExpression);

            return base.Visit(node);
        }
    }
}