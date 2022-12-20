using Celeste.Mod.ShatteringStrawberries.Behavior;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.ShatteringStrawberries
{
    public static class Hooks
    {
        #region Used methods & types

        private static readonly MethodInfo m_Strawberry_CollectRountine
            = typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.Instance | BindingFlags.NonPublic)
                                .GetStateMachineTarget();

        private static readonly MethodInfo m_Entity_RemoveSelf
            = typeof(Entity).GetMethod(nameof(Entity.RemoveSelf), BindingFlags.Instance | BindingFlags.Public);

        private static readonly MethodInfo m_PlayerDeadBody_DeathRountine
            = typeof(PlayerDeadBody).GetMethod("DeathRoutine", BindingFlags.Instance | BindingFlags.NonPublic)
                                    .GetStateMachineTarget();

        private static readonly FieldInfo f_PlayerDeadBody_DeathRoutine
            = typeof(PlayerDeadBody).GetNestedType("<DeathRoutine>d__15", BindingFlags.NonPublic)
                                    .GetField("<>2__current", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo f_PlayerDeadBody_player
            = typeof(PlayerDeadBody).GetField("player", BindingFlags.Instance | BindingFlags.NonPublic);

        #endregion

        private static ILHook IL_Strawberry_CollectRountine;
        private static ILHook IL_PlayerDeadBody_DeathRountine;

        internal static void Hook()
        {
            IL_Strawberry_CollectRountine = new ILHook(m_Strawberry_CollectRountine, Mod_Strawberry_CollectRountine);
            IL_PlayerDeadBody_DeathRountine = new ILHook(m_PlayerDeadBody_DeathRountine, Mod_PlayerDeadBody_DeathRountine);
        }

        internal static void Unhook()
        {
            IL_Strawberry_CollectRountine.Dispose();
            IL_PlayerDeadBody_DeathRountine.Dispose();
        }

        private static void Mod_Strawberry_CollectRountine(ILContext il)
        {
            ILCursor cursor = new(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt(m_Entity_RemoveSelf));
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(Strawberries.OnShatter);
        }

        private static void Mod_PlayerDeadBody_DeathRountine(ILContext il)
        {
            ILCursor cursor = new(il);
            cursor.GotoNext(MoveType.After, instr => instr.MatchStfld<DeathEffect>(nameof(DeathEffect.OnUpdate)));

            Instruction skip = cursor.Clone()
                                     .GotoNext(MoveType.After, instr => instr.MatchStfld(f_PlayerDeadBody_DeathRoutine))
                                     .Next;

            Instruction next = cursor.Next;

            cursor.EmitDelegate(() => ShatteringStrawberriesModule.Settings.PlayerExplosion);
            cursor.Emit(OpCodes.Brfalse_S, next);

            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(Players.OnShatter);
            
            cursor.Emit(OpCodes.Br_S, skip);
        }
    }
}
