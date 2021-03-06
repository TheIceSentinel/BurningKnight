using System;
using BurningKnight.assets.particle;
using BurningKnight.entity.component;
using BurningKnight.entity.creature;
using BurningKnight.entity.creature.player;
using BurningKnight.entity.events;
using BurningKnight.entity.item;
using BurningKnight.entity.item.stand;
using BurningKnight.ui.editor;
using Lens;
using Lens.entity;
using Lens.util.camera;
using Lens.util.tween;
using Microsoft.Xna.Framework;

namespace BurningKnight.level.entities {
	public class BreakableProp : SolidProp, PlaceableEntity {
		public static string[] Infos = {
			"pot_a",
			"pot_b",
			"pot_c",
			"pot_d",
			"pot_e",

			"cup",
			
			"chair_a",
			"chair_b",
			"chair_c",
			
			"crate_a",
			"crate_b"
		};
		
		public static string[] IceInfos = {
			"cup",
			
			"gift_a",
			"gift_b",
			"gift_c"
		};

		private Entity from;
		private bool hurts;

		public override void AddComponents() {
			base.AddComponents();
			
			AddComponent(new ShadowComponent(RenderShadow));
			AddComponent(new HealthComponent {
				InitMaxHealth = 1,
				RenderInvt = true,
				AutoKill = false
			});

			AddComponent(new ExplodableComponent());
		}

		private void RenderShadow() {
			GraphicsComponent.Render(true);
		}

		protected override Rectangle GetCollider() {
			var rect = GetComponent<SliceComponent>().Sprite.Source;

			/*if (Sprite.Contains("pot") || Sprite.Contains("crate")) {
				AddComponent(new PoolDropsComponent(ItemPool.Crate, 0.3f, 1, 3));
			}*/
			
			hurts = Sprite == "cactus";

			Width = rect.Width;
			Height = rect.Height;
			
			return new Rectangle(hurts ? 2 : 0, 0, (int) (hurts ? Width - 4 : Width), (int) (hurts ? Height - 6 : Height));
		}

		public override bool HandleEvent(Event e) {
			if (e is HealthModifiedEvent ev) {
				var h = GetComponent<HealthComponent>();
				
				if (Math.Abs(h.Health + ev.Amount) < 0.1f) {
					from = ev.From;
				}
			} else if (e is CollisionStartedEvent c && hurts) {
				if (c.Entity is Player) {
					c.Entity.GetComponent<HealthComponent>().ModifyHealth(-1, this);
					c.Entity.GetAnyComponent<BodyComponent>().KnockbackFrom(this, 1);
				}
			}
			
			return base.HandleEvent(e);
		}

		public override void Update(float dt) {
			base.Update(dt);

			if (from != null && TryGetComponent<HealthComponent>(out var h) && h.InvincibilityTimer <= 0.45f) {
				Done = true;

				if (!Camera.Instance.Overlaps(this)) {
					return;
				}

				if (TryGetComponent<PoolDropsComponent>(out var pool)) {
					pool.SpawnDrops();
				}
				
				for (var i = 0; i < 4; i++) {
					var part = new ParticleEntity(Particles.Dust());
						
					part.Position = Center;
					part.Particle.Scale = Lens.util.math.Rnd.Float(0.4f, 0.8f);
					
					Area.Add(part);
				}

				var d = AudioEmitterComponent.Dummy(Area, Center);
				
				if (Sprite == "cup" || Sprite.StartsWith("pot")) {
					d.EmitRandomizedPrefixed("level_cup", 2, 0.75f);
				} else {
					d.EmitRandomizedPrefixed("level_chair_break", 2, 0.75f);
				}

				Particles.BreakSprite(Area, GetComponent<SliceComponent>().Sprite, Position);
				Camera.Instance.Shake(2f);
				Engine.Instance.Freeze = 1f;
			}
		}

		public override bool ShouldCollide(Entity entity) {
			return entity is Level || entity is Chasm || entity is SolidProp || entity is HalfWall || entity is ItemStand;
		}
	}
}