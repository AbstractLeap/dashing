namespace Dashing.Tools {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
#if COREFX
    using System.Reflection;
#endif

    public class ConsoleLogger : ILogger {
        private readonly bool isVerbose;

        private readonly ConsoleColor color;

        public ConsoleLogger(bool isVerbose, ConsoleColor color = ConsoleColor.Gray) {
            this.isVerbose = isVerbose;
            this.color = color;
        }

        public void Trace(string message) {
            if (!this.isVerbose) {
                return;
            }

            this.InternalTrace(message);
        }

        public void Trace(string message, params object[] args) {
            if (!this.isVerbose) {
                return;
            }

            this.InternalTrace(string.Format(message, args));
        }

        public void Trace<T>(IEnumerable<T> items, string[] columnHeaders = null) {
            if (!this.isVerbose) {
                return;
            }

            var props = typeof(T).GetProperties().ToArray();
            if (columnHeaders == null) {
                columnHeaders = props.Select(p => p.Name).ToArray();
            }

            var arrValues = new string[items.Count() + 1, columnHeaders.Length];

            // fill headers
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++) {
                arrValues[0, colIndex] = columnHeaders[colIndex];
            }

            // Fill table rows
            for (int rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++) {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++) {
                    arrValues[rowIndex, colIndex] = props.ElementAt(colIndex).GetValue(items.ElementAt(rowIndex - 1)).ToString();
                }
            }

            var message = this.ToStringTable(arrValues);
            this.InternalTrace(message);
        }

        public void Error(string message) {
            this.InternalError(message);
        }

        public void Error(string message, params object[] args) {
            this.InternalError(string.Format(message, args));
        }

        private void InternalError(string message) {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }

        private void InternalTrace(string message) {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = this.color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }

        private string ToStringTable(string[,] arrValues) {
            int[] maxColumnsWidth = this.GetMaxColumnsWidth(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++) {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++) {
                    // Print cell
                    string cell = arrValues[rowIndex, colIndex];
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0) {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private int[] GetMaxColumnsWidth(string[,] arrValues) {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++) {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++) {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength) {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }
    }
}