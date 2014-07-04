namespace Dashing.CodeGeneration {
    using System.CodeDom;

    internal static class CodeHelpers {
        public static CodeBinaryOperatorExpression ThisFieldIsTrue(string name) {
            return ElementIs(ThisField(name), true);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsFalse(string name) {
            return ElementIs(ThisField(name), false);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNull(string name) {
            return ElementIs(ThisField(name), null);
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNotNull(string name) {
            return ElementIsNot(ThisField(name), null);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsTrue(string name) {
            return ElementIs(ThisProperty(name), true);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsFalse(string name) {
            return ElementIs(ThisProperty(name), false);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNull(string name) {
            return ElementIs(ThisProperty(name), null);
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNotNull(string name) {
            return ElementIsNot(ThisProperty(name), null);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsTrue(string name) {
            return ElementIs(BaseProperty(name), true);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsFalse(string name) {
            return ElementIs(BaseProperty(name), false);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsNull(string name) {
            return ElementIs(BaseProperty(name), null);
        }

        public static CodeBinaryOperatorExpression BasePropertyIsNotNull(string name) {
            return ElementIsNot(BaseProperty(name), null);
        }

        public static CodeExpression BaseProperty(string name) {
            return new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), name);
        }

        public static CodeExpression BaseField(string name) {
            return new CodeFieldReferenceExpression(new CodeBaseReferenceExpression(), name);
        }

        public static CodeExpression ThisProperty(string name) {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name);
        }

        public static CodeExpression ThisField(string name) {
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);
        }

        public static CodeBinaryOperatorExpression ElementIs(CodeExpression element, object expression) {
            return ElementIs(element, expression, CodeBinaryOperatorType.IdentityEquality);
        }

        public static CodeBinaryOperatorExpression ElementIsNot(CodeExpression element, object expression) {
            return ElementIs(element, expression, CodeBinaryOperatorType.IdentityInequality);
        }

        private static CodeBinaryOperatorExpression ElementIs(CodeExpression element, object expression, CodeBinaryOperatorType op) {
            return new CodeBinaryOperatorExpression(element, op, new CodePrimitiveExpression(expression));
        }
    }
}