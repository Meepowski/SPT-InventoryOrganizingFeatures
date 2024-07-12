using System;
using System.Diagnostics;
using System.Reflection;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using UnityEngine.UI;
using SPT.Reflection.Patching;
using static Seion.Iof.UI.UserInterfaceElements;

namespace Seion.Iof.Patches
{
    internal class PostGridSortPanelShow : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridSortPanel), "Show");
        }

        [PatchPostfix]
        private static void PatchPostfix(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            try
            {
                var callerClassType = new StackTrace().GetFrame(2).GetMethod().ReflectedType;
                //NotificationManagerClass.DisplayMessageNotification($"Caller class {callerClassType.Name}");

                // For Stash panel
                // TraderDealScreen - when opening trader
                // SimpleStashPanel - in stash
                // GridWindow - when opening a container
                if (callerClassType == typeof(SimpleStashPanel))
                {
                    PatchForSimpleStashPanel(__instance, controller, item, ____button);
                    return;
                }

                // Since 3.7.1 (or whenever the new trader ui was)
                // TraderDealScreen is opened an closed several times when a opens the trade screen.
                // While the button won't clone itself if it already existrs, this behaviour should be noted.
                if (callerClassType == typeof(TraderDealScreen))
                {
                    PatchForTraderDealScreen(__instance, controller, item, ____button);
                    return;
                }

                // For Container view panel (caller class is GridWindow)
                // Hoping container panel disposes children properly.
                PatchForOtherCases(__instance, controller, item, ____button);
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }

        private static void PatchForSimpleStashPanel(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            if (OrganizeButtonStash != null)
                if (!OrganizeButtonStash.IsDestroyed()) return;
            OrganizeButtonStash = SetupOrganizeButton(____button, item, controller);
        }

        private static void PatchForTraderDealScreen(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            if (OrganizeButtonTrader != null)
                if (!OrganizeButtonTrader.IsDestroyed()) return;
            OrganizeButtonTrader = SetupOrganizeButton(____button, item, controller);
        }

        // Hopefully the "other" cases are only GridViewPanels(if I remember the name correctly)
        private static void PatchForOtherCases(GridSortPanel __instance, InventoryControllerClass controller, LootItemClass item, Button ____button)
        {
            // Setup Organize button
            var orgbtn = SetupOrganizeButton(____button, item, controller);
            orgbtn.transform.parent.GetChild(orgbtn.transform.parent.childCount - 2).SetAsLastSibling();
            // Setup Take Out button
            var takeoutbtn = SetupTakeOutButton(____button, item, controller);
            takeoutbtn.transform.parent.GetChild(orgbtn.transform.parent.childCount - 2).SetAsLastSibling();

        }
    }
}