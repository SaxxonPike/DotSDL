﻿namespace DotSDL.Sample.BasicPixels {
    internal class BasicPixels {
        private static void Main(string[] args) {
            var window = new Window(512, 256);
            window.Start(100, 16);  // 10fps, 62.5ups
        }
    }
}
