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
            return new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(true)
                    );
        }

        public static CodeBinaryOperatorExpression ThisFieldIsFalse(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(false)
                    );
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNull(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)
                    );
        }

        public static CodeBinaryOperatorExpression ThisFieldIsNotNull(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityInequality,
                        new CodePrimitiveExpression(null)
                    );
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsTrue(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(true)
                    );
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsFalse(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(false)
                    );
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNull(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)
                    );
        }

        public static CodeBinaryOperatorExpression ThisPropertyIsNotNull(string name)
        {
            return new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), name),
                        CodeBinaryOperatorType.IdentityInequality,
                        new CodePrimitiveExpression(null)
                    );
        }
    }
}