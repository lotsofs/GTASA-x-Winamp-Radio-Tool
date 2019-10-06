﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace GTASARadioExternal
{
    public class MusicPlayer
	{
        public enum HookMethods {
            None,
            SendMessage,    // for winamp, spotify
            ProcessStart,    // for foobar
            MemoryWrite     // for anything else?
        }

        Process _process;
        int _window = 0;
        int _volume = 0;
		int _addressVolume = 0x0;
        int _addressRunning = 0x0;
        HookMethods _hookMethod;


        public MusicPlayer() {
            Configure();
            GetProcess();
        }

        public void Configure() {
            // TODO: move settings file stuff here
        }

        /// <summary>
        /// Reinitializes the music player: Finds a process for the music player and configures it if found. Returns whether it found something or not.
        /// </summary>
        /// <returns></returns>
        public bool GetProcess()
		{
            _hookMethod = HookMethods.None;

            Process process = WinApi.GetProcess(Settings.ProcessName);
            if (process == null || process.HasExited) {
                _process = null;
                return false;
            }
            //if (IsRunning() == false) {
            //    _status = Statuses.Stopped;
            //}
            //else {
            //    _status = Statuses.Playing;
            //}
            _process = process;
            
            int moduleAddress = WinApi.GetModuleAddress(process, Settings.VolumeModuleName);
            if (moduleAddress == -1) {
                // requested module not found. This can happen while the process boots, or if it's not configured properly. TODO: Message
                _process = null;
                return false;
            }       // todo: There's overlap in this code with Game.cs , maybe make them inherit this from somewhere
            _addressVolume = moduleAddress + Settings.VolumeAddressOffset;


            _volume = ReadVolume();

            //moduleAddress = WinApi.GetModuleAddress(process, Settings.RunningModuleName);
            //_addressRunning = moduleAddress + Settings.RunningAddressOffset;

            if (!string.IsNullOrEmpty(Settings.WindowName)) {   // todo: add an actual setting where the user can just select this
                _hookMethod = HookMethods.SendMessage;
                _window = WinApi.FindWindow(Settings.WindowName, null);
            }
            else if (!string.IsNullOrEmpty(Settings.ProcessArguments)) {
                _hookMethod = HookMethods.ProcessStart;
                // TODO: Implement the below snippet from old code that would mute Foobar into this new code
                //ProcessStartInfo psi = new ProcessStartInfo();
                //psi.FileName = Path.GetFileName(executable_location);
                //psi.WorkingDirectory = Path.GetDirectoryName(executable_location);
                //psi.Arguments = "/command:mute";
                //Process.Start(psi);
            }
            else {
                _hookMethod = HookMethods.None;
            }
            return true;
        }

        public bool Running() {
            if (_process != null) {
                // no process found
                _process.Refresh();
            }
            else {
                return GetProcess();
            }

            if (_process.HasExited) {
                // processes exited
                return GetProcess();
            }
            return true;
        }


        /// <summary>
        /// Set radio to on or off
        /// </summary>
        /// <param name="mute">whether to turn the radio on</param>
        public void Mute(bool mute) {
            if (!Running()) {
                //throw new WarningException("No music player is running.");
            }

            // TODO: take care of all the other hookmethods for these methods
            if (!mute) {   // turn radio on
                WinApi.SendMessage(_window, 0x0400, _volume, Settings.MessageLParam); //0x0400 = WM_USER
            }
            else {      // turn radio off
                _volume = ReadVolume();
                WinApi.SendMessage(_window, 0x0400, 0, Settings.MessageLParam); //0x0400 = WM_USER
            }
        }

        /// <summary>
        /// toggles the radio on or off
        /// </summary>
        public void ToggleMute() {
            if (!Running()) {
                //throw new WarningException("No music player is running.");
            }
            // TODO: Other hookmethods

            int volume = ReadVolume();
            if (volume == 0) {  // turn radio on
                WinApi.SendMessage(_window, 0x0400, _volume, Settings.MessageLParam); //0x0400 = WM_USER
            }
            else {              // turn radio off
                _volume = volume;
                WinApi.SendMessage(_window, 0x0400, 0, Settings.MessageLParam); //0x0400 = WM_USER
            }
        }

        [Obsolete("Only exists to avoid a rare infinite recursion bug. Please deal with it where it happens")]
        bool IsPlaying() {
            if (!Running()) {
                //throw new WarningException("No music player is running.");
            }

            int playing = 0;
            try {
                playing = WinApi.ReadValue(_process, _addressRunning, Settings.RunningAddressType);
            }
            catch {
                // TODO: catch errors
            }
            return playing != 0;
        }

        int ReadVolume() {
            if (!Running()) {
                //throw new WarningException("No music player is running.");
            }

            int volume = -1;
            try {
                volume = WinApi.ReadValue(_process, _addressVolume, Settings.VolumeAddressType);
            }
            catch {
                // TODO: catch errors
            }
            Debug.WriteLine(volume);
            return volume;
        }

	}
}