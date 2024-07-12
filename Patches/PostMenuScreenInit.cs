using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using SPT.Reflection.Patching;
using Seion.Iof.Reflection;
using static Seion.Iof.UI.UserInterfaceElements;

namespace Seion.Iof.Patches
{
    internal class PostMenuScreenInit : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuScreen), "Init");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref DefaultUIButton ____hideoutButton)
        {
            try
            {
                if (OrganizeSprite != null) return;
                //OrganizeSprite = AccessTools.Field(____hideoutButton.GetType(), "_iconSprite").GetValue(____hideoutButton) as Sprite;
                OrganizeSprite = ____hideoutButton.GetFieldValue<Sprite>("_iconSprite");
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}