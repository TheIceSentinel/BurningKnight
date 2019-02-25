namespace BurningKnight.entity.item.weapon.projectile {
	public class BadBullet : BulletProjectile {
		protected override void Setup() {
			base.Setup();
			NoRotation = true;
			Second = false;
			Sprite = Graphics.GetTexture("bullet-bad");
		}
	}
}