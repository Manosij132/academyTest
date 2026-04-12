using System.Linq.Expressions;

namespace Academy.Core
{
    /// <summary>
    /// Visits and replaces occurrences of a specified parameter expression with a new parameter expression 
    /// in an expression tree.
    /// </summary>
    public class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        // <summary>
        /// Initializes a new instance of the <see cref="ReplaceParameterVisitor"/> class.
        /// </summary>
        /// <param name="oldParameter">The parameter expression to be replaced.</param>
        /// <param name="newParameter">The new parameter expression that will replace the old one.</param>
        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {        
            // Store the old parameter expression to be replaced
            _oldParameter = oldParameter;
            // Store the new parameter expression to use as a replacement
            _newParameter = newParameter;
        }
        /// <summary>
        /// Visits a <see cref="ParameterExpression"/> node in the expression tree.
        /// </summary>
        /// <param name="node">The <see cref="ParameterExpression"/> node to visit.</param>
        /// <returns>
        /// The modified <see cref="Expression"/>. If the node matches the old parameter, it is replaced 
        /// with the new parameter. Otherwise, the base method is called for further processing.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            // If the node is the old parameter, replace it with the new parameter
            // Otherwise, continue processing the node as usual
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}
