namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    //http://www.mikeobrien.net/blog/building-better-expression-visitor
    public abstract class ExpressionVisitorBase<TState> {
        public virtual TState Visit(Expression node) {
            return this.Visit(node, default(TState));
        }

        public virtual TState Visit(Expression node, TState state) {
            var context = Context.Create(state);
            this.Visit(context, node);
            return context.State;
        }

        private void Visit(Context context, Expression node) {
            ValidateArguments(context, node);

            if (node is BinaryExpression) this.VisitBinary(context, (BinaryExpression)node);
            else if (node is BlockExpression) this.VisitBlock(context, (BlockExpression)node);
            else if (node is ConditionalExpression) this.VisitConditional(context, (ConditionalExpression)node);
            else if (node is ConstantExpression) this.VisitConstant(context, (ConstantExpression)node);
            else if (node is DebugInfoExpression) this.VisitDebugInfo(context, (DebugInfoExpression)node);
            else if (node is DefaultExpression) this.VisitDefault(context, (DefaultExpression)node);
            else if (node is DynamicExpression) this.VisitDynamic(context, (DynamicExpression)node);
            else if (node is GotoExpression) this.VisitGoto(context, (GotoExpression)node);
            else if (node is IndexExpression) this.VisitIndex(context, (IndexExpression)node);
            else if (node is InvocationExpression) this.VisitInvocation(context, (InvocationExpression)node);
            else if (node is LabelExpression) this.VisitLabel(context, (LabelExpression)node);
            else if (node is LambdaExpression) this.VisitLambda(context, (LambdaExpression)node);
            else if (node is ListInitExpression) this.VisitListInit(context, (ListInitExpression)node);
            else if (node is LoopExpression) this.VisitLoop(context, (LoopExpression)node);
            else if (node is MemberExpression) this.VisitMember(context, (MemberExpression)node);
            else if (node is MemberInitExpression) this.VisitMemberInit(context, (MemberInitExpression)node);
            else if (node is MethodCallExpression) this.VisitMethodCall(context, (MethodCallExpression)node);
            else if (node is NewArrayExpression) this.VisitNewArray(context, (NewArrayExpression)node);
            else if (node is NewExpression) this.VisitNew(context, (NewExpression)node);
            else if (node is ParameterExpression) this.VisitParameter(context, (ParameterExpression)node);
            else if (node is RuntimeVariablesExpression) this.VisitRuntimeVariables(context, (RuntimeVariablesExpression)node);
            else if (node is SwitchExpression) this.VisitSwitch(context, (SwitchExpression)node);
            else if (node is TryExpression) this.VisitTry(context, (TryExpression)node);
            else if (node is TypeBinaryExpression) this.VisitTypeBinary(context, (TypeBinaryExpression)node);
            else if (node is UnaryExpression) this.VisitUnary(context, (UnaryExpression)node);
            else throw new ExpressionNotSupportedException(node);
        }

        private void Visit(Context context, IEnumerable<Expression> nodes) {
            foreach (var node in nodes) this.Visit(context, node);
        }

        private void Visit(IDictionary<Expression, Context> nodes) {
            foreach (var node in nodes) this.Visit(node.Value, node.Key);
        }

        private static void Visit<T>(Context context, IEnumerable<T> nodes, Action<Context, T> elementVisitor) {
            foreach (var node in nodes) elementVisitor(context, node);
        }

        private static void ValidateArguments(object node) {
            if (node == null) throw new ArgumentNullException("node");
        }

        private static void ValidateArguments(Context context, object node) {
            if (node == null) throw new ArgumentNullException("node");
            if (context == null) throw new ArgumentNullException("context");
        }

        protected virtual void VisitBinary(Context context, BinaryExpression node) {
            ValidateArguments(context, node);
            this.VisitBinary(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitBinary(BinaryExpression node, TState leftState, TState conversionState, TState rightState, bool visitLeft, bool visitConversion, bool visitRight) {
            ValidateArguments(node);
            if (visitLeft && node.Left != null) this.Visit(Context.Create(node, leftState), node.Left);
            if (visitConversion && node.Conversion != null) this.Visit(Context.Create(node, conversionState), node.Conversion);
            if (visitRight && node.Right != null) this.Visit(Context.Create(node, rightState), node.Right);
        }

        protected virtual void VisitBlock(Context context, BlockExpression node) {
            ValidateArguments(context, node);
            this.VisitBlock(node, context.State, context.State, true, true, Enumerable.Empty<Expression>(), Enumerable.Empty<ParameterExpression>());
        }

        protected void VisitBlock(BlockExpression node, TState expressionsState, TState variablesState, bool visitExpressions, bool visitVariables, IEnumerable<Expression> expressionsNotToVisit, IEnumerable<ParameterExpression> variablesNotToVisit) {
            ValidateArguments(node);
            if (visitExpressions && node.Expressions != null) this.Visit(Context.Create(node, expressionsState), node.Expressions.Except(expressionsNotToVisit));
            if (visitVariables && node.Variables != null) this.Visit(Context.Create(node, variablesState), node.Variables.Except(variablesNotToVisit));
        }

        protected virtual void VisitCatchBlock(Context context, CatchBlock node) {
            ValidateArguments(context, node);
            this.VisitCatchBlock(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitCatchBlock(CatchBlock node, TState variableState, TState filterState, TState bodyState, bool visitVariable, bool visitFilter, bool visitBody) {
            ValidateArguments(node);
            if (visitVariable && node.Variable != null) this.Visit(Context.Create(node, variableState), node.Variable);
            if (visitFilter && node.Filter != null) this.Visit(Context.Create(node, filterState), node.Filter);
            if (visitBody && node.Body != null) this.Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitConditional(Context context, ConditionalExpression node) {
            ValidateArguments(context, node);
            this.VisitConditional(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitConditional(ConditionalExpression node, TState testState, TState trueState, TState falseState, bool visitTest, bool visitTrue, bool visitFalse) {
            ValidateArguments(node);
            if (visitTest && node.Test != null) this.Visit(Context.Create(node, testState), node.Test);
            if (visitTrue && node.IfTrue != null) this.Visit(Context.Create(node, trueState), node.IfTrue);
            if (visitFalse && node.IfFalse != null) this.Visit(Context.Create(node, falseState), node.IfFalse);
        }

        protected virtual void VisitConstant(Context context, ConstantExpression node) { }

        protected virtual void VisitDebugInfo(Context context, DebugInfoExpression node) { }

        protected virtual void VisitDefault(Context context, DefaultExpression node) { }

        protected virtual void VisitDynamic(Context context, DynamicExpression node) {
            ValidateArguments(context, node);
            this.VisitDynamic(node, context.State);
        }

        protected void VisitDynamic(DynamicExpression node, TState state, params Expression[] argsNotToVisit) {
            ValidateArguments(node);
            if (node.Arguments != null) Visit(Context.Create(node, state), node.Arguments.Except(argsNotToVisit), this.Visit);
        }

        protected virtual void VisitElementInit(Context context, ElementInit node) {
            ValidateArguments(context, node);
            this.VisitElementInit(node, context.State);
        }

        protected void VisitElementInit(ElementInit node, TState state, params Expression[] argsNotToVisit) {
            ValidateArguments(node);
            if (node.Arguments != null) Visit(Context.Create(node, state), node.Arguments.Except(argsNotToVisit), this.Visit);
        }

        protected virtual void VisitGoto(Context context, GotoExpression node) {
            ValidateArguments(context, node);
            this.VisitGoto(node, context.State, context.State, true, true);
        }

        protected void VisitGoto(GotoExpression node, TState targetState, TState valueState, bool visitTarget, bool visitValue) {
            ValidateArguments(node);
            if (visitTarget && node.Target != null) this.VisitLabelTarget(Context.Create(node, targetState), node.Target);
            if (visitValue && node.Value != null) this.Visit(Context.Create(node, valueState), node.Value);
        }

        protected virtual void VisitIndex(Context context, IndexExpression node) {
            ValidateArguments(context, node);
            this.VisitIndex(node, context.State, context.State, true, true);
        }

        protected void VisitIndex(IndexExpression node, TState objectState, TState argumentsState, bool visitObject, bool visitArguments, params Expression[] argsNotToVisit) {
            ValidateArguments(node);
            if (visitObject && node.Object != null) this.Visit(Context.Create(node, objectState), node.Object);
            if (visitArguments && node.Arguments != null) Visit(Context.Create(node, argumentsState), node.Arguments.Except(argsNotToVisit), this.Visit);
        }

        protected virtual void VisitInvocation(Context context, InvocationExpression node) {
            ValidateArguments(context, node);
            this.VisitInvocation(node, context.State, context.State, true, true);
        }

        protected void VisitInvocation(InvocationExpression node, TState expressionState, TState argumentsState, bool visitExpression, bool visitArguments, params Expression[] argsNotToVisit) {
            ValidateArguments(node);
            if (visitExpression && node.Expression != null) this.Visit(Context.Create(node, expressionState), node.Expression);
            if (visitArguments && node.Arguments != null) Visit(Context.Create(node, argumentsState), node.Arguments.Except(argsNotToVisit), this.Visit);
        }

        protected virtual void VisitLabel(Context context, LabelExpression node) {
            ValidateArguments(context, node);
            this.VisitLabel(node, context.State, context.State, true, true);
        }

        protected void VisitLabel(LabelExpression node, TState targetState, TState defaultValueState, bool visitTarget, bool visitDefaultValue) {
            ValidateArguments(node);
            if (visitTarget && node.Target != null) this.VisitLabelTarget(Context.Create(node, targetState), node.Target);
            if (visitDefaultValue && node.DefaultValue != null) this.Visit(Context.Create(node, defaultValueState), node.DefaultValue);
        }

        protected virtual void VisitLabelTarget(Context context, LabelTarget node) { }

        protected virtual void VisitLambda(Context context, LambdaExpression node) {
            ValidateArguments(context, node);
            this.VisitLambda(node, context.State, context.State, true, true);
        }

        protected void VisitLambda(LambdaExpression node, TState bodyState, TState parametersState, bool visitBody, bool visitParameters, params ParameterExpression[] paramsNotToVisit) {
            ValidateArguments(node);
            if (visitBody && node.Body != null) this.Visit(Context.Create(node, bodyState), node.Body);
            if (visitParameters && node.Parameters != null) Visit(Context.Create(node, parametersState), node.Parameters.Except(paramsNotToVisit), this.Visit);
        }

        protected virtual void VisitListInit(Context context, ListInitExpression node) {
            ValidateArguments(context, node);
            this.VisitListInit(node, context.State, context.State, true, true);
        }

        protected void VisitListInit(ListInitExpression node, TState newExpressionState, TState initializerState, bool visitNewExpression, bool visitInitializers, params ElementInit[] initializersNotToVisit) {
            ValidateArguments(node);
            if (visitNewExpression && node.NewExpression != null) this.Visit(Context.Create(node, newExpressionState), node.NewExpression);
            if (visitInitializers && node.Initializers != null) Visit(Context.Create(node, initializerState), node.Initializers.Except(initializersNotToVisit), this.VisitElementInit);
        }

        protected virtual void VisitLoop(Context context, LoopExpression node) {
            ValidateArguments(context, node);
            this.VisitLoop(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitLoop(LoopExpression node, TState breakLabelState, TState continueLabelState, TState bodyState, bool visitBreakLabel, bool visitContinueLabel, bool visitBody) {
            ValidateArguments(node);
            if (visitBreakLabel && node.BreakLabel != null) this.VisitLabelTarget(Context.Create(node, breakLabelState), node.BreakLabel);
            if (visitContinueLabel && node.ContinueLabel != null) this.VisitLabelTarget(Context.Create(node, continueLabelState), node.ContinueLabel);
            if (visitBody && node.Body != null) this.Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitMember(Context context, MemberExpression node) {
            ValidateArguments(context, node);
            this.VisitMember(node, context.State);
        }

        protected void VisitMember(MemberExpression node, TState state) {
            ValidateArguments(node);
            if (node.Expression != null) this.Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitMemberAssignment(Context context, MemberAssignment node) {
            ValidateArguments(context, node);
            this.VisitMemberAssignment(node, context.State);
        }

        protected void VisitMemberAssignment(MemberAssignment node, TState state) {
            ValidateArguments(node);
            if (node.Expression != null) this.Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitMemberBinding(Context context, MemberBinding node) {
            ValidateArguments(context, node);
            this.VisitMemberBinding(context, node, context.State);
        }

        protected void VisitMemberBinding(Context context, MemberBinding node, TState state) {
            ValidateArguments(node);
            switch (node.BindingType) {
                case MemberBindingType.Assignment:
                    this.VisitMemberAssignment(Context.Create(context.Parent, state), (MemberAssignment)node);
                    break;
                case MemberBindingType.MemberBinding:
                    this.VisitMemberMemberBinding(Context.Create(context.Parent, state), (MemberMemberBinding)node);
                    break;
                case MemberBindingType.ListBinding:
                    this.VisitMemberListBinding(Context.Create(context.Parent, state), (MemberListBinding)node);
                    break;
                default:
                    throw new Exception("Cannont bind type " + node.BindingType);
                    break;
            }
        }

        protected virtual void VisitMemberInit(Context context, MemberInitExpression node) {
            ValidateArguments(context, node);
            this.VisitMemberInit(node, context.State, context.State, true, true);
        }

        protected void VisitMemberInit(MemberInitExpression node, TState newExpressionState, TState bindingsState, bool visitNewExpression, bool visitBindings, params MemberBinding[] bindingsNotToVisit) {
            ValidateArguments(node);
            if (visitNewExpression && node.NewExpression != null) this.Visit(Context.Create(node, newExpressionState), node.NewExpression);
            if (visitBindings && node.Bindings != null) Visit(Context.Create(node, bindingsState), node.Bindings.Except(bindingsNotToVisit), this.VisitMemberBinding);
        }

        protected virtual void VisitMemberListBinding(Context context, MemberListBinding node) {
            ValidateArguments(context, node);
            this.VisitMemberListBinding(node, context.State);
        }

        protected void VisitMemberListBinding(MemberListBinding node, TState state, params ElementInit[] initializersNotToVisit) {
            ValidateArguments(node);
            if (node.Initializers != null) Visit(Context.Create(node, state), node.Initializers.Except(initializersNotToVisit), this.VisitElementInit);
        }

        protected virtual void VisitMemberMemberBinding(Context context, MemberMemberBinding node) {
            ValidateArguments(context, node);
            this.VisitMemberMemberBinding(node, context.State);
        }

        protected void VisitMemberMemberBinding(MemberMemberBinding node, TState state, params MemberBinding[] memberBindingsNotToVisit) {
            ValidateArguments(node);
            if (node.Bindings != null) Visit(Context.Create(node, state), node.Bindings.Except(memberBindingsNotToVisit), this.VisitMemberBinding);
        }

        protected virtual void VisitMethodCall(Context context, MethodCallExpression node) {
            ValidateArguments(context, node);
            this.VisitMethodCall(node, context.State, context.State, true, true);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, bool visitObject) {
            this.VisitMethodCall(node, objectState, null, visitObject, false);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, TState argumentsState, bool visitObject, bool visitArguments, params Expression[] argumentsNotToVisit) {
            this.VisitMethodCall(node, objectState, node.Arguments.ToDictionary(x => x, x => objectState), visitObject, visitArguments, argumentsNotToVisit);
        }

        protected void VisitMethodCall(MethodCallExpression node, TState objectState, IDictionary<Expression, TState> argumentsState, bool visitObject, bool visitArguments, params Expression[] argumentsNotToVisit) {
            ValidateArguments(node);
            if (visitObject && node.Object != null) this.Visit(Context.Create(node, objectState), node.Object);
            if (visitArguments && node.Arguments != null)
                this.Visit(
                    argumentsState.Where(x => !argumentsNotToVisit.Contains(x.Key))
                                  .ToDictionary(x => x.Key, x => Context.Create(x.Key, x.Value)));
        }

        protected virtual void VisitNew(Context context, NewExpression node) {
            ValidateArguments(context, node);
            this.VisitNew(node, context.State);
        }

        protected void VisitNew(NewExpression node, TState state, params Expression[] argumentsNotToVisit) {
            ValidateArguments(node);
            if (node.Arguments != null) this.Visit(Context.Create(node, state), node.Arguments.Except(argumentsNotToVisit));
        }

        protected virtual void VisitNewArray(Context context, NewArrayExpression node) {
            ValidateArguments(context, node);
            this.VisitNewArray(node, context.State);
        }

        protected void VisitNewArray(NewArrayExpression node, TState state, params Expression[] expressionsNotToVisit) {
            ValidateArguments(node);
            if (node.Expressions != null) this.Visit(Context.Create(node, state), node.Expressions.Except(expressionsNotToVisit));
        }

        protected virtual void VisitParameter(Context context, ParameterExpression node) { }

        protected virtual void VisitRuntimeVariables(Context context, RuntimeVariablesExpression node) {
            ValidateArguments(context, node);
            this.VisitRuntimeVariables(node, context.State);
        }

        protected void VisitRuntimeVariables(RuntimeVariablesExpression node, TState state, params ParameterExpression[] variablesNotToVisit) {
            ValidateArguments(node);
            if (node.Variables != null) Visit(Context.Create(node, state), node.Variables.Except(variablesNotToVisit), this.Visit);
        }

        protected virtual void VisitSwitch(Context context, SwitchExpression node) {
            ValidateArguments(context, node);
            this.VisitSwitch(node, context.State, context.State, context.State, true, true, true);
        }

        protected void VisitSwitch(SwitchExpression node, TState switchValueState, TState switchCasesState, TState defaultBodyState, bool visitSwitchValue, bool visitSwitchCases, bool visitDefaultBody, params SwitchCase[] switchCasesNotToVisit) {
            ValidateArguments(node);
            if (visitSwitchValue && node.SwitchValue != null) this.Visit(Context.Create(node, switchValueState), node.SwitchValue);
            if (visitSwitchCases && node.Cases != null) Visit(Context.Create(node, switchCasesState), node.Cases.Except(switchCasesNotToVisit), this.VisitSwitchCase);
            if (visitDefaultBody && node.DefaultBody != null) this.Visit(Context.Create(node, defaultBodyState), node.DefaultBody);
        }

        protected virtual void VisitSwitchCase(Context context, SwitchCase node) {
            ValidateArguments(context, node);
            this.VisitSwitchCase(node, context.State, context.State, true, true);
        }

        protected void VisitSwitchCase(SwitchCase node, TState testValuesState, TState bodyState, bool visitTestValues, bool visitBody, params Expression[] testValuesNotToVisit) {
            ValidateArguments(node);
            if (visitTestValues && node.TestValues != null) this.Visit(Context.Create(node, testValuesState), node.TestValues.Except(testValuesNotToVisit));
            if (visitBody && node.Body != null) this.Visit(Context.Create(node, bodyState), node.Body);
        }

        protected virtual void VisitTry(Context context, TryExpression node) {
            ValidateArguments(context, node);
            this.VisitTry(node, context.State, context.State, context.State, context.State, true, true, true, true);
        }

        protected void VisitTry(TryExpression node, TState bodyState, TState handlersState, TState finallyState, TState faultState, bool visitBody, bool visitHandlers, bool visitFinally, bool visitFault, params CatchBlock[] catchBlocksNotToVisit) {
            ValidateArguments(node);
            if (visitBody && node.Body != null) this.Visit(Context.Create(node, bodyState), node.Body);
            if (visitHandlers && node.Handlers != null) Visit(Context.Create(node, handlersState), node.Handlers.Except(catchBlocksNotToVisit), this.VisitCatchBlock);
            if (visitFinally && node.Finally != null) this.Visit(Context.Create(node, finallyState), node.Finally);
            if (visitFault && node.Fault != null) this.Visit(Context.Create(node, faultState), node.Fault);
        }

        protected virtual void VisitTypeBinary(Context context, TypeBinaryExpression node) {
            ValidateArguments(context, node);
            this.VisitTypeBinary(node, context.State);
        }

        protected void VisitTypeBinary(TypeBinaryExpression node, TState state) {
            ValidateArguments(node);
            if (node.Expression != null) this.Visit(Context.Create(node, state), node.Expression);
        }

        protected virtual void VisitUnary(Context context, UnaryExpression node) {
            ValidateArguments(context, node);
            this.VisitUnary(node, context.State);
        }

        protected void VisitUnary(UnaryExpression node, TState state) {
            ValidateArguments(node);
            if (node.Operand != null) this.Visit(Context.Create(node, state), node.Operand);
        }

        public class ExpressionNotSupportedException : Exception {
            public ExpressionNotSupportedException(Expression expression)
                : base($"Expression {expression.GetType() .Name} not supported.") { }
        }

        public class Context {
            private Context(ExpressionParent expressionParent, TState state) {
                this.Parent = expressionParent;
                this.State = state;
            }

            public bool HasParent => this.Parent != null;

            public ExpressionParent Parent { get; }

            public TState State { get; set; }

            public static Context Create(TState state) {
                return new Context(null, state);
            }

            public static Context Create(ExpressionParent parent, TState state) {
                return new Context(parent, state);
            }

            public static Context Create(Expression parentExpression, TState state) {
                return new Context(new ExpressionParent(parentExpression), state);
            }

            public static Context Create(SwitchCase parentSwitchCase, TState state) {
                return new Context(new ExpressionParent(parentSwitchCase), state);
            }

            public static Context Create(MemberBinding parentMemberBinding, TState state) {
                return new Context(new ExpressionParent(parentMemberBinding), state);
            }

            public static Context Create(MemberMemberBinding parentMemberMemberBinding, TState state) {
                return new Context(new ExpressionParent(parentMemberMemberBinding), state);
            }

            public static Context Create(MemberListBinding parentMemberListBinding, TState state) {
                return new Context(new ExpressionParent(parentMemberListBinding), state);
            }

            public static Context Create(MemberAssignment parentMemberAssignment, TState state) {
                return new Context(new ExpressionParent(parentMemberAssignment), state);
            }

            public static Context Create(ElementInit parentElementInit, TState state) {
                return new Context(new ExpressionParent(parentElementInit), state);
            }

            public static Context Create(CatchBlock parentCatchBlock, TState state) {
                return new Context(new ExpressionParent(parentCatchBlock), state);
            }
        }

        public class ExpressionParent {
            public enum ParentType {
                Expression,

                SwitchCase,

                MemberBinding,

                MemberMemberBinding,

                MemberListBinding,

                MemberAssignment,

                ElementInit,

                CatchBlock
            }

            public readonly CatchBlock CatchBlock;

            public readonly ElementInit ElementInit;

            public readonly Expression Expression;

            public readonly MemberAssignment MemberAssignment;

            public readonly MemberBinding MemberBinding;

            public readonly MemberListBinding MemberListBinding;

            public readonly MemberMemberBinding MemberMemberBinding;

            public readonly SwitchCase SwitchCase;

            public readonly ParentType Type;

            public ExpressionParent(Expression expression) {
                this.Expression = expression;
                this.Type = ParentType.Expression;
            }

            public ExpressionParent(SwitchCase switchCase) {
                this.SwitchCase = switchCase;
                this.Type = ParentType.SwitchCase;
            }

            public ExpressionParent(MemberBinding memberBinding) {
                this.MemberBinding = memberBinding;
                this.Type = ParentType.MemberBinding;
            }

            public ExpressionParent(MemberMemberBinding memberMemberBinding) {
                this.MemberMemberBinding = memberMemberBinding;
                this.Type = ParentType.MemberMemberBinding;
            }

            public ExpressionParent(MemberListBinding memberListBinding) {
                this.MemberListBinding = memberListBinding;
                this.Type = ParentType.MemberListBinding;
            }

            public ExpressionParent(MemberAssignment memberAssignment) {
                this.MemberAssignment = memberAssignment;
                this.Type = ParentType.MemberAssignment;
            }

            public ExpressionParent(ElementInit elementInit) {
                this.ElementInit = elementInit;
                this.Type = ParentType.ElementInit;
            }

            public ExpressionParent(CatchBlock catchBlock) {
                this.CatchBlock = catchBlock;
                this.Type = ParentType.CatchBlock;
            }

            public bool IsExpressionOfType<T>() {
                return this.Type == ParentType.Expression && this.Expression is T;
            }

            public bool IsExpressionOf(ExpressionType type) {
                return this.Type == ParentType.Expression && this.Expression.NodeType == type;
            }

            public bool IsBinaryLogicalOperator() {
                if (this.Type != ParentType.Expression) return false;
                switch (this.Expression.NodeType) {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse: return true;
                    default: return false;
                }
            }

            public bool IsBinaryComparisonOperator() {
                if (this.Type != ParentType.Expression) return false;
                switch (this.Expression.NodeType) {
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.NotEqual: return true;
                    default: return false;
                }
            }

            public bool IsBinaryMathOperator() {
                if (this.Type != ParentType.Expression) return false;
                switch (this.Expression.NodeType) {
                    case ExpressionType.Add:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.Subtract: return true;
                    default: return false;
                }
            }
        }
    }
}