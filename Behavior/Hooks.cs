using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.ShatteringStrawberries {
    public static class Hooks {
        #region Used methods

        private static readonly MethodInfo Strawberry_CollectRountine
            = typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.Instance | BindingFlags.NonPublic)
                                .GetStateMachineTarget();

        private static readonly MethodInfo Entity_RemoveSelf
            = typeof(Entity).GetMethod("RemoveSelf", BindingFlags.Instance | BindingFlags.Public);

        #endregion

        private static ILHook Strawberry_CollectRountine_ILHook;

        internal static void Hook() {
            Strawberry_CollectRountine_ILHook = new ILHook(Strawberry_CollectRountine, Mod_Strawberry_CollectRountine);
        }

        internal static void Unhook() {
            Strawberry_CollectRountine_ILHook.Dispose();
        }

        private static void Mod_Strawberry_CollectRountine(ILContext il) {
            ILCursor cursor = new(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(Entity_RemoveSelf));
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(Shattering.OnShatter);
        }
    }
}
