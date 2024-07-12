using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using Seion.Iof.Reflection;
using Seion.Iof.Reflection.Extensions;
using Seion.Iof.Features;
using Seion.Iof.Patches;

namespace Seion.Iof
{
    [BepInPlugin("com.seion.iof", "Seion.Iof", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool EnableLogs = false;
        public static ManualLogSource GlobalLogger;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin com.seion.iof is loaded!");
            GlobalLogger = Logger;
            ReflectionHelper.Logger = Logger;

            // Pre intialize static reflection classes
            // since some of "method name" based searches can take a second or so,
            // and it's pretty noticable when playing (well... being in your stash, doing stuff)
            // E.g. First time you click on the "Organize" button it hangs for about a second.
            // Every other time is quick.
            RuntimeHelpers.RunClassConstructor(typeof(LocaleHelper).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(ItemTransactionHelper).TypeHandle); // this one especially.

            OrganizedContainer.Logger = Logger;

            // Pull handbook from the init method.
            new PostInitHanbook().Enable();

            // Pre-load image from hideout button for organize button
            new PostMenuScreenInit().Enable();
            
            // Assign tag and show active tags when saving EditTagWindow.
            new PostEditTagWindowShow().Enable();

            // Sort lock
            new PreGridClassRemoveAll().Enable(); // Prevent Sorting

            // Move lock
            new PreItemViewOnPointerDown().Enable(); // Prevent Drag
            new PreItemViewOnBeginDrag().Enable(); // Prevent Drag
            new PostGetFailedProperty().Enable(); // Prevent quick move(Ctrl/Shift+Click)
            new PreQuickFindAppropriatePlace().Enable(); // Don't show warnings when item is Move Locked

            // Clone sort button and make it an organize button
            new PostGridSortPanelShow().Enable();

            // Clean up the buttons. Perhaps unnecessary, but I'll leave it here for now
            new PostSimpleStashPanelClose().Enable();
            new PostTraderScreensGroupClose().Enable();
        }

        private static HashSet<string> AlreadyThrownPatches = new HashSet<string>();
        public static Exception ShowErrorNotif(Exception ex)
        {
            if (!AlreadyThrownPatches.Add(ex.Source))
            {
                return ex;
            }

            NotificationManagerClass.DisplayWarningNotification(
                $"Seion.Iof thew an exception. Perhaps version incompatibility? Exception: {ex.Message}",
                duration: EFT.Communications.ENotificationDurationType.Infinite
                );

            return ex;
        }
    }
}
