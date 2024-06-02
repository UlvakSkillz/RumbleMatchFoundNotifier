using System;
using System.Diagnostics;
using System.Media;
using System.Threading;
using MelonLoader;
using RUMBLE.Managers;
using UnityEngine;
using RumbleModUI;

namespace Match_Found_Notifier
{
    public class MatchFoundNotifierClass : MelonMod
    {
        //variables
        private string currentScene = "";
        private static Thread soundThread;
        private static bool soundThreadActive = false;
        private bool matchFoundPlayed = false;
        private MatchmakingHandler matchmakingHandler;
        private bool gymInitRan = false;
        UI UI = UI.instance;
        private Mod MatchFoundNotifier = new Mod();
        bool openImage = true;
        bool playSound = true;

        public override void OnLateInitializeMelon()
        {
            MatchFoundNotifier.ModName = "Rumble Match Found Notifier";
            MatchFoundNotifier.ModVersion = "1.1.1";
            MatchFoundNotifier.SetFolder("MatchFound");
            MatchFoundNotifier.AddDescription("Description", "Description", "Plays Sound and Opens Image on Match Found", new Tags { IsSummary = true });
            MatchFoundNotifier.AddToList("Play Sound", true, 0, "Toggle for Playing Sound", new Tags { });
            MatchFoundNotifier.AddToList("Open Image", true, 0, "Toggle for Opening Image", new Tags { });
            MatchFoundNotifier.GetFromFile();
            MatchFoundNotifier.ModSaved += Save;
            UI.instance.UI_Initialized += UIInit;
            openImage = (bool)MatchFoundNotifier.Settings[1].Value;
            playSound = (bool)MatchFoundNotifier.Settings[2].Value;
        }

        public void UIInit()
        {
            UI.AddMod(MatchFoundNotifier);
        }

        public void Save()
        {
            openImage = (bool)MatchFoundNotifier.Settings[1].Value;
            playSound = (bool)MatchFoundNotifier.Settings[2].Value;
        }

        //run every update
        public override void OnUpdate()
        {
            if ((currentScene == "Gym") && (!gymInitRan))
            {
                try
                {
                    matchmakingHandler = MatchmakingHandler.instance;
                    gymInitRan = true;
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Error with Finding MatchmakingHandler");
                    MelonLogger.Error(e.Message);
                }
            }
            else if ((currentScene == "Gym") && (matchmakingHandler.IsMatchmaking) && (!matchFoundPlayed))
            {
                try
                {
                    if (matchmakingHandler.CurrentMatchmakeStatus == MatchmakingHandler.MatchmakeStatus.Success)
                    {
                        MelonLogger.Msg("Match Found!");
                        PlayNotifications();
                        matchFoundPlayed = true;
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Error("Error with Listening to Match Found");
                    MelonLogger.Error(e.Message);
                }
            }
        }

        //run on Scene Change
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            currentScene = sceneName;
            matchFoundPlayed = false;
        }

        //Plays the Notifications
        public void PlayNotifications()
        {
            if (openImage) { PlayNotificationPhoto(); }
            if (playSound) { PlayNotificationSound(); }
        }

        //
        public void PlayNotificationPhoto()
        {
            //file path to file
            string photoFilePath = @"UserData\MatchFound\NotificationPhoto.png";
            //open photo
            OpenExternalProgram(photoFilePath, "");
        }

        //Plays the Notification Sound
        public void PlayNotificationSound()
        {
            //file path to notification sound
            string soundFilePath = @"UserData\MatchFound\NotificationSound.wav";
            try
            {
                // Ensure that only one sound is playing at a time
                if (soundThreadActive)
                {
                    MelonLogger.Msg("Sound is already playing. Ignoring request.");
                    return;
                }

                // Create a SoundPlayer instance with the specified sound file path
                using (SoundPlayer player = new SoundPlayer(soundFilePath))
                {
                    // Set flag to indicate that a sound is currently playing
                    soundThreadActive = true;

                    // Create a new thread if no thread is active
                    if (soundThread == null || !soundThread.IsAlive)
                    {
                        soundThread = new Thread(() =>
                        {
                            player.PlaySync(); // Use PlaySync for synchronous playback

                            // Reset flag to indicate that the sound has finished playing
                            soundThreadActive = false;
                        });

                        // Start the thread
                        soundThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"Error playing sound: {ex.Message}");
            }
        }

        //Opens External Programs (with commands)
        private void OpenExternalProgram(string programPath, string parameters)
        {
            //setup program to open
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = programPath,
                Arguments = parameters,
                UseShellExecute = true
            };
            try
            {
                //open program
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"-Error opening external program: {ex.Message}");
            }
        }
    }
}
