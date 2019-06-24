﻿using Lens.entity.component.logic;
using Lens.util.camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using SharpDX.Direct2D1.Effects;

namespace Lens.graphics.gamerenderer {
	public class PixelPerfectGameRenderer : GameRenderer {
		public Batcher2D Batcher2D;

		private Matrix One = Matrix.Identity;
		private Matrix UiScale = Matrix.Identity;

		private bool inUi;
		
		public PixelPerfectGameRenderer() {
			GameTarget = new RenderTarget2D(
				Engine.GraphicsDevice, Display.Width + 1, Display.Height + 1, false,
				Engine.Graphics.PreferredBackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents
			);
			
			Batcher2D = new Batcher2D(Engine.GraphicsDevice);
		}

		public override void Begin() {
			if (inUi) {
				BeginUi();
				return;
			}
			
			Graphics.Batch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, DefaultRasterizerState, SurfaceEffect, Camera.Instance?.Matrix ?? One);
		}

		public override void End() {
			Graphics.Batch.End();
		}
		
		public void BeginShadows() {
			Engine.GraphicsDevice.SetRenderTarget(UiTarget);
			Graphics.Batch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, DefaultRasterizerState, SurfaceEffect, Camera.Instance?.Matrix ?? One);
			Graphics.Clear(Color.Transparent);
		}

		public void EndShadows() {
			Graphics.Batch.End();
			Engine.GraphicsDevice.SetRenderTarget(GameTarget);
		}

		private void RenderGame() {
			Engine.GraphicsDevice.SetRenderTarget(GameTarget);
			Begin();
			Graphics.Clear(Bg);
			Engine.Instance.State?.Render();
			End();
		}

		private void BeginUi() {
			Engine.GraphicsDevice.SetRenderTarget(UiTarget);
			Graphics.Batch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, DefaultRasterizerState, SurfaceEffect, UiScale);
		}

		private void RenderUi() {
			if (UiTarget == null) {
				return;
			}

			BeginUi();
			Graphics.Clear(Color.Transparent);
			Engine.Instance.State?.RenderUi();

			if (Engine.Instance.Flash > 0) {
				Graphics.Clear(Engine.Instance.FlashColor);
			}
			
			End();
		}
		
		public override void Render() {
			var start = EnableBatcher;
			
			if (start) {
				Batcher2D.Begin();
			}
			
			RenderGame();
			inUi = true;
			RenderUi();
			inUi = false;

			Engine.GraphicsDevice.SetRenderTarget(null);
			Engine.GraphicsDevice.ScissorRectangle = new Rectangle((int) Engine.Viewport.X, (int) Engine.Viewport.Y, (int) (Display.Width * Engine.Instance.Upscale), (int) (Display.Height * Engine.Instance.Upscale));

		
			Graphics.Batch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, ClipRasterizerState, GameEffect, One);

			if (Camera.Instance != null) {
				var shake = Camera.Instance.GetComponent<ShakeComponent>();
				var scale = Engine.Instance.Upscale * Camera.Instance.TextureZoom; 

				Graphics.Render(GameTarget,
					new Vector2(Engine.Viewport.X + Display.Width / 2f * Engine.Instance.Upscale + scale * shake.Position.X,
						Engine.Viewport.Y + Display.Height / 2f * Engine.Instance.Upscale + scale * shake.Position.Y),
					shake.Angle,
					new Vector2(Camera.Instance.Position.X % 1 + Display.Width / 2f,
						Camera.Instance.Position.Y % 1 + Display.Height / 2f),
					new Vector2(scale));
			}

			Graphics.Batch.End();

			if (UiTarget != null) {
				Graphics.Batch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, ClipRasterizerState, UiEffect, One);
			
				Graphics.Color = new Color(0, 0, 0, 0.5f);
				Graphics.Render(UiTarget, Engine.Viewport + new Vector2(0, Engine.Instance.UiUpscale));
				Graphics.Color = ColorUtils.WhiteColor;
				
				Graphics.Render(UiTarget, Engine.Viewport);
				
				Graphics.Batch.End();
			}

			Engine.Instance.State?.RenderNative();

			if (start) {
				Batcher2D.End();
			}
		}

		public override void Resize(int width, int height) {
			base.Resize(width, height);
			
			UiTarget = new RenderTarget2D(
				Engine.GraphicsDevice, (int) (Display.UiWidth * Engine.Instance.Upscale), (int) (Display.UiHeight * Engine.Instance.Upscale), false,
				Engine.Graphics.PreferredBackBufferFormat, DepthFormat.Depth24
			);

			UiScale = Matrix.Identity * Matrix.CreateScale(Engine.Instance.UiUpscale);
		}

		public override void Destroy() {
			base.Destroy();
			
			GameTarget.Dispose();
			UiTarget?.Dispose();
			Batcher2D.Dispose();
		}
	}
}