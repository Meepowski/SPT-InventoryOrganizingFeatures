using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using SPT.Reflection.Patching;
using static Seion.Iof.UI.UserInterfaceElements;

namespace Seion.Iof.Patches
{
    internal class PostSimpleStashPanelClose : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SimpleStashPanel), "Close");
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            try
            {
                if (OrganizeButtonStash == null) return;
                if (OrganizeButtonStash.IsDestroyed()) return;

                OrganizeButtonStash.gameObject.SetActive(false);
                GameObject.Destroy(OrganizeButtonStash);

                // Might need it.
                //GameObject.DestroyImmediate(OrganizeButton);
                //OrganizeButton = null;
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}