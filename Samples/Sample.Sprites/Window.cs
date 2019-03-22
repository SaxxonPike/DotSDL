﻿using DotSDL.Events;
using DotSDL.Graphics;
using DotSDL.Input.Keyboard;
using System;

namespace Sample.Sprites {
    public class Window : SdlWindow {
        //private int _camX = 0, _camY = 0, _deltaX = 2, _deltaY = 1;
        private Player _player1, _player2;
        private Point _player1Delta, _player2Delta;

        public Window(int scale) : base("Sprites Test",
                                        new Point(WindowPosUndefined, WindowPosUndefined),
                                        256 * scale, 196 * scale,
                                        256, 196) {
            KeyPressed += OnKeyPressed;
            KeyReleased += OnKeyReleased;

            Background.Height = Background.Width = 1024;

            GenerateBackground();
            GeneratePlayers();
        }

        private void GenerateBackground() {
            // Draw colored, diagonal strips across the entire background canvas.
            var stripRef = new Color[Background.Width * 2];
            for(var i = 0; i < Background.Width * 2; i++) {
                stripRef[i].R = (byte)(Math.Abs(i % 511 - 255) / 2);
                stripRef[i].G = (byte)(80 / (stripRef[i].R + 4) / 2);
                stripRef[i].B = (byte)(120 * (Math.Sin(i * Math.PI / 128 + 256) * 0.2 + 0.8));
            }

            for(var y = 0; y < Background.Height; y++) {
                var stripIdx = y % Background.Width;
                for(var x = 0; x < Background.Width; x++) {
                    var pix = y * Background.Width + x;
                    Background.Pixels[pix] = stripRef[stripIdx + x];
                }
            }

            // Darken every other line, because why not. :)
            for(var y = 2; y < Background.Height; y += 2) {
                for(var x = 0; x < Background.Width; x++) {
                    var pix = Background.Width * y + x;
                    Background.Pixels[pix].R = (byte)(Background.Pixels[pix].R * 0.8);
                    Background.Pixels[pix].G = (byte)(Background.Pixels[pix].G * 0.8);
                    Background.Pixels[pix].B = (byte)(Background.Pixels[pix].B * 0.8);
                }
            }

            // Finally, draw a dashed border around the edge to show the edge boundaries.
            // This routine assumes that the canvas is square.
            const int lineSize = 7;
            const int margin = 4;
            var black = new Color { R = 0, G = 0, B = 0 };
            var yellow = new Color { R = 255, G = 255, B = 0 };

            for(var i = margin; i < Background.Width - margin; i++) {
                // Y axis.
                var activeColor = (i - margin) / lineSize % 2 == 1 ? black : yellow;

                var pix = Background.Width * i + margin;
                Background.Pixels[pix] = activeColor;
                pix = Background.Width * i + Background.Width - 1 - margin;
                Background.Pixels[pix] = activeColor;

                // X axis.
                activeColor = (i - margin) / lineSize % 2 == 1 ? yellow : black;

                pix = Background.Width * margin + i;
                Background.Pixels[pix] = activeColor;
                pix = Background.Width * (Background.Height - 1 - margin) + i;
                Background.Pixels[pix] = activeColor;
            }
        }

        private void GeneratePlayers() {
            _player1 = new Player(new Color { R = 255, G = 64, B = 64 }, 2, 1);
            _player2 = new Player(new Color { R = 64, G = 64, B = 255 }, 3, 2);

            _player1.Position.X = 24;
            _player1.Position.Y = 24;
            _player1Delta = new Point();

            _player2.Position.X = 96;
            _player2.Position.Y = 24;
            _player2Delta = new Point();

            _player1.Scale.X = 1.5f;
            _player1.BlendMode = BlendMode.Additive;

            _player2.ScalingQuality = ScalingQuality.Linear;
            _player2.Scale.Y = 2.0f;

            Sprites.Add(_player1);
            Sprites.Add(_player2);
        }

        private void OnKeyPressed(object sender, KeyboardEvent e) {
            switch(e.Keycode) {
                case Keycode.Escape:
                    Stop();
                    break;
                case Keycode.W:
                    _player1Delta.Y = -1;
                    break;
                case Keycode.S:
                    _player1Delta.Y = 1;
                    break;
                case Keycode.A:
                    _player1Delta.X = -1;
                    break;
                case Keycode.D:
                    _player1Delta.X = 1;
                    break;
                case Keycode.Up:
                    _player2Delta.Y = -1;
                    break;
                case Keycode.Down:
                    _player2Delta.Y = 1;
                    break;
                case Keycode.Left:
                    _player2Delta.X = -1;
                    break;
                case Keycode.Right:
                    _player2Delta.X = 1;
                    break;
            }
        }

        private void OnKeyReleased(object sender, KeyboardEvent e) {
            switch(e.Keycode) {
                case Keycode.W:
                case Keycode.S:
                    _player1Delta.Y = 0;
                    break;
                case Keycode.A:
                case Keycode.D:
                    _player1Delta.X = 0;
                    break;
                case Keycode.Up:
                case Keycode.Down:
                    _player2Delta.Y = 0;
                    break;
                case Keycode.Left:
                case Keycode.Right:
                    _player2Delta.X = 0;
                    break;

            }
        }

        protected override void OnUpdate() {
            _player1.Move(_player1Delta);
            _player2.Move(_player2Delta);

            var x1 = _player1.Position.X <= _player2.Position.X
                         ? _player1.Position.X - (_player1.Width * _player1.Scale.X)
                         : _player2.Position.X - (_player2.Width * _player2.Scale.X);
            var x2 = _player1.Position.X >= _player2.Position.X
                         ? _player1.Position.X + (_player1.Width * _player1.Scale.X)
                         : _player2.Position.X + (_player2.Width * _player2.Scale.X);
            var y1 = _player1.Position.Y <= _player2.Position.Y
                         ? _player1.Position.Y - (_player1.Height * _player1.Scale.Y)
                         : _player2.Position.Y - (_player2.Height * _player2.Scale.Y);
            var y2 = _player1.Position.Y >= _player2.Position.Y
                         ? _player1.Position.Y + (_player1.Height * _player1.Scale.Y)
                         : _player2.Position.Y + (_player2.Height * _player2.Scale.Y);

            x1 = x1 < 0 ? 0 : x1;
            x2 = x2 >= Background.Width ? Background.Width : x2;
            y1 = y1 < 0 ? 0 : y1;
            y2 = y2 >= Background.Height ? Background.Height : y2;

            Background.Clipping.Position.X = (int)x1;
            Background.Clipping.Position.Y = (int)y1;
            Background.Clipping.Size.X = (int)(x2 - x1);
            Background.Clipping.Size.Y = (int)(y2 - y1);

            WindowTitle = $"({(int)x1} {(int)y2}), ({(int)(x2 - x1)}, {(int)(y2 - y1)}) / ({Background.Width}, {Background.Height})";

            /*_camX += _deltaX;
            _camY += _deltaY;

            if(_camX + Background.Clipping.Size.X >= Background.Width || _camX <= 0)
                _deltaX = -_deltaX;
            if(_camY + Background.Clipping.Size.Y >= Background.Height || _camY <= 0)
                _deltaY = -_deltaY;

            Background.Clipping.Position.X = _camX;
            Background.Clipping.Position.Y = _camY;*/
        }
    }
}
