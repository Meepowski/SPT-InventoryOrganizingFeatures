using System;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using Seion.Iof.Reflection;
using static Seion.Iof.Features.Locker;

namespace Seion.Iof.Patches
{
    internal class PreQuickFindAppropriatePlace : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ItemUiContext), "QuickFindAppropriatePlace");
        }

        [PatchPrefix]
        private static void PatchPrefix(object itemContext, ref bool displayWarnings)
        {
            try
            {
                var item = itemContext.GetFieldValue<Item>("Item");
                // Don't display warnings if item IsMoveLocked
                if (IsMoveLocked(item)) displayWarnings = false;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}