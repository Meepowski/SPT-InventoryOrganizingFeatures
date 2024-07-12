using System;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using static Seion.Iof.Features.Locker;
using static Seion.Iof.Features.Organizer;
using static Seion.Iof.Features.OrganizedContainer;

namespace Seion.Iof.Patches
{
    internal class PostEditTagWindowShow : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EditTagWindow), "Show", new Type[] { typeof(TagComponent), typeof(Action), typeof(Action), typeof(Action<string, int>) });
        }

        [PatchPrefix]
        private static void PatchPrefix(ref EditTagWindow __instance, ref DefaultUIButton ____saveButtonSpawner, ValidationInputField ____tagInput)
        {
            try
            {
                ____tagInput.characterLimit = 256;
                ____saveButtonSpawner.OnClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                {
                    try
                    {
                        string notifMsg = "";
                        if (IsSortLocked(____tagInput.text)) notifMsg += "This item is Sort Locked.";
                        if (IsMoveLocked(____tagInput.text))
                        {
                            if (notifMsg.Length > 0) notifMsg += "\n";
                            notifMsg += "This item is Move Locked.";
                        }
                        if (IsOrganized(____tagInput.text))
                        {
                            if (notifMsg.Length > 0) notifMsg += "\n";
                            // Add pretty notification output
                            var orgParams = ParseOrganizeParams(____tagInput.text);
                            var categoryParams = GetCategoryParams(orgParams);
                            var nameParams = GetNameParams(orgParams);

                            notifMsg += "This item's tag has following organize params:";
                            if (HasOrderParam(orgParams))
                            {
                                notifMsg += $"\n  -  Order #{GetOrderParam(orgParams).GetValueOrDefault()}";
                            }
                            if (HasParamDefault(orgParams))
                            {
                                notifMsg += $"\n  -  Category: default container categories";
                            }
                            else if (categoryParams.Length > 0)
                            {
                                notifMsg += $"\n  -  Category: {string.Join(", ", categoryParams)}";
                            }

                            if (nameParams.Length > 0)
                            {
                                notifMsg += $"\n  -  Name: {string.Join(", ", nameParams)}";
                            }

                            if (HasParamFoundInRaid(orgParams))
                            {
                                notifMsg += "\n  -  Only \"Found in raid\".";
                            }
                            else if (HasParamNotFoundInRaid(orgParams))
                            {
                                notifMsg += "\n  -  Only \"Not found in raid.\"";
                            }
                        }
                        if (notifMsg.Length > 0) NotificationManagerClass.DisplayMessageNotification(notifMsg);
                    }
                    catch (Exception ex)
                    {
                        throw Plugin.ShowErrorNotif(ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}
