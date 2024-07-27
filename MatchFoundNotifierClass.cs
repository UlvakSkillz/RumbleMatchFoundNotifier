using System;
using System.Diagnostics;
using MelonLoader;
using Il2CppRUMBLE.Managers;
using UnityEngine;
using RumbleModUI;
using System.Collections;
using NAudio.Wave;
using MelonLoader.Utils;

namespace Match_Found_Notifier
{
    public class MatchFoundNotifierClass : MelonMod
    {
        //variables
        private string currentScene = "";
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
            MatchFoundNotifier.ModVersion = "2.0.1";
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

        private IEnumerator PlaySound(string FilePath)
        {
            var reader = new Mp3FileReader(FilePath);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                yield return new WaitForFixedUpdate();
            }
            yield break;
        }

        //Plays the Notification Sound
        public void PlayNotificationSound()
        {
            //file path to notification sound
            string soundFilePath = MelonEnvironment.UserDataDirectory + @"\MatchFound\NotificationSound.mp3";
            try
            {
                // Ensure that only one sound is playing at a time
                if (soundThreadActive)
                {
                    MelonLogger.Msg("Sound is already playing. Ignoring request.");
                    return;
                }
                MelonCoroutines.Start(PlaySound(soundFilePath));
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
