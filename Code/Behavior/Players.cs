namespace Celeste.Mod.ShatteringStrawberries.Behavior;

public static class Players
{
    public static void OnShatter(this PlayerDeadBody body)
    {
        Level level = body.SceneAs<Level>();
        level.Shake();
        level.Displacement.AddBurst(body.Center, 0.4f, 12f, 36f, 0.5f);

        Audio.Play(SFX.game_10_puffer_splode, body.Center);
    }
}
