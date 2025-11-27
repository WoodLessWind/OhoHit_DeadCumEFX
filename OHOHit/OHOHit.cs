using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using FMOD;
using static FMODUnity.RuntimeManager;
using Debug = UnityEngine.Debug;
using Duckov;
using Duckov.Modding;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace OHOHit
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private AssetBundle efxAssetBundle;
        private Sound[] hitAudios;
        private Sound[] deadAudios;
        private ChannelGroup masterSfxGroup;
        private SoundGroup soundHitGroup;
        private SoundGroup soundDeadGroup;
        private GameObject ohoEfx;
        private GameObject ohoEfxUncritical;
        private GameObject ohoEfxDeadNormal;
        private GameObject ohoEfxDeadLarge;
        private String efxPath = Path.Combine(GetModPath(), "EFX");
        private static String audioPath = Path.Combine(GetModPath(), "Audio");
        private String audioHitPath = Path.Combine(audioPath, "Hit");
        private String audioDeadPath = Path.Combine(audioPath, "Dead");
        private float oHoInterval = 1f;
        protected override void OnAfterSetup()
        {

            LoadAssetBundles();
            ohoEfx.AddComponent<DestroyAfterPlayedFiveSec>();
            ohoEfxUncritical.AddComponent<DestroyAfterPlayedFiveSec>();
            ohoEfxDeadNormal.AddComponent<DestroyAfterPlayedFiveSec>();
            ohoEfxDeadLarge.AddComponent<DestroyAfterPlayedFiveSec>();
            masterSfxGroup = new ChannelGroup();
            if (audioHitPath!=null)
            {
                string[] filesHit = Directory.GetFiles(audioHitPath, "*.wav", SearchOption.AllDirectories);
                string[] filesDead = Directory.GetFiles(audioDeadPath, "*.wav", SearchOption.AllDirectories);
                CoreSystem.createSoundGroup("OhoHitSoundGroup", out soundHitGroup);
                hitAudios = new Sound[filesHit.Length];
                for (int i = 0; i < filesHit.Length; i++)
                {
                    CoreSystem.createSound(filesHit[i], MODE._3D, out hitAudios[i]);
                    hitAudios[i].set3DMinMaxDistance(0.8f, 5f);
                    hitAudios[i].setSoundGroup(soundHitGroup);
                }

                CoreSystem.createSoundGroup("OhoDeadSoundGroup", out soundDeadGroup);
                deadAudios = new Sound[filesDead.Length];
                for(int i = 0;i < filesDead.Length; i++)
                {
                    CoreSystem.createSound(filesDead[i], MODE._3D, out deadAudios[i]);
                    deadAudios[i].set3DMinMaxDistance(0.8f, 5f);
                    deadAudios[i].setSoundGroup(soundDeadGroup);
                }
            }
            else
            {
                Debug.Log("没有找到音频文件夹");
            }

            Debug.Log("[哦齁受击]已启用");
            
            if (!ModSettingAPI.Init(info)) return;
            Setting.Init();
            AddUI();
        }

        protected override void OnBeforeDeactivate()
        {
            efxAssetBundle.Unload(true);
            hitAudios = null;
            deadAudios = null;
            Setting.Clear();
            Debug.Log("[哦齁受击]已卸载");

        }
        void OnEnable()
        {
            ModManager.OnModActivated += ModManager_OnModActivated;
            Setting.OnHitSoundVolumeChanged += HitVolumeChange;
            Setting.OnDeadSoundVolumeChanged += DeadVolumeChange;
        }

        void OnDisable()
        {

            ModManager.OnModActivated -= ModManager_OnModActivated;

        }
        void Awake()
        {
            LevelManager.OnAfterLevelInitialized += OhoHitInit;
            LevelManager.OnLevelBeginInitializing += OhoHitDeinit;
        }
        void FixedUpdate()
        {
            oHoInterval -= Time.fixedDeltaTime;
        }


        private void AddUI()
        {
            ModSettingAPI.AddSlider("HitSoundVolume", "击中音效音量", Setting.HitSoundVolume, new Vector2(0, 100), Setting.SetHitSoundVolume);
            ModSettingAPI.AddToggle("HitSound", "关闭击中音效", Setting.DisableHitSound, Setting.SetDisableHitSound);
            ModSettingAPI.AddSlider("DeadSoundVolume", "死亡音效音量", Setting.DeadSoundVolume, new Vector2(0, 100), Setting.SetDeadSoundVolume);
            ModSettingAPI.AddToggle("DeadSound", "关闭死亡音效", Setting.DisableDeadSound, Setting.SetDisableHitSound);
            ModSettingAPI.AddDropdownList("OhoHitEfx", "选择击中特效", new List<string> { "关闭特效", "关闭哦齁特效", "哦齁特效" }, Setting.OhoHitEfx, Setting.SetOhoHitEfx);
            ModSettingAPI.AddDropdownList("OhoDeadEfx", "选择死亡特效", new List<string> { "关闭特效", "正常死亡特效", "大型死亡特效" }, Setting.OhoDeadEfx, Setting.SetOhoDeadEfx);
            ModSettingAPI.AddSlider("DamageThreshold", "触发伤害阈值:直接输入→", Setting.DamageThreshold, new Vector2(0, 10000), Setting.SetDamageThreshold);
            ModSettingAPI.AddSlider("OnlyOHOSelfThreshold", "仅对自己触发阈值:直接输入→", Setting.OnlyOHOSelfThreshold, new Vector2(0, 10000), Setting.SetOnlyOHOSelfThreshold);
            ModSettingAPI.AddToggle("OhoSelf", "我只想我自己哦齁", Setting.OnlyOHOSelf, Setting.SetOnlyOHOSelf);
            ModSettingAPI.AddToggle("DontOhoSoFast", "别齁那么快，来点间隔", Setting.DontOhoSoFast, Setting.SetDontOhoSoFast);
            ModSettingAPI.AddSlider("OHoInterval", "间隔时间:/秒", Setting.OHoInterval, new Vector2(0, 10), Setting.SetOHoInterval);
        }
        private void OhoHitEfx(Health health,DamageInfo damageInfo)
        {
            Vector3 hitPoint = health.transform.position;
            hitPoint.y += 1.0f;
            Debug.Log($"受到了伤害：{damageInfo.damageValue}");
            Debug.Log($"是否暴击：{damageInfo.crit}");
            switch (damageInfo.crit)
            {
                case 0 when damageInfo.damageValue < Setting.DamageThreshold:
                    if (Setting.OhoHitEfx == "关闭特效")break;
                    // 低伤害非暴击：仅播放非暴击特效
                    HandleHitEffects(hitPoint, health,Setting.OnlyOHOSelf,damageInfo);
                    break;

                case 0:
                case 1:
                    HandleHitEffects(hitPoint,health,Setting.OnlyOHOSelf,damageInfo);
                    break;

                default:
                    Debug.LogWarning($"无效的暴击类型: {damageInfo.crit}. 应为0或1");
                    break;
            }

        }

        private void OhoDeadEfx(Health health, DamageInfo info)
        {
            Vector3 hitPoint = health.transform.position;
            HandleDeadEffects(hitPoint, health);
        }


        public void PlaySound3D(int index, Vector3 position,String type)
        {
            if (Setting.DontOhoSoFast)
            {
                if (oHoInterval > 0f)
                {
                    return;
                }
                else
                {
                    oHoInterval = Setting.OHoInterval;
                }
            }
            if (index < 0 || index >= hitAudios.Length)
            {
                Debug.LogError($"无效的音效索引：{index}");
                return;
            }
            VECTOR pos = new VECTOR();
            pos.x = position.x;
            pos.y = position.y;
            pos.z = position.z;
            VECTOR vel = new VECTOR();
            Channel channel = default;
            if (type == "Dead") CoreSystem.playSound(deadAudios[index], masterSfxGroup, true, out channel);
            else if (type == "Hit") CoreSystem.playSound(hitAudios[index], masterSfxGroup, true, out channel);
            else return;
            channel.set3DAttributes(ref pos, ref vel);
            channel.setPaused(false);
        }
        private void HandleHitEffects(Vector3 hitPoint, Health health,bool onlyOHOSelf, DamageInfo damageInfo)
        {
            
            if (Setting.OhoHitEfx != "关闭特效")
            {
                // 选择特效预制体
                GameObject effectPrefab;
                if (onlyOHOSelf && health.IsMainCharacterHealth)
                {
                    bool forceUncriticalEffect = damageInfo.crit == 0 && damageInfo.damageValue < Setting.OnlyOHOSelfThreshold;
                    effectPrefab = (forceUncriticalEffect || Setting.OhoHitEfx == "关闭哦齁特效") ? ohoEfxUncritical : ohoEfx;
                }
                else if (health.IsMainCharacterHealth)
                {
                    bool forceUncriticalEffect = damageInfo.crit == 0 && damageInfo.damageValue < Setting.OnlyOHOSelfThreshold;
                    effectPrefab = (forceUncriticalEffect || Setting.OhoHitEfx == "关闭哦齁特效") ? ohoEfxUncritical : ohoEfx;
                }
                else
                {
                    bool forceUncriticalEffect = damageInfo.crit == 0 && damageInfo.damageValue < Setting.DamageThreshold;
                    effectPrefab = (forceUncriticalEffect || Setting.OhoHitEfx == "关闭哦齁特效") ? ohoEfxUncritical : ohoEfx;
                }
                
                Instantiate(effectPrefab, hitPoint, Quaternion.identity);
            }


            if (Setting.DisableHitSound) return;
            bool shouldPlaySound;
            if (onlyOHOSelf && health.IsMainCharacterHealth)
            {
               shouldPlaySound = damageInfo.damageValue > Setting.OnlyOHOSelfThreshold;
            }
            else if (health.IsMainCharacterHealth)
            {
                shouldPlaySound = damageInfo.damageValue > Setting.OnlyOHOSelfThreshold;
            }
            else
            {
                if(onlyOHOSelf)
                {
                    shouldPlaySound = false;
                }
                else
                {
                    shouldPlaySound = damageInfo.crit == 1 || damageInfo.damageValue > Setting.DamageThreshold;
                }
            }
            if (shouldPlaySound)
            {
                PlaySound3D(UnityEngine.Random.Range(0, hitAudios.Length),health.transform.position,"Hit");
            }
        }
        private void HandleDeadEffects(Vector3 hitPoint, Health health)
        {
            if (Setting.OhoDeadEfx != "关闭特效") 
            {
                GameObject effectPrefab = Setting.OhoDeadEfx == "正常死亡特效" ? ohoEfxDeadNormal : ohoEfxDeadLarge;
                Instantiate(effectPrefab, hitPoint, Quaternion.identity);
                if (Setting.DisableDeadSound) return;
                PlaySound3D(UnityEngine.Random.Range(0, deadAudios.Length), health.transform.position, "Dead");
            }

        }

        private void HitVolumeChange(float volume)
        {
            soundHitGroup.setVolume(volume / 100f);
        }
        private void DeadVolumeChange(float volume)
        {
            soundDeadGroup.setVolume(volume / 100f);
        }


        private void OhoHitInit()
        {
            Health.OnHurt += OhoHitEfx;
            Health.OnDead += OhoDeadEfx;
        }
        private void OhoHitDeinit()
        {
            Health.OnHurt -= OhoHitEfx;
            Health.OnDead -= OhoDeadEfx;
        }
        public static String GetModPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
        private void ModManager_OnModActivated(ModInfo arg1, Duckov.Modding.ModBehaviour arg2)
        {
            if (arg1.name != ModSettingAPI.MOD_NAME || !ModSettingAPI.Init(info)) return;
            //(触发时机:此mod在ModSetting之前启用)检查启用的mod是否是ModSetting,是进行初始化
            //先从ModSetting中读取配置
            Setting.Init();
            AddUI();
        }
        private void LoadAssetBundles()
        {
            if (File.Exists(Path.Combine(efxPath, "ohohitefx.unity3d")))
            {
                efxAssetBundle = AssetBundle.LoadFromFile(Path.Combine(efxPath, "ohohitefx.unity3d"));
                ohoEfx = efxAssetBundle.LoadAsset<GameObject>("OhoHit.prefab");
                ohoEfxUncritical = efxAssetBundle.LoadAsset<GameObject>("OhoHitUncritical.prefab");
                ohoEfxDeadNormal = efxAssetBundle.LoadAsset<GameObject>("OhoSpritNormal.prefab");
                ohoEfxDeadLarge = efxAssetBundle.LoadAsset<GameObject>("OhoSpritLarge.prefab");
            }
        }
    }
}
