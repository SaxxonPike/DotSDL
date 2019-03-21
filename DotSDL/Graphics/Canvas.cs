﻿using DotSDL.Interop.Core;
using SdlPixels = DotSDL.Interop.Core.Pixels;
using System;

namespace DotSDL.Graphics {
    /// <summary>
    /// A representation of the contents of the SDL window, with a number of
    /// helper routines.
    /// </summary>
    public class Canvas {
        private int _width, _height;

        /// <summary>
        /// <c>true</c> if this <see cref="Canvas"/> has an SDL texture associated with it, otherwise <c>false</c>.
        /// </summary>
        protected bool HasTexture { get; private set; }

        /// <summary>
        /// The scaling type that should be used to draw this <see cref="Canvas"/>. This field should not be
        /// manipulated directly--use <see cref="ScalingQuality"/> instead.
        /// </summary>
        protected ScalingQuality ScalingQualityValue;

        /// <summary>
        /// The blend mode that should be used to draw this <see cref="Canvas"/>. This field should
        /// not be manipulated directly--use <see cref="BlendMode"/> instead.
        /// </summary>
        protected BlendMode BlendModeValue = BlendMode.Alpha;

        /// <summary>
        /// The SDL_Renderer that this <see cref="Canvas"/> will be drawn by.
        /// </summary>
        internal IntPtr Renderer;

        /// <summary>
        /// The SDL_Texture that this <see cref="Canvas"/> maintains.
        /// </summary>
        internal IntPtr Texture;

        /// <summary>
        /// The raw pixels in the <see cref="Canvas"/>.
        /// </summary>
        public Color[] Pixels;

        /// <summary>
        /// Gets an <see cref="IntPtr"/> that points to what should be displayed on the window's background. This is
        /// useful if you're maintaining your own ARGB framebuffer and don't plan to use DotSDL's <see cref="Canvas"/>
        /// object. You usually do not need to override this method.
        /// </summary>
        /// <remarks>If an invalid <see cref="IntPtr"/> is generated by this method, your application will
        /// crash with a segmentation fault.</remarks>
        /// <returns>An <see cref="IntPtr"/> containing the contents of the window's background.</returns>
        public Func<IntPtr> GetCanvasPointer;

        /// <summary>
        /// Gets or sets the width of the <see cref="Canvas"/> texture.
        /// </summary>
        public int Width {
            get => _width;
            set {
                if(value <= 0) throw new ArgumentException("Width must be greater than 0.");

                _width = value;
                Resize();
            }
        }

        /// <summary>
        /// Gets or sets the height of the <see cref="Canvas"/> texture.
        /// </summary>
        public int Height {
            get => _height;
            set {
                if(value <= 0) throw new ArgumentException("Height must be greater than 0.");

                _height = value;
                Resize();
            }
        }

        public BlendMode BlendMode {
            get => BlendModeValue;
            set {
                BlendModeValue = value;

                if(HasTexture)
                    Render.SetTextureBlendMode(Texture, BlendModeValue);
            }
        }

        /// <summary>
        /// Determines the method that will be used to scale this sprite when it is plotted to the
        /// screen.
        /// </summary>
        public virtual ScalingQuality ScalingQuality {
            get => ScalingQualityValue;
            set {
                ScalingQualityValue = value;

                if(HasTexture)
                    CreateTexture();
            }
        }

        /// <summary>
        /// Sets the section of the <see cref="Canvas"/> that should be drawn. If the size values are set to 0, the
        /// <see cref="Canvas"/> will fill as much of its containing object as possible.
        /// </summary>
        public Rectangle Clipping { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Canvas"/>.
        /// </summary>
        /// <param name="textureWidth">The width of the <see cref="Canvas"/>.</param>
        /// <param name="textureHeight">The height of the <see cref="Canvas"/>.</param>
        internal Canvas(int textureWidth, int textureHeight)
            : this(textureWidth, textureHeight, new Rectangle(0, 0, textureWidth, textureHeight)) { }

        /// <summary>
        /// Initializes a new <see cref="Canvas"/>.
        /// </summary>
        /// <param name="textureWidth">The width of the <see cref="Canvas"/>.</param>
        /// <param name="textureHeight">The height of the <see cref="Canvas"/>.</param>
        /// <param name="clipping">The clipping <see cref="Rectangle"/> for the <see cref="Canvas"/>.</param>
        internal Canvas(int textureWidth, int textureHeight, Rectangle clipping) {
            _width = textureWidth;
            _height = textureHeight;

            Clipping = clipping;

            GetCanvasPointer = () => {
                unsafe {
                    fixed(void* pixelPtr = Pixels) {
                        return (IntPtr)pixelPtr;
                    }
                }
            };

            Resize();
        }

        /// <summary>
        /// Creates a texture or recreates it if it already exists.
        /// </summary>
        internal virtual void CreateTexture() {
            CreateTexture(Render.TextureAccess.Streaming);
        }

        /// <summary>
        /// Creates a texture or recreates it if it already exists.
        /// </summary>
        /// <param name="textureAccess">The access mode for this texture.</param>
        internal void CreateTexture(Render.TextureAccess textureAccess) {
            if(Renderer == IntPtr.Zero) return;

            DestroyTexture();
            Hints.SetHint(Hints.RenderScaleQuality, ScalingQuality.ToString());
            Texture = Render.CreateTexture(Renderer, SdlPixels.PixelFormatArgb8888, textureAccess, Width, Height);
            HasTexture = true;

            Render.SetTextureBlendMode(Texture, BlendModeValue);
        }

        /// <summary>
        /// Destroys the texture associated with this <see cref="Sprite"/>.
        /// </summary>
        internal void DestroyTexture() {
            if(!HasTexture) return;

            Render.DestroyTexture(Texture);
            HasTexture = false;
        }

        /// <summary>
        /// Retrieves an array index on the <see cref="Canvas"/>.
        /// </summary>
        /// <param name="x">The Y coordinate of the desired location on the <see cref="Canvas"/>.</param>
        /// <param name="y">The Y coordinate of the desired location on the <see cref="Canvas"/>.</param>
        /// <returns>The array index for the given point.</returns>
        public int GetIndex(int x, int y) {
            return (Width * y) + x;
        }

        /// <summary>
        /// Retrieves an array index on the <see cref="Canvas"/>.
        /// </summary>
        /// <param name="point">A <see cref="Point"/> representing the desired location on the <see cref="Canvas"/>.</param>
        /// <returns>The array index for the given point.</returns>
        public int GetIndex(Point point) {
            return (Width * point.Y) + point.X;
        }

        /// <summary>
        /// Resizes the <see cref="Canvas"/>. Please note that this will also clear the canvas of
        /// its existing contents.
        /// </summary>
        protected void Resize() {
            Pixels = new Color[Width * Height];

            if(HasTexture)
                CreateTexture();
        }

        /// <summary>
        /// Updates the texture associated with this <see cref="Canvas"/>. This function must be called when the
        /// <see cref="Canvas.Pixels"/> array is changed.
        /// </summary>
        /// <returns><c>true</c> if the texture was successfully updated, otherwise <c>false</c>. This will return <c>false</c> if this <see cref="Sprite"/> hasn't been added to the sprite list.</returns>
        internal bool UpdateTexture() {
            if(!HasTexture) return false;
            Render.UpdateTexture(Texture, IntPtr.Zero, GetCanvasPointer(), Width * 4);
            return true;
        }
    }
}
