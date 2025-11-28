using FMOD;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace OHOHit
{
    public static class Setting
    {
        public static string OhoHitEfx { get; private set; }
        public static string OhoDeadEfx { get; private set; }
        public static bool OnlyOHOSelf { get; private set; }
        public static float OnlyOHOSelfThreshold { get; private set; }
        public static bool DontOhoSoFast { get; private set; }
        public static float HitSoundVolume { get; private set; }
        public static bool DisableHitSound { get; private set; }
        public static float DeadSoundVolume { get; private set; }
        public static bool DisableDeadSound { get; private set; }
        public static float DamageThreshold { get; private set; }
        public static float OHoInterval { get; private set; }
        public static float AudioMinDistance { get; private set; }
        public static float AudioMaxDistance { get; private set; }
        public static void SetOhoHitEfx(string value) => OhoHitEfx = value;
        public static void SetOhoDeadEfx(string value) => OhoDeadEfx = value;
        public static void SetOnlyOHOSelfThreshold(float value) => OnlyOHOSelfThreshold = value;
        public static void SetOnlyOHOSelf(bool value) => OnlyOHOSelf = value;
        public static void SetDamageThreshold(float value) =>DamageThreshold = value;
        public static void SetDontOhoSoFast(bool value) => DontOhoSoFast = value;
        public static void SetOHoInterval(float value) => OHoInterval = value;
        public static void SetAudioMinDistance(float value)=>AudioMinDistance = value;
        public static void SetAudioMaxDistance(float value) => AudioMaxDistance = value;
        public static event Action<float> OnHitSoundVolumeChanged;
        public static void SetHitSoundVolume(float value)
        {
            HitSoundVolume = value;
            OnHitSoundVolumeChanged.Invoke(value);
        }
        public static void SetDisableHitSound(bool value) => DisableHitSound = value;


        public static event Action<float> OnDeadSoundVolumeChanged;
        public static void SetDeadSoundVolume(float value)
        {
            DeadSoundVolume = value;
            OnDeadSoundVolumeChanged.Invoke(value);
        }
        public static void SetDisableDeadSound(bool value) => DisableDeadSound = value;

        public static void Clear()
        {
            OnHitSoundVolumeChanged=null;
            OnDeadSoundVolumeChanged = null;
        }

        public static void Init()
        {
            if (ModSettingAPI.HasConfig())
            {
                DisableHitSound = ModSettingAPI.GetSavedValue("HitSound", out bool t0) ? t0 : true;
                OnlyOHOSelf = ModSettingAPI.GetSavedValue("OhoSelf", out bool t1) ? t1 : false;
                OnlyOHOSelfThreshold = ModSettingAPI.GetSavedValue("OnlyOHOSelfThreshold", out float t2) ? t2 : 35f;
                DamageThreshold = ModSettingAPI.GetSavedValue("DamageThreshold", out float t3) ? t3 : 35f;
                DisableDeadSound = ModSettingAPI.GetSavedValue("DeadSound", out bool t5) ? t5 : true;
                DontOhoSoFast = ModSettingAPI.GetSavedValue("DontOhoSoFast", out bool t6) ? t6 : false;
                OHoInterval = ModSettingAPI.GetSavedValue("OHoInterval", out float t7) ? t7 : 0.5f;
                OhoHitEfx = ModSettingAPI.GetSavedValue("OhoHitEfx", out string d1) ? d1 : "哦齁特效";
                OhoDeadEfx = ModSettingAPI.GetSavedValue("OhoDeadEfx", out string d2) ? d2 : "正常死亡特效";
                DeadSoundVolume = ModSettingAPI.GetSavedValue("DeadSoundVolume", out float t8) ? t8 : 1f;
                HitSoundVolume = ModSettingAPI.GetSavedValue("HitSoundVolume", out float t4) ? t4 : 1f;
                AudioMinDistance = ModSettingAPI.GetSavedValue("AudioMinDistance", out float t9) ? t9 : 0.8f;
                AudioMaxDistance = ModSettingAPI.GetSavedValue("AudioMaxDistance", out float t10) ? t10 : 5f;
            }
            else
            {
                DisableHitSound = false;
                DisableDeadSound = false;
                OnlyOHOSelf = false;
                DamageThreshold = 35f;
                DontOhoSoFast = false;
                OHoInterval = 0.5f;
                OhoHitEfx = "哦齁特效";
                OhoDeadEfx = "正常死亡特效";
                OnlyOHOSelfThreshold = 35f;
                HitSoundVolume = 100f;
                DeadSoundVolume = 100f;
                AudioMinDistance = 0.8f;
                AudioMaxDistance = 5f;
            }
        }
    }
}