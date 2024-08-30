using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;

namespace IgnoreConfigErrors {
    public class IgnoreConfigErrorsMod: Mod {
        public IgnoreConfigErrorsMod(ModContentPack content) : base(content) {
            Log.Message("[IgnoreConfigErrors] Now active");
            var harmony = new Harmony("kaitorisenkou.IgnoreConfigErrors");
            
            var innerType_AbilityStopMentalState =
                typeof(CompProperties_AbilityStopMentalState)
                .GetNestedTypes(AccessTools.all)
                .FirstOrDefault((Type t) => t.Name.Contains("<ConfigErrors>"));
            harmony.Patch(
                AccessTools.Method(innerType_AbilityStopMentalState, "MoveNext", null, null),
                null,
                null,
                new HarmonyMethod(typeof(IgnoreConfigErrorsMod), nameof(Patch_AbilityStopMentalState), null),
                null
                );
            var innerType_VerbProperties =
                typeof(VerbProperties)
                .GetNestedTypes(AccessTools.all)
                .FirstOrDefault((Type t) => t.Name.Contains("<ConfigErrors>"));
            harmony.Patch(
                AccessTools.Method(innerType_VerbProperties, "MoveNext", null, null),
                null,
                null,
                new HarmonyMethod(typeof(IgnoreConfigErrorsMod), nameof(Patch_VerbProperties), null),
                null
                );

            Log.Message("[IgnoreConfigErrors] Harmony patch complete!");
        }


        static IEnumerable<CodeInstruction> Patch_AbilityStopMentalState(IEnumerable<CodeInstruction> instructions) {
            var instructionList = instructions.ToList();
            int patchCount = 0;
            FieldInfo targetInfo1 = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForMinor");
            FieldInfo targetInfo2 = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForMajor");
            FieldInfo targetInfo3 = AccessTools.Field(typeof(CompProperties_AbilityStopMentalState), "psyfocusCostForExtreme");
            var innerType = typeof(CompProperties_AbilityStopMentalState).GetNestedTypes(AccessTools.all).First(t => t.Name.Contains("<ConfigErrors>"));
            FieldInfo fieldInfo_parentDef = innerType.GetFields(AccessTools.all).First(t => t.Name.Contains("parentDef"));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo1 ||
                    instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo2 ||
                    instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == targetInfo3 ) {
                    instructionList.InsertRange(i + 3, new CodeInstruction[]{
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,fieldInfo_parentDef),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(IgnoreConfigErrorsMod),nameof(HasModExt_AbilityStopMentalState))),
                        new CodeInstruction(OpCodes.Brtrue_S, instructionList[i+2].operand)
                    });
                    patchCount++;
                }
            }
            if (patchCount < 3) {
                Log.Error("[IgnoreConfigErrors] Patch_AbilityStopMentalState seems failed! (patchCount: "+patchCount+")");
            }
            return instructionList;
        }
        static bool HasModExt_AbilityStopMentalState(AbilityDef parentDef) {
#if DEBUG
            Log.Message("[IgnoreConfigErrors] HasModExt_AbilityStopMentalState");
#endif
            return parentDef.HasModExtension<Ignore_AbilityStopMentalState>();
        }


        static IEnumerable<CodeInstruction> Patch_VerbProperties(IEnumerable<CodeInstruction> instructions) {
            var instructionList = instructions.ToList();
            int patchCount = 0;
            MethodInfo targetInfo = AccessTools.PropertyGetter(typeof(VerbProperties), nameof(VerbProperties.LaunchesProjectile));
            var innerType =typeof(VerbProperties).GetNestedTypes(AccessTools.all).FirstOrDefault((Type t) => t.Name.Contains("<ConfigErrors>"));
            FieldInfo fieldInfo_parent = innerType.GetFields(AccessTools.all).First(t => t.Name.Contains("parent"));
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Call && (MethodInfo)instructionList[i].operand == targetInfo) {
                    instructionList.InsertRange(i + 2, new CodeInstruction[]{
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld,fieldInfo_parent),
                        new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(IgnoreConfigErrorsMod),nameof(HasModExt_ForcedMissRadius))),
                        new CodeInstruction(OpCodes.Brtrue_S, instructionList[i+1].operand)
                    });
                    patchCount++;
                    break;
                }
            }
            if (patchCount < 1) {
                Log.Error("[IgnoreConfigErrors] Patch_VerbProperties seems failed!");
            }
            return instructionList;
        }
        static bool HasModExt_ForcedMissRadius(ThingDef parentDef) {
#if DEBUG
            Log.Message("[IgnoreConfigErrors] HasModExt_ForcedMissRadius");
#endif
            return parentDef.HasModExtension<Ignore_ForcedMissRadius>();
        }
    }

    public class Ignore_AbilityStopMentalState : DefModExtension { }
    public class Ignore_ForcedMissRadius : DefModExtension { }
    
}