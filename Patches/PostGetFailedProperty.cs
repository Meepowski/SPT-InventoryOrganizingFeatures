using System;
using System.Diagnostics;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using Seion.Iof.Reflection;
using static Seion.Iof.Features.Locker;

namespace Seion.Iof.Patches
{
    internal class PostGetFailedProperty : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(AccessTools.Method(typeof(ItemUiContext), "QuickFindAppropriatePlace").ReturnType, "Failed");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref object __instance, ref bool __result)
        {
            try
            {
                if (__instance == null) return;

                // Make sure to only execute if called for ItemView, OnClick method.
                var callerMethod = new StackTrace().GetFrame(2).GetMethod();
                if (callerMethod.Name.Equals("OnClick") && callerMethod.ReflectedType == typeof(ItemView))
                {
                    Item item = null;
                    var traverse = Traverse.Create(__instance);
                    // When __instance is just moved (GClass2441 - SPT AKI 3.5.5)
                    if (traverse.Property("Item").PropertyExists())
                        item ??= __instance.GetPropertyValueOrDefault<Item>("Item");
                    // When __instance is moved and merged(stacked) (GClass2443 - SPT AKI 3.5.5)
                    if (traverse.Field("Item").FieldExists())
                        item ??= __instance.GetFieldValueOrDefault<Item>("Item");
                    if (item == null)
                    {
                        Logger.LogWarning($"Seion.Iof Error | Patch@ {__instance.GetType()} Getter of Property \"Failed\": Item is still null. Skipping patch.");
                        NotificationManagerClass.DisplayWarningNotification($"Seion.Iof Error | {__instance.GetType()} Item is still null. You should probably send your bepinex logs to mod developer.");
                        return;
                    }; // null safety
                    if (item.TryGetItemComponent(out TagComponent tagComp))
                    {
                        if (IsMoveLocked(tagComp.Name)) __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}