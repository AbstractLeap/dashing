namespace Dashing.Console {
    using System;

    public class ColorContext : IDisposable {
        private readonly ConsoleColor original;

        public ColorContext(ConsoleColor color) {
            this.original = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public void Dispose() {
            Console.ForegroundColor = this.original;
        }
    }
}