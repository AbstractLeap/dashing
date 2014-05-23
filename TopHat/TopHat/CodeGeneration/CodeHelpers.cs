using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.CodeGeneration
{
    internal static class CodeHelpers
    {
        public static CodeBinaryOperatorExpression ThisFieldIsTrue(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisField(name), true);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsFalse(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisField(name), false);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNull(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisField(name), null);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNotNull(string name)
        {
            return CodeHelpers.ElementIsNot(CodeHelpers.ThisField(name), null);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsTrue(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisProperty(name), true);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsFalse(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisProperty(name), false);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNull(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.ThisProperty(name), null);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNotNull(string name)
        {
            return CodeHelpers.ElementIsNot(CodeHelpers.ThisProperty(name), null);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsTrue(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.BaseProperty(name), true);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsFalse(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.BaseProperty(name), false);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsNull(string name)
        {
            return CodeHelpers.ElementIs(CodeHelpers.BaseProperty(name), null);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsNotNull(string name)
        {
            return CodeHelpers.ElementIsNot(CodeHelpers.BaseProperty(name), null);
        }

        public static CodeExpression BaseProperty(string name)
        {
            return new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), name);
        }

        public static CodeExpression BaseField(string name)
        {
            return new CodeFieldReferenceExpression(new CodeBaseReferenceExpression(), name);
        }

        public static CodeExpression ThisProperty(string name)
        {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name);
        }

        public static CodeExpression ThisField(string name)
        {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);
        }

        public static CodeBinaryOperatorExpression ElementIs(CodeExpression element, object expression)
        {
            return ElementIs(element, expression, CodeBinaryOperatorType.IdentityEquality);
        }

        public static CodeBinaryOperatorExpression ElementIsNot(CodeExpression element, object expression)
        {
            return ElementIs(element, expression, CodeBinaryOperatorType.IdentityInequality);
        }

        private static CodeBinaryOperatorExpression ElementIs(CodeExpression element, object expression, CodeBinaryOperatorType op)
        {
            return new CodeBinaryOperatorExpression(
                element,
                op,
                new CodePrimitiveExpression(expression)
            );
        }
    }
}