﻿/*
* CombatClass for TheNoobBot
* Credit : Vesper, Neo2003, Dreadlocks
* Thanks you !
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using nManager.Helpful;
using nManager.Wow;
using nManager.Wow.Bot.Tasks;
using nManager.Wow.Class;
using nManager.Wow.Enums;
using nManager.Wow.Helpers;
using nManager.Wow.ObjectManager;
using Timer = nManager.Helpful.Timer;

// ReSharper disable EmptyGeneralCatchClause
// ReSharper disable ObjectCreationAsStatement

public class Main : ICombatClass
{
    internal static float InternalRange = 5.0f;
    internal static float InternalAggroRange = 5.0f;
    internal static bool InternalLoop = true;

    #region ICombatClass Members

    public float AggroRange
    {
        get { return InternalAggroRange; }
    }

    public float Range
    {
        get { return InternalRange; }
        set { InternalRange = value; }
    }

    public void Initialize()
    {
        Initialize(false);
    }

    public void Dispose()
    {
        Logging.WriteFight("Combat system stopped.");
        InternalLoop = false;
    }

    public void ShowConfiguration()
    {
        Directory.CreateDirectory(Application.StartupPath + "\\CombatClasses\\Settings\\");
        Initialize(true);
    }

    public void ResetConfiguration()
    {
        Directory.CreateDirectory(Application.StartupPath + "\\CombatClasses\\Settings\\");
        Initialize(true, true);
    }

    #endregion

    public void Initialize(bool configOnly, bool resetSettings = false)
    {
        try
        {
            if (!InternalLoop)
                InternalLoop = true;
            Logging.WriteFight("Loading combat system.");
            WoWSpecialization wowSpecialization = ObjectManager.Me.WowSpecialization(true);
            switch (ObjectManager.Me.WowClass)
            {
                    #region Paladin Specialisation checking

                case WoWClass.Paladin:

                    if (wowSpecialization == WoWSpecialization.PaladinRetribution || wowSpecialization == WoWSpecialization.None)
                    {
                        if (configOnly)
                        {
                            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Retribution.xml";
                            var currentSetting = new PaladinRetribution.PaladinRetributionSettings();
                            if (File.Exists(currentSettingsFile) && !resetSettings)
                            {
                                currentSetting = Settings.Load<PaladinRetribution.PaladinRetributionSettings>(currentSettingsFile);
                            }
                            currentSetting.ToForm();
                            currentSetting.Save(currentSettingsFile);
                        }
                        else
                        {
                            Logging.WriteFight("Loading Paladin Retribution Combat class...");
                            EquipmentAndStats.SetPlayerSpe(WoWSpecialization.PaladinRetribution);
                            new PaladinRetribution();
                        }
                        break;
                    }
                    if (wowSpecialization == WoWSpecialization.PaladinProtection)
                    {
                        if (configOnly)
                        {
                            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Protection.xml";
                            var currentSetting = new PaladinProtection.PaladinProtectionSettings();
                            if (File.Exists(currentSettingsFile) && !resetSettings)
                            {
                                currentSetting = Settings.Load<PaladinProtection.PaladinProtectionSettings>(currentSettingsFile);
                            }
                            currentSetting.ToForm();
                            currentSetting.Save(currentSettingsFile);
                        }
                        else
                        {
                            Logging.WriteFight("Loading Paladin Protection Combat class...");
                            EquipmentAndStats.SetPlayerSpe(WoWSpecialization.PaladinProtection);
                            new PaladinProtection();
                        }
                        break;
                    }
                    if (wowSpecialization == WoWSpecialization.PaladinHoly)
                    {
                        if (configOnly)
                        {
                            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Holy.xml";
                            var currentSetting = new PaladinHoly.PaladinHolySettings();
                            if (File.Exists(currentSettingsFile) && !resetSettings)
                            {
                                currentSetting = Settings.Load<PaladinHoly.PaladinHolySettings>(currentSettingsFile);
                            }
                            currentSetting.ToForm();
                            currentSetting.Save(currentSettingsFile);
                        }
                        else
                        {
                            Logging.WriteFight("Loading Paladin Holy Combat class...");
                            InternalRange = 30.0f;
                            EquipmentAndStats.SetPlayerSpe(WoWSpecialization.PaladinHoly);
                            new PaladinHoly();
                        }
                        break;
                    }
                    break;

                    #endregion

                default:
                    Dispose();
                    break;
            }
        }
        catch
        {
        }
        Logging.WriteFight("Combat system stopped.");
    }

    internal static void DumpCurrentSettings<T>(object mySettings)
    {
        mySettings = mySettings is T ? (T) mySettings : default(T);
        BindingFlags bindingFlags = BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.Static;
        for (int i = 0; i < mySettings.GetType().GetFields(bindingFlags).Length - 1; i++)
        {
            FieldInfo field = mySettings.GetType().GetFields(bindingFlags)[i];
            Logging.WriteDebug(field.Name + " = " + field.GetValue(mySettings));
        }

        // Last field is intentionnally ommited because it's a backing field.
    }
}

#region Paladin

public class PaladinHoly
{
    private static PaladinHolySettings MySettings = PaladinHolySettings.GetSettings();

    #region Professions & Racial

    public readonly Spell ArcaneTorrent = new Spell("Arcane Torrent");
    public readonly Spell Berserking = new Spell("Berserking");
    public readonly Spell GiftoftheNaaru = new Spell("Gift of the Naaru");

    public readonly Spell Stoneform = new Spell("Stoneform");
    public readonly Spell WarStomp = new Spell("War Stomp");
    private readonly WoWItem _firstTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET);
    private readonly WoWItem _secondTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET, 2);

    #endregion

    #region Paladin Seals & Buffs

    #endregion

    #region Offensive Spell

    public readonly Spell Denounce = new Spell("Denounce");
    public readonly Spell HammerOfJustice = new Spell("Hammer of Justice");
    public readonly Spell HammerOfWrath = new Spell("Hammer of Wrath");
    public readonly Spell HolyShock = new Spell("Holy Shock");

    #endregion

    #region Offensive Cooldown

    public readonly Spell AvengingWrath = new Spell("Avenging Wrath");
    public readonly Spell HolyAvenger = new Spell("HolyAvenger");

    #endregion

    #region Defensive Cooldown

    public readonly Spell DevotionAura = new Spell("Devotion Aura");
    public readonly Spell DivineProtection = new Spell("Divine Protection");
    public readonly Spell DivineShield = new Spell("Divine Shield");
    public readonly Spell HandOfProtection = new Spell("Hand of Protection");
    public readonly Spell HandOfPurity = new Spell("Hand of Purity");
    public readonly Spell SacredShield = new Spell("Sacred Shield");

    #endregion

    #region Healing Spell

    public readonly Spell BeaconOfLight = new Spell("Beacon of Light");
    public readonly Spell FlashOfLight = new Spell("Flash of Light");
    public readonly Spell GlyphOfHarshWords = new Spell("Glyph of Harsh Words");
    public readonly Spell HolyLight = new Spell("Holy Light");
    public readonly Spell HolyRadiance = new Spell("Holy Radiance");
    public readonly Spell LayOnHands = new Spell("Lay on Hands");
    public readonly Spell WordOfGlory = new Spell("Word of Glory");

    #endregion

    public PaladinHoly()
    {
        Main.InternalRange = 30f;
        Main.InternalAggroRange = 30f;
        MySettings = PaladinHolySettings.GetSettings();
        Main.DumpCurrentSettings<PaladinHolySettings>(MySettings);
        UInt128 lastTarget = 0;

        while (Main.InternalLoop)
        {
            try
            {
                if (!ObjectManager.Me.IsDeadMe)
                {
                    if (!ObjectManager.Me.IsMounted)
                    {
                        if (Fight.InFight && ObjectManager.Me.Target > 0)
                        {
                            if (ObjectManager.Me.Target != lastTarget && HolyShock.IsHostileDistanceGood)
                            {
                                Pull();
                                lastTarget = ObjectManager.Me.Target;
                            }
                            if (ObjectManager.Target.GetDistance <= 40f)
                                Combat();
                        }
                        if (!ObjectManager.Me.IsCast)
                            Patrolling();
                    }
                }
                else
                    Thread.Sleep(500);
            }
            catch
            {
            }
            Thread.Sleep(100);
        }
    }

    private void Pull()
    {
        if (HolyShock.KnownSpell && HolyShock.IsHostileDistanceGood && HolyShock.IsSpellUsable && MySettings.UseHolyShock)
        {
            HolyShock.Cast();
        }
    }

    private void Combat()
    {
        Buffs();
        DPSBurst();
        if (MySettings.DoAvoidMelee)
            AvoidMelee();
        DPSCycle();
        Heal();
    }

    private void Patrolling()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            Heal();
        }
    }

    private void Buffs()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            if (MySettings.UseAlchFlask && !ObjectManager.Me.HaveBuff(79638) && !ObjectManager.Me.HaveBuff(79640) && !ObjectManager.Me.HaveBuff(79639)
                && !ItemsManager.IsItemOnCooldown(75525) && ItemsManager.GetItemCount(75525) > 0)
                ItemsManager.UseItem(75525);
        }
    }

    private void Heal()
    {
        if (ObjectManager.Me.HealthPercent < 95 && !ObjectManager.Me.InCombat)
        {
            if (HolyLight.KnownSpell && HolyLight.IsSpellUsable && MySettings.UseHolyLight)
            {
                HolyLight.Cast(true, true, true);
                return;
            }
            if (FlashOfLight.KnownSpell && FlashOfLight.IsSpellUsable && MySettings.UseFlashOfLight)
            {
                FlashOfLight.Cast(true, true, true);
                return;
            }
        }
        if (!ObjectManager.Me.HaveBuff(25771))
        {
            if (DivineShield.KnownSpell && ObjectManager.Me.HealthPercent > 0 && ObjectManager.Me.HealthPercent <= 20 &&
                DivineShield.IsSpellUsable && MySettings.UseDivineShield)
            {
                DivineShield.Cast();
                return;
            }
            if (LayOnHands.KnownSpell && ObjectManager.Me.HealthPercent > 0 && ObjectManager.Me.HealthPercent <= 20 &&
                LayOnHands.IsSpellUsable && MySettings.UseLayOnHands)
            {
                LayOnHands.Cast();
                return;
            }
            if (HandOfProtection.KnownSpell && ObjectManager.Me.HealthPercent > 0 &&
                ObjectManager.Me.HealthPercent <= 20 &&
                HandOfProtection.IsSpellUsable && MySettings.UseHandOfProtection)
            {
                HandOfProtection.Cast();
                return;
            }
        }
        if (ObjectManager.Me.ManaPercentage < 30)
        {
            if (ArcaneTorrent.KnownSpell && ArcaneTorrent.IsSpellUsable && MySettings.UseArcaneTorrentForResource)
                ArcaneTorrent.Cast();
        }
        if (ObjectManager.Me.HealthPercent > 0 && ObjectManager.Me.HealthPercent < 50)
        {
            if (WordOfGlory.KnownSpell && WordOfGlory.IsSpellUsable &&
                (!GlyphOfHarshWords.KnownSpell /* || cast on me */) && MySettings.UseWordOfGlory)
                WordOfGlory.Cast();
            if (HolyLight.KnownSpell && HolyLight.IsSpellUsable && MySettings.UseHolyLight)
            {
                HolyLight.Cast();
                return;
            }
            if (FlashOfLight.KnownSpell && FlashOfLight.IsSpellUsable && MySettings.UseFlashOfLight)
            {
                FlashOfLight.Cast();
                return;
            }
        }
        if (ObjectManager.Me.HealthPercent >= 0 && ObjectManager.Me.HealthPercent < 30)
        {
            if (WordOfGlory.KnownSpell && WordOfGlory.IsSpellUsable &&
                (!GlyphOfHarshWords.KnownSpell /* || cast on me */) && MySettings.UseWordOfGlory)
                WordOfGlory.Cast();
            if (DivineProtection.KnownSpell && DivineProtection.IsSpellUsable && MySettings.UseDivineProtection)
                DivineProtection.Cast();
            if (FlashOfLight.KnownSpell && FlashOfLight.IsSpellUsable && MySettings.UseFlashOfLight)
            {
                FlashOfLight.Cast();
                return;
            }
            if (HolyLight.KnownSpell && HolyLight.IsSpellUsable && MySettings.UseHolyLight)
            {
                HolyLight.Cast();
            }
        }
    }

    private void DPSBurst()
    {
        if (MySettings.UseAvengingWrath && AvengingWrath.KnownSpell && AvengingWrath.IsSpellUsable)
        {
            AvengingWrath.Cast();
            if (MySettings.UseHolyAvenger && HolyAvenger.KnownSpell && HolyAvenger.IsSpellUsable)
            {
                HolyAvenger.Cast();
            }
            return;
        }
        if (!MySettings.UseAvengingWrath || !AvengingWrath.KnownSpell || !AvengingWrath.IsSpellUsable)
        {
            if (MySettings.UseHolyAvenger && HolyAvenger.KnownSpell && HolyAvenger.IsSpellUsable)
            {
                HolyAvenger.Cast();
                return;
            }
        }
        if (MySettings.UseTrinketOne && !ItemsManager.IsItemOnCooldown(_firstTrinket.Entry) && ItemsManager.IsItemUsable(_firstTrinket.Entry))
        {
            ItemsManager.UseItem(_firstTrinket.Name);
            Logging.WriteFight("Use First Trinket Slot");
            return;
        }
        if (MySettings.UseTrinketTwo && !ItemsManager.IsItemOnCooldown(_secondTrinket.Entry) && ItemsManager.IsItemUsable(_secondTrinket.Entry))
        {
            ItemsManager.UseItem(_secondTrinket.Name);
            Logging.WriteFight("Use Second Trinket Slot");
        }
    }

    private void DPSCycle()
    {
        Usefuls.SleepGlobalCooldown();
        try
        {
            Memory.WowMemory.GameFrameLock(); // !!! WARNING - DONT SLEEP WHILE LOCKED - DO FINALLY(GameFrameUnLock()) !!!

            if (HolyShock.KnownSpell && HolyShock.IsHostileDistanceGood && HolyShock.IsSpellUsable && MySettings.UseHolyShock)
            {
                HolyShock.Cast();
                return;
            }
            if (HammerOfWrath.KnownSpell && HammerOfWrath.IsHostileDistanceGood && ObjectManager.Target.IsAlive && HammerOfWrath.IsSpellUsable &&
                MySettings.UseHammerOfWrath)
            {
                HammerOfWrath.Cast();
                return;
            }
            if (HammerOfJustice.KnownSpell && HammerOfJustice.IsHostileDistanceGood && HammerOfJustice.IsSpellUsable &&
                MySettings.UseHammerOfJustice)
            {
                HammerOfJustice.Cast();
                return;
            }
            if (Denounce.KnownSpell && Denounce.IsHostileDistanceGood && Denounce.IsSpellUsable && MySettings.UseDenounce)
            {
                Denounce.Cast();
            }
        }
        finally
        {
            Memory.WowMemory.GameFrameUnLock();
        }
    }

    private void AvoidMelee()
    {
        if (ObjectManager.Target.GetDistance < MySettings.DoAvoidMeleeDistance && ObjectManager.Target.InCombat)
        {
            Logging.WriteFight("Too Close. Moving Back");
            var maxTimeTimer = new Timer(1000*2);
            MovementsAction.MoveBackward(true);
            while (ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat && !maxTimeTimer.IsReady)
                Others.SafeSleep(300);
            MovementsAction.MoveBackward(false);
            if (maxTimeTimer.IsReady && ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat)
            {
                MovementsAction.MoveForward(true);
                Others.SafeSleep(1000);
                MovementsAction.MoveForward(false);
                MovementManager.Face(ObjectManager.Target.Position);
            }
        }
    }

    #region Nested type: PaladinHolySettings

    [Serializable]
    public class PaladinHolySettings : Settings
    {
        public bool DoAvoidMelee = false;
        public int DoAvoidMeleeDistance = 0;
        public bool UseAlchFlask = true;
        public bool UseArcaneTorrentForDecast = true;
        public int UseArcaneTorrentForDecastAtPercentage = 100;
        public bool UseArcaneTorrentForResource = true;
        public int UseArcaneTorrentForResourceAtPercentage = 80;
        public bool UseAvengingWrath = true;
        public bool UseBeaconOfLight = true;
        public bool UseBerserking = true;
        public bool UseDenounce = true;
        public bool UseDevotionAura = true;
        public bool UseDivineProtection = true;
        public bool UseDivineShield = true;
        public bool UseFlashOfLight = true;
        public bool UseGiftoftheNaaru = true;
        public int UseGiftoftheNaaruAtPercentage = 80;
        public bool UseHammerOfJustice = true;
        public bool UseHammerOfWrath = true;
        public bool UseHandOfProtection = true;
        public bool UseHandOfPurity = true;
        public bool UseHolyAvenger = true;
        public bool UseHolyLight = true;
        public bool UseHolyRadiance = true;
        public bool UseHolyShock = true;
        public bool UseLayOnHands = true;

        public bool UseSacredShield = true;
        public bool UseStoneform = true;
        public int UseStoneformAtPercentage = 80;
        public bool UseTrinketOne = true;
        public bool UseTrinketTwo = true;
        public bool UseWarStomp = true;
        public int UseWarStompAtPercentage = 80;
        public bool UseWordOfGlory = true;

        public PaladinHolySettings()
        {
            ConfigWinForm("Paladin Protection Settings");
            /* Professions & Racials */
            AddControlInWinForm("Use Alchemist Flask", "UseAlchFlask", "Game Settings");
            AddControlInWinForm("Use Arcane Torrent for Interrupt", "UseArcaneTorrentForDecast", "Professions & Racials", "AtPercentage");
            AddControlInWinForm("Use Arcane Torrent for Resource", "UseArcaneTorrentForResource", "Professions & Racials", "AtPercentage");

            AddControlInWinForm("Use Stoneform", "UseStoneform", "Professions & Racials");
            AddControlInWinForm("Use Gift of the Naaru", "UseGiftoftheNaaru", "Professions & Racials");
            AddControlInWinForm("Use War Stomp", "UseWarStomp", "Professions & Racials");
            AddControlInWinForm("Use Berserking", "UseBerserking", "Professions & Racials");
            /* Paladin Seals & Buffs */
            /* Offensive Spell */
            AddControlInWinForm("Use Holy Shock", "UseHolyShock", "Offensive Spell");
            AddControlInWinForm("Use Denounce", "UseDenounce", "Offensive Spell");
            AddControlInWinForm("Use Hammer of Justice", "UseHammerOfJustice", "Offensive Spell");
            AddControlInWinForm("Use Hammer of Wrath", "UseHammerOfWrath", "Offensive Spell");
            /* Offensive Cooldown */
            AddControlInWinForm("Use Holy Avenger", "UseHolyAvenger", "Offensive Cooldown");
            AddControlInWinForm("Use Avenging Wrath", "UseAvengingWrath", "Offensive Cooldown");
            /* Defensive Cooldown */
            AddControlInWinForm("Use Sacred Shield", "UseSacredShield", "Defensive Cooldown");
            AddControlInWinForm("Use Hand of Purity", "UseHandOfPurity", "Defensive Cooldown");
            AddControlInWinForm("Use Devotion Aura", "UseDevotionAura", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Protection", "UseDivineProtection", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Shield", "UseDivineShield", "Defensive Cooldown");
            AddControlInWinForm("Use Hand of Protection", "UseHandOfProtection", "Defensive Cooldown");
            /* Healing Spell */
            AddControlInWinForm("Use Holy Radiance", "UseHolyRadiance", "Healing Spell");
            AddControlInWinForm("Use Flash of Light", "UseFlashOfLight", "Healing Spell");
            AddControlInWinForm("Use Holy Light", "UseHolyLight", "Healing Spell");
            AddControlInWinForm("Use Lay on Hands", "UseLayOnHands", "Healing Spell");
            AddControlInWinForm("Use Word of Glory", "UseWordOfGlory", "Healing Spell");
            AddControlInWinForm("Use Beacon of Light", "UseBeaconOfLight", "Healing Spell");
            AddControlInWinForm("Use Trinket One", "UseTrinketOne", "Game Settings");
            AddControlInWinForm("Use Trinket Two", "UseTrinketTwo", "Game Settings");
            AddControlInWinForm("Do avoid melee (Off Advised!!)", "DoAvoidMelee", "Game Settings");
            AddControlInWinForm("Avoid melee distance (1 to 4)", "DoAvoidMeleeDistance", "Game Settings");
        }

        public static PaladinHolySettings CurrentSetting { get; set; }

        public static PaladinHolySettings GetSettings()
        {
            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Holy.xml";
            if (File.Exists(currentSettingsFile))
            {
                return
                    CurrentSetting = Load<PaladinHolySettings>(currentSettingsFile);
            }
            return new PaladinHolySettings();
        }
    }

    #endregion
}

public class PaladinProtection
{
    private static PaladinProtectionSettings MySettings = PaladinProtectionSettings.GetSettings();

    #region Professions & Racial

    public readonly Spell ArcaneTorrent = new Spell("Arcane Torrent");
    public readonly Spell Berserking = new Spell("Berserking");
    public readonly Spell GiftoftheNaaru = new Spell("Gift of the Naaru");

    public readonly Spell Stoneform = new Spell("Stoneform");
    public readonly Spell WarStomp = new Spell("War Stomp");
    private readonly WoWItem _firstTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET);
    private readonly WoWItem _secondTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET, 2);

    #endregion

    #region Paladin Seals & Buffs

    #endregion

    #region Offensive Spell

    public readonly Spell AvengersShield = new Spell("Avenger's Shield");
    public readonly Spell Consecration = new Spell("Consecration");
    public readonly Spell CrusaderStrike = new Spell("Crusader Strike");
    public readonly Spell HammerOfJustice = new Spell("Hammer of Justice");
    public readonly Spell HammerOfTheRighteous = new Spell("Hammer of the Righteous"); // 115798 = Weakened Blows
    public readonly Spell HammerOfWrath = new Spell("Hammer of Wrath");
    public readonly Spell HolyWrath = new Spell("Holy Wrath");
    public readonly Spell Judgment = new Spell("Judgment");
    public readonly Spell ShieldOfTheRighteous = new Spell("Shield of the Righteous");

    #endregion

    #region Offensive Cooldown

    public readonly Spell HolyAvenger = new Spell("Holy Avenger");

    #endregion

    #region Defensive Cooldown

    public readonly Spell ArdentDefender = new Spell("Ardent Defender");
    public readonly Spell DivineProtection = new Spell("Divine Protection");
    public readonly Spell DivineShield = new Spell("Divine Shield");
    public readonly Spell GuardianOfAncientKings = new Spell("Guardian of Ancient Kings");
    public readonly Spell HandOfProtection = new Spell("Hand of Protection");
    public readonly Spell HandOfPurity = new Spell("Hand Of Purity");
    public readonly Spell SacredShield = new Spell("Sacred Shield");
    private Timer _onCd = new Timer(0);

    #endregion

    #region Healing Spell

    public readonly Spell FlashOfLight = new Spell("Flash of Light");
    public readonly Spell LayOnHands = new Spell("Lay on Hands");
    public readonly Spell WordOfGlory = new Spell("Word of Glory");

    #endregion

    public PaladinProtection()
    {
        Main.InternalRange = ObjectManager.Me.GetCombatReach;
        MySettings = PaladinProtectionSettings.GetSettings();
        Main.DumpCurrentSettings<PaladinProtectionSettings>(MySettings);
        UInt128 lastTarget = 0;

        while (Main.InternalLoop)
        {
            try
            {
                if (!ObjectManager.Me.IsDeadMe)
                {
                    if (!ObjectManager.Me.IsMounted)
                    {
                        if (Fight.InFight && ObjectManager.Me.Target > 0)
                        {
                            if (ObjectManager.Me.Target != lastTarget && Judgment.IsHostileDistanceGood)
                            {
                                Pull();
                                lastTarget = ObjectManager.Me.Target;
                            }
                            if (ObjectManager.Target.GetDistance <= 40f)
                                Combat();
                        }
                        if (!ObjectManager.Me.IsCast)
                            Patrolling();
                    }
                }
                else
                    Thread.Sleep(500);
            }
            catch
            {
            }
            Thread.Sleep(100);
        }
    }

    private void Pull()
    {
        if (AvengersShield.KnownSpell && MySettings.UseAvengersShield && AvengersShield.IsHostileDistanceGood &&
            AvengersShield.IsSpellUsable)
        {
            AvengersShield.Cast();
        }
        if (Judgment.KnownSpell && MySettings.UseJudgment && Judgment.IsHostileDistanceGood && Judgment.IsSpellUsable)
        {
            Judgment.Cast();
            Others.SafeSleep(1000);
        }
        DPSBurst();
    }

    private void Combat()
    {
        if (MySettings.DoAvoidMelee)
            AvoidMelee();
        if (_onCd.IsReady)
            DefenseCycle();
        DPSCycle();
        Heal();
        DPSCycle();
        Buffs();
        DPSBurst();
        DPSCycle();
    }

    private void Patrolling()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            Heal();
        }
    }

    private void Buffs()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            if (MySettings.UseAlchFlask && !ObjectManager.Me.HaveBuff(79638) && !ObjectManager.Me.HaveBuff(79640) && !ObjectManager.Me.HaveBuff(79639)
                && !ItemsManager.IsItemOnCooldown(75525) && ItemsManager.GetItemCount(75525) > 0)
                ItemsManager.UseItem(75525);
        }
    }

    private void Heal()
    {
        if (ObjectManager.Me.HealthPercent < 85 && !ObjectManager.Me.InCombat && !ObjectManager.Me.GetMove && !ObjectManager.Me.IsCast)
        {
            if (FlashOfLight.KnownSpell && FlashOfLight.IsSpellUsable && MySettings.UseFlashOfLight)
            {
                FlashOfLight.CastOnSelf(true, true, true);
                return;
            }
        }
        if (DivineShield.KnownSpell && MySettings.UseDivineShield && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && DivineShield.IsSpellUsable)
        {
            DivineShield.Cast();
            return;
        }
        if (LayOnHands.KnownSpell && MySettings.UseLayOnHands && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && LayOnHands.IsSpellUsable)
        {
            LayOnHands.CastOnSelf();
            return;
        }
        if (HandOfProtection.KnownSpell && MySettings.UseHandOfProtection && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && HandOfProtection.IsSpellUsable)
        {
            HandOfProtection.CastOnSelf();
            return;
        }
        if (ObjectManager.Me.ManaPercentage < 10)
        {
            if (ArcaneTorrent.KnownSpell && MySettings.UseArcaneTorrentForResource && ArcaneTorrent.IsSpellUsable)
            {
                ArcaneTorrent.Cast();
                return;
            }
        }
        if (ObjectManager.Me.HealthPercent > 0 && ObjectManager.Me.HealthPercent < 50)
        {
            if (WordOfGlory.KnownSpell && MySettings.UseWordOfGlory && WordOfGlory.IsSpellUsable && ObjectManager.Me.HolyPower >= 3)
                WordOfGlory.Cast();
            if (FlashOfLight.KnownSpell && MySettings.UseFlashOfLight && FlashOfLight.IsSpellUsable)
            {
                FlashOfLight.CastOnSelf();
                return;
            }
        }
        if (ObjectManager.Me.HealthPercent >= 0 && ObjectManager.Me.HealthPercent < 30)
        {
            if (WordOfGlory.KnownSpell && MySettings.UseWordOfGlory && WordOfGlory.IsSpellUsable && ObjectManager.Me.HolyPower >= 3)
                WordOfGlory.Cast();
            if (DivineProtection.KnownSpell && MySettings.UseDivineProtection && DivineProtection.IsSpellUsable)
                DivineProtection.Cast();
            if (FlashOfLight.KnownSpell && MySettings.UseFlashOfLight && FlashOfLight.IsSpellUsable)
            {
                FlashOfLight.CastOnSelf();
            }
        }
    }

    private void DPSBurst()
    {
        if (HolyAvenger.KnownSpell && MySettings.UseHolyAvenger && HolyAvenger.IsSpellUsable)
        {
            HolyAvenger.Cast();
        }
        if (MySettings.UseTrinketOne && !ItemsManager.IsItemOnCooldown(_firstTrinket.Entry) && ItemsManager.IsItemUsable(_firstTrinket.Entry))
        {
            ItemsManager.UseItem(_firstTrinket.Name);
            Logging.WriteFight("Use First Trinket Slot");
        }
        if (MySettings.UseTrinketTwo && !ItemsManager.IsItemOnCooldown(_secondTrinket.Entry) && ItemsManager.IsItemUsable(_secondTrinket.Entry))
        {
            ItemsManager.UseItem(_secondTrinket.Name);
            Logging.WriteFight("Use Second Trinket Slot");
        }
    }

    private void DefenseCycle()
    {
        if (HandOfPurity.KnownSpell && MySettings.UseHandOfPurity && HandOfPurity.IsSpellUsable && !HandOfPurity.HaveBuff)
        {
            HandOfPurity.Cast();
            _onCd = new Timer(1000*6);
            return;
        }
        if (HammerOfJustice.KnownSpell && MySettings.UseHammerOfJustice && HammerOfJustice.IsSpellUsable)
        {
            HammerOfJustice.Cast();
            _onCd = new Timer(1000*6);
            return;
        }
        if (DivineProtection.KnownSpell && MySettings.UseDivineProtection && DivineProtection.IsSpellUsable)
        {
            DivineProtection.Cast();
            _onCd = new Timer(1000*10);
            return;
        }
        if (GuardianOfAncientKings.KnownSpell && MySettings.UseGuardianOfAncientKings &&
            GuardianOfAncientKings.IsSpellUsable)
        {
            GuardianOfAncientKings.Cast();
            _onCd = new Timer(1000*12);
            return;
        }
        if (ArdentDefender.KnownSpell && MySettings.UseArdentDefender &&
            ArdentDefender.IsSpellUsable)
        {
            ArdentDefender.Cast();
            _onCd = new Timer(1000*10);
            return;
        }
        if (WordOfGlory.KnownSpell && MySettings.UseWordOfGlory && WordOfGlory.IsSpellUsable)
        {
            WordOfGlory.Cast();
            _onCd = new Timer(1000*5);
        }
    }

    private void DPSCycle()
    {
        Usefuls.SleepGlobalCooldown();
        try
        {
            Memory.WowMemory.GameFrameLock(); // !!! WARNING - DONT SLEEP WHILE LOCKED - DO FINALLY(GameFrameUnLock()) !!!

            if (ShieldOfTheRighteous.KnownSpell && MySettings.UseShieldOfTheRighteous && ShieldOfTheRighteous.IsSpellUsable &&
                ShieldOfTheRighteous.IsHostileDistanceGood && (ObjectManager.Me.HaveBuff(90174) || ObjectManager.Me.HolyPower >= 3))
            {
                ShieldOfTheRighteous.Cast();
                return;
            }
            if ((ObjectManager.GetNumberAttackPlayer() >= 2 || !ObjectManager.Target.HaveBuff(115798)) &&
                !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3)
            {
                if (HammerOfTheRighteous.KnownSpell && MySettings.UseHammerOfTheRighteous &&
                    HammerOfTheRighteous.IsHostileDistanceGood && HammerOfTheRighteous.IsSpellUsable)
                {
                    HammerOfTheRighteous.Cast();
                    return;
                }
            }
            else
            {
                if (CrusaderStrike.KnownSpell && MySettings.UseCrusaderStrike && CrusaderStrike.IsHostileDistanceGood &&
                    CrusaderStrike.IsSpellUsable && !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3)
                {
                    CrusaderStrike.Cast();
                    return;
                }
            }
            if (AvengersShield.KnownSpell && MySettings.UseAvengersShield && AvengersShield.IsHostileDistanceGood &&
                AvengersShield.IsSpellUsable && !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3)
            {
                AvengersShield.Cast();
                return;
            }
            if (HammerOfWrath.KnownSpell && MySettings.UseHammerOfWrath && HammerOfWrath.IsHostileDistanceGood &&
                HammerOfWrath.IsSpellUsable && !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3)
            {
                HammerOfWrath.Cast();
                return;
            }
            if (Judgment.KnownSpell && MySettings.UseJudgment && Judgment.IsHostileDistanceGood && Judgment.IsSpellUsable &&
                !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3)
            {
                Judgment.Cast();
                return;
            }
            if (Consecration.KnownSpell && MySettings.UseConsecration && Consecration.IsSpellUsable &&
                !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3 && ObjectManager.Target.GetDistance < 8)
            {
                Consecration.Cast();
                return;
            }
            if (HolyWrath.KnownSpell && MySettings.UseHolyWrath && HolyWrath.IsSpellUsable &&
                !ObjectManager.Me.HaveBuff(90174) && ObjectManager.Me.HolyPower < 3 && !Judgment.IsSpellUsable &&
                !CrusaderStrike.IsSpellUsable && !Consecration.IsSpellUsable)
            {
                HolyWrath.Cast();
            }
        }
        finally
        {
            Memory.WowMemory.GameFrameUnLock();
        }
    }

    private void AvoidMelee()
    {
        if (ObjectManager.Target.GetDistance < MySettings.DoAvoidMeleeDistance && ObjectManager.Target.InCombat)
        {
            Logging.WriteFight("Too Close. Moving Back");
            var maxTimeTimer = new Timer(1000*2);
            MovementsAction.MoveBackward(true);
            while (ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat && !maxTimeTimer.IsReady)
                Others.SafeSleep(300);
            MovementsAction.MoveBackward(false);
            if (maxTimeTimer.IsReady && ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat)
            {
                MovementsAction.MoveForward(true);
                Others.SafeSleep(1000);
                MovementsAction.MoveForward(false);
                MovementManager.Face(ObjectManager.Target.Position);
            }
        }
    }

    #region Nested type: PaladinProtectionSettings

    [Serializable]
    public class PaladinProtectionSettings : Settings
    {
        public bool DoAvoidMelee = false;
        public int DoAvoidMeleeDistance = 0;
        public bool UseAlchFlask = true;
        public bool UseArcaneTorrentForDecast = true;
        public int UseArcaneTorrentForDecastAtPercentage = 100;
        public bool UseArcaneTorrentForResource = true;
        public int UseArcaneTorrentForResourceAtPercentage = 80;
        public bool UseArdentDefender = true;
        public bool UseAvengersShield = true;
        public bool UseBerserking = true;
        public bool UseConsecration = true;
        public bool UseCrusaderStrike = true;
        public bool UseDivineProtection = true;
        public bool UseDivineShield = true;
        public bool UseFlashOfLight = true;
        public bool UseGiftoftheNaaru = true;
        public int UseGiftoftheNaaruAtPercentage = 80;
        public bool UseGuardianOfAncientKings = true;
        public bool UseHammerOfJustice = true;
        public bool UseHammerOfTheRighteous = true;
        public bool UseHammerOfWrath = true;
        public bool UseHandOfProtection = true;
        public bool UseHandOfPurity = true;
        public bool UseHolyAvenger = true;
        public bool UseHolyWrath = true;
        public bool UseJudgment = true;
        public bool UseLayOnHands = true;

        public bool UseSacredShield = true;
        public bool UseShieldOfTheRighteous = true;
        public bool UseStoneform = true;
        public int UseStoneformAtPercentage = 80;
        public bool UseTrinketOne = true;
        public bool UseTrinketTwo = true;
        public bool UseWarStomp = true;
        public int UseWarStompAtPercentage = 80;
        public bool UseWordOfGlory = true;

        public PaladinProtectionSettings()
        {
            ConfigWinForm("Paladin Protection Settings");
            /* Professions & Racials */
            AddControlInWinForm("Use Alchemist Flask", "UseAlchFlask", "Game Settings");
            AddControlInWinForm("Use Arcane Torrent for Interrupt", "UseArcaneTorrentForDecast", "Professions & Racials", "AtPercentage");
            AddControlInWinForm("Use Arcane Torrent for Resource", "UseArcaneTorrentForResource", "Professions & Racials", "AtPercentage");

            AddControlInWinForm("Use Stoneform", "UseStoneform", "Professions & Racials");
            AddControlInWinForm("Use Gift of the Naaru", "UseGiftoftheNaaru", "Professions & Racials");
            AddControlInWinForm("Use War Stomp", "UseWarStomp", "Professions & Racials");
            AddControlInWinForm("Use Berserking", "UseBerserking", "Professions & Racials");
            /* Paladin Seals & Buffs */
            /* Offensive Spell */
            AddControlInWinForm("Use Shield of the Righteous", "UseShieldOfTheRighteous", "Offensive Spell");
            AddControlInWinForm("Use Consecration", "UseConsecration", "Offensive Spell");
            AddControlInWinForm("Use Avenger's Shield", "UseAvengersShield", "Offensive Spell");
            AddControlInWinForm("Use Hammer of Wrath", "UseHammerOfWrath", "Offensive Spell");
            AddControlInWinForm("Use Crusader Strike", "UseCrusaderStrike", "Offensive Spell");
            AddControlInWinForm("Use Hammer of the Righteous", "UseHammerOfTheRighteous", "Offensive Spell");
            AddControlInWinForm("Use Judgment", "UseJudgment", "Offensive Spell");
            AddControlInWinForm("Use Hammer of Justice", "UseHammerOfJustice", "Offensive Spell");
            AddControlInWinForm("Use Holy Wrath", "UseHolyWrath", "Offensive Spell");
            /* Offensive Cooldown */
            AddControlInWinForm("Use Holy Avenger", "UseHolyAvenger", "Offensive Cooldown");
            /* Defensive Cooldown */
            AddControlInWinForm("Use Guardian of Ancient Kings", "UseGuardianOfAncientKings", "Defensive Cooldown");
            AddControlInWinForm("Use Ardent Defender", "UseArdentDefender", "Defensive Cooldown");
            AddControlInWinForm("Use Sacred Shield", "UseSacredShield", "Defensive Cooldown");
            AddControlInWinForm("Use Hand of Purity", "UseHandOfPurity", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Protection", "UseDivineProtection", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Shield", "UseDivineShield", "Defensive Cooldown");
            AddControlInWinForm("Use Hand of Protection", "UseHandOfProtection", "Defensive Cooldown");
            /* Healing Spell */
            AddControlInWinForm("Use Flash of Light", "UseFlashOfLight", "Healing Spell");
            AddControlInWinForm("Use Lay on Hands", "UseLayOnHands", "Healing Spell");
            AddControlInWinForm("Use Word of Glory", "UseWordOfGlory", "Healing Spell");
            AddControlInWinForm("Use Trinket One", "UseTrinketOne", "Game Settings");
            AddControlInWinForm("Use Trinket Two", "UseTrinketTwo", "Game Settings");
            AddControlInWinForm("Do avoid melee (Off Advised!!)", "DoAvoidMelee", "Game Settings");
            AddControlInWinForm("Avoid melee distance (1 to 4)", "DoAvoidMeleeDistance", "Game Settings");
        }

        public static PaladinProtectionSettings CurrentSetting { get; set; }

        public static PaladinProtectionSettings GetSettings()
        {
            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Protection.xml";
            if (File.Exists(currentSettingsFile))
            {
                return
                    CurrentSetting = Load<PaladinProtectionSettings>(currentSettingsFile);
            }
            return new PaladinProtectionSettings();
        }
    }

    #endregion
}

public class PaladinRetribution
{
    private static PaladinRetributionSettings MySettings = PaladinRetributionSettings.GetSettings();

    #region Professions & Racials

    public readonly Spell ArcaneTorrent = new Spell("Arcane Torrent");
    public readonly Spell Berserking = new Spell("Berserking");
    public readonly Spell GiftoftheNaaru = new Spell("Gift of the Naaru");

    public readonly Spell Stoneform = new Spell("Stoneform");
    public readonly Spell WarStomp = new Spell("War Stomp");

    public readonly Spell SanctifiedWrath = new Spell(53376);
    public Timer AvengingWrathTimer = new Timer(0);

    #endregion

    #region Paladin Seals & Buffs

    public readonly Spell GreaterBlessingOfKings = new Spell("Greater Blessing of Kings");
    public readonly Spell GreaterBlessingOfMight = new Spell("Greater Blessing of Might");
    public readonly Spell GreaterBlessingOfWisdom = new Spell("Greater Blessing of Wisdom");

    #endregion

    #region Offensive Spell

    public readonly Spell CrusaderStrike = new Spell("Crusader Strike");
    public readonly Spell BladeOfJustice = new Spell("Blade of Justice");
    public readonly Spell BladeOfWrath = new Spell("Blade of Wrath");
    public readonly Spell DivineHammer = new Spell("Divine Hammer");
    public readonly uint DivinePurposeBuff = 223819;
    public readonly Spell DivineStorm = new Spell("Divine Storm");
    public readonly Spell ExecutionSentence = new Spell("Execution Sentence");
    public readonly Spell JusticarsVengeance = new Spell("Justicar's Vengeance");
    public readonly Spell HammerOfJustice = new Spell("Hammer of Justice");
    public readonly Spell Judgment = new Spell("Judgment");
    public readonly Spell TemplarsVerdict = new Spell("Templar's Verdict");

    #endregion

    #region Offensive Cooldown

    public readonly Spell AvengingWrath = new Spell("Avenging Wrath");

    #endregion

    #region Defensive Cooldown

    public readonly Spell DivineProtection = new Spell("Divine Protection");
    public readonly Spell DivineShield = new Spell("Divine Shield");
    public readonly Spell HandOfProtection = new Spell("Hand of Protection");
    public readonly Spell Reckoning = new Spell("Reckoning");
    public readonly Spell SacredShield = new Spell("Sacred Shield");

    #endregion

    #region Healing Spell

    public readonly Spell FlashOfLight = new Spell("Flash of Light");
    public readonly Spell LayOnHands = new Spell("Lay on Hands");
    public readonly Spell WordOfGlory = new Spell("Word of Glory");

    #endregion

    #region Flask & Potion Management

/*
        private readonly int _combatPotion = ItemsManager.GetIdByName(MySettings.CombatPotion);
*/
    private readonly WoWItem _firstTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET);
    private readonly int _flaskOrBattleElixir = ItemsManager.GetItemIdByName(MySettings.FlaskOrBattleElixir);
    private readonly int _guardianElixir = ItemsManager.GetItemIdByName(MySettings.GuardianElixir);

/*
        private readonly WoWItem _hands = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_HAND);
*/
    private readonly WoWItem _secondTrinket = EquippedItems.GetEquippedItem(WoWInventorySlot.INVTYPE_TRINKET, 2);
/*
        private readonly int _teasureFindingPotion = ItemsManager.GetIdByName(MySettings.TeasureFindingPotion);
*/
/*
        private readonly int _wellFedBuff = ItemsManager.GetIdByName(MySettings.WellFedBuff);
*/

    #endregion

    public PaladinRetribution()
    {
        Main.InternalRange = ObjectManager.Me.GetCombatReach;
        MySettings = PaladinRetributionSettings.GetSettings();
        Main.DumpCurrentSettings<PaladinRetributionSettings>(MySettings);
        UInt128 lastTarget = 0;

        while (Main.InternalLoop)
        {
            try
            {
                if (!ObjectManager.Me.IsDeadMe)
                {
                    if (!ObjectManager.Me.IsMounted)
                    {
                        if (Fight.InFight && ObjectManager.Me.Target > 0)
                        {
                            if (ObjectManager.Me.Target != lastTarget && Reckoning.IsHostileDistanceGood)
                            {
                                Pull();
                                lastTarget = ObjectManager.Me.Target;
                            }
                            if (ObjectManager.Target.GetDistance <= 40f)
                                Combat();
                        }
                        if (!ObjectManager.Me.IsCast)
                            Patrolling();
                    }
                }
                else
                    Thread.Sleep(500);
            }
            catch
            {
            }
            Thread.Sleep(100);
        }
    }

    private void Pull()
    {
        if (MySettings.UseCrusaderStrike && CrusaderStrike.IsSpellUsable && CrusaderStrike.IsHostileDistanceGood)
        {
            CrusaderStrike.Cast();
            return;
        }
        if (MySettings.UseReckoning && Reckoning.IsSpellUsable && Reckoning.IsHostileDistanceGood)
        {
            Reckoning.Cast();
        }
    }

    private void Combat()
    {
        Buffs();
        DPSBurst();
        if (MySettings.DoAvoidMelee)
            AvoidMelee();
        DPSCycle();
        Heal();
    }

    private void Patrolling()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            if (MySettings.UseFlaskOrBattleElixir && MySettings.FlaskOrBattleElixir != string.Empty)
                if (!SpellManager.HaveBuffLua(ItemsManager.GetItemSpell(MySettings.FlaskOrBattleElixir)) &&
                    !ItemsManager.IsItemOnCooldown(_flaskOrBattleElixir) &&
                    ItemsManager.IsItemUsable(_flaskOrBattleElixir))
                    ItemsManager.UseItem(MySettings.FlaskOrBattleElixir);
            if (MySettings.UseGuardianElixir && MySettings.GuardianElixir != string.Empty)
                if (!SpellManager.HaveBuffLua(ItemsManager.GetItemSpell(MySettings.GuardianElixir)) &&
                    !ItemsManager.IsItemOnCooldown(_guardianElixir) && ItemsManager.IsItemUsable(_guardianElixir))
                    ItemsManager.UseItem(MySettings.GuardianElixir);
            Blessing();
            Heal();
        }
    }

    private void Buffs()
    {
        if (!ObjectManager.Me.IsMounted)
        {
            if (MySettings.UseFlaskOrBattleElixir && MySettings.FlaskOrBattleElixir != string.Empty)
                if (!SpellManager.HaveBuffLua(ItemsManager.GetItemSpell(MySettings.FlaskOrBattleElixir)) &&
                    !ItemsManager.IsItemOnCooldown(_flaskOrBattleElixir) &&
                    ItemsManager.IsItemUsable(_flaskOrBattleElixir))
                    ItemsManager.UseItem(MySettings.FlaskOrBattleElixir);
            if (MySettings.UseGuardianElixir && MySettings.GuardianElixir != string.Empty)
                if (!SpellManager.HaveBuffLua(ItemsManager.GetItemSpell(MySettings.GuardianElixir)) &&
                    !ItemsManager.IsItemOnCooldown(_guardianElixir) && ItemsManager.IsItemUsable(_guardianElixir))
                    ItemsManager.UseItem(MySettings.GuardianElixir);
            Blessing();

            if (MySettings.UseAlchFlask && !ObjectManager.Me.HaveBuff(79638) && !ObjectManager.Me.HaveBuff(79640) && !ObjectManager.Me.HaveBuff(79639)
                && !ItemsManager.IsItemOnCooldown(75525) && ItemsManager.GetItemCount(75525) > 0)
                ItemsManager.UseItem(75525);
        }
    }

    private void Blessing()
    {
        if (ObjectManager.Me.IsMounted)
            return;
        Usefuls.SleepGlobalCooldown();

        if (MySettings.UseGreaterBlessingOfKings && GreaterBlessingOfKings.KnownSpell && !GreaterBlessingOfKings.HaveBuff && GreaterBlessingOfKings.IsSpellUsable)
        {
            GreaterBlessingOfKings.Cast();
            return;
        }
        if (MySettings.UseGreaterBlessingOfMight && GreaterBlessingOfMight.KnownSpell && !GreaterBlessingOfMight.HaveBuff && GreaterBlessingOfMight.IsSpellUsable)
        {
            Logging.Write("If for raiding reasons you need to bless certains party member, disable Greater Blessings in settings and do it manually.");
            GreaterBlessingOfMight.Cast();
            return;
        }
        if (MySettings.UseGreaterBlessingOfWisdom && GreaterBlessingOfWisdom.KnownSpell && !GreaterBlessingOfWisdom.HaveBuff && GreaterBlessingOfWisdom.IsSpellUsable)
        {
            GreaterBlessingOfWisdom.Cast();
            return;
        }
    }

    private void Heal()
    {
        if (ObjectManager.Me.HealthPercent < 85 && !ObjectManager.Me.InCombat && !ObjectManager.Me.GetMove && !ObjectManager.Me.IsCast)
        {
            if (FlashOfLight.KnownSpell && FlashOfLight.IsSpellUsable && MySettings.UseFlashOfLight)
            {
                FlashOfLight.CastOnSelf(true, true, true);
                return;
            }
        }
        if (DivineShield.KnownSpell && MySettings.UseDivineShield && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && DivineShield.IsSpellUsable)
        {
            DivineShield.Cast();
            return;
        }
        if (LayOnHands.KnownSpell && MySettings.UseLayOnHands && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && LayOnHands.IsSpellUsable)
        {
            LayOnHands.CastOnSelf();
            return;
        }
        if (HandOfProtection.KnownSpell && MySettings.UseHandOfProtection && ObjectManager.Me.HealthPercent > 0 &&
            ObjectManager.Me.HealthPercent <= 20 && !ObjectManager.Me.HaveBuff(25771) && HandOfProtection.IsSpellUsable)
        {
            HandOfProtection.CastOnSelf();
            return;
        }
        if (ObjectManager.Me.ManaPercentage < 10)
        {
            if (ArcaneTorrent.KnownSpell && MySettings.UseArcaneTorrentForResource && ArcaneTorrent.IsSpellUsable)
            {
                ArcaneTorrent.Cast();
                return;
            }
        }
        if (ObjectManager.Me.HealthPercent > 0 && ObjectManager.Me.HealthPercent < 50)
        {
            if (WordOfGlory.KnownSpell && MySettings.UseWordOfGlory && WordOfGlory.IsSpellUsable && ObjectManager.Me.HolyPower >= 3)
                WordOfGlory.Cast();
            if (FlashOfLight.KnownSpell && MySettings.UseFlashOfLight && FlashOfLight.IsSpellUsable)
            {
                FlashOfLight.CastOnSelf();
                return;
            }
        }
        if (ObjectManager.Me.HealthPercent >= 0 && ObjectManager.Me.HealthPercent < 30)
        {
            if (WordOfGlory.KnownSpell && MySettings.UseWordOfGlory && WordOfGlory.IsSpellUsable && ObjectManager.Me.HolyPower >= 3)
                WordOfGlory.Cast();
            if (DivineProtection.KnownSpell && MySettings.UseDivineProtection && DivineProtection.IsSpellUsable)
                DivineProtection.Cast();
            if (FlashOfLight.KnownSpell && MySettings.UseFlashOfLight && FlashOfLight.IsSpellUsable)
            {
                FlashOfLight.CastOnSelf();
            }
        }
    }

    private void DPSBurst()
    {
        if (MySettings.UseAvengingWrath && Judgment.TargetHaveBuff && !AvengingWrath.HaveBuff && AvengingWrath.IsSpellUsable)
        {
            AvengingWrath.Cast();
        }
        if (MySettings.UseTrinketOne && !ItemsManager.IsItemOnCooldown(_firstTrinket.Entry) && ItemsManager.IsItemUsable(_firstTrinket.Entry))
        {
            ItemsManager.UseItem(_firstTrinket.Name);
            Logging.WriteFight("Use First Trinket Slot");
        }
        if (MySettings.UseTrinketTwo && !ItemsManager.IsItemOnCooldown(_secondTrinket.Entry) && ItemsManager.IsItemUsable(_secondTrinket.Entry))
        {
            ItemsManager.UseItem(_secondTrinket.Name);
            Logging.WriteFight("Use Second Trinket Slot");
        }
    }

    private void DPSCycle()
    {
        Usefuls.SleepGlobalCooldown();
        try
        {
            Memory.WowMemory.GameFrameLock(); // !!! WARNING - DONT SLEEP WHILE LOCKED - DO FINALLY(GameFrameUnLock()) !!!


            if (MySettings.UseJusticarsVengeance && ObjectManager.Me.HaveBuff(DivinePurposeBuff) &&
                (!MySettings.UseDivineStorm || !DivineStorm.IsSpellUsable || ObjectManager.GetUnitInSpellRange(DivineStorm.MaxRangeHostile) < 3) &&
                JusticarsVengeance.IsSpellUsable && JusticarsVengeance.IsHostileDistanceGood)
            {
                JusticarsVengeance.Cast();
                return;
            }
            if (MySettings.UseExecutionSentence && (!MySettings.UseJudgment || !Judgment.TargetHaveBuff) && (ObjectManager.Me.HaveBuff(DivinePurposeBuff) ||
                                                                                                             ObjectManager.Me.HolyPower >= 3) && ExecutionSentence.IsSpellUsable &&
                ExecutionSentence.IsHostileDistanceGood)
            {
                // don't cast if target have judgment buff because it's mean it will be expired when Sentence hit. If Judgement just faded, is no issues since we can recast it before the end of Sentence.
                ExecutionSentence.Cast();
                return;
            }
            if (MySettings.UseJudgment && (ObjectManager.Target.AuraIsActiveAndExpireInLessThanMs(ExecutionSentence.Id, 2000) || ObjectManager.Me.HolyPower == 5) && Judgment.IsSpellUsable &&
                Judgment.IsHostileDistanceGood)
            {
                // We cast judgment before ExecutionSentence deals its damages for 50% more damages.
                // We do 3 Holy Power worth of generation.
                Judgment.Cast();
                return;
            }
            if (((!MySettings.UseDivineStorm && MySettings.UseTemplarsVerdict && TemplarsVerdict.IsSpellUsable) ||
                 (TemplarsVerdict.IsSpellUsable && ObjectManager.GetUnitInSpellRange(DivineStorm.MaxRangeHostile) <= 2)) &&
                ((ObjectManager.Me.HaveBuff(DivinePurposeBuff) || (ObjectManager.Me.HolyPower == 5 || (Judgment.TargetHaveBuff && ObjectManager.Me.HolyPower >= 3))) &&
                 TemplarsVerdict.IsHostileDistanceGood))
            {
                TemplarsVerdict.Cast();
                return;
            }
            if (((MySettings.UseDivineStorm && !MySettings.UseTemplarsVerdict && DivineStorm.IsSpellUsable) || (DivineStorm.IsSpellUsable && ObjectManager.GetUnitInSpellRange(DivineStorm.MaxRangeHostile) >= 3)) &&
                ((ObjectManager.Me.HaveBuff(DivinePurposeBuff) || (ObjectManager.Me.HolyPower == 5 || Judgment.TargetHaveBuff && ObjectManager.Me.HolyPower >= 3))))
            {
                DivineStorm.Cast();
                return;
            }
            if (MySettings.UseCrusaderStrike && ObjectManager.Me.HolyPower < 5 && CrusaderStrike.IsSpellUsable && CrusaderStrike.IsHostileDistanceGood && CrusaderStrike.GetSpellCharges == 2)
            {
                // burn first CS Charge before any blade.
                CrusaderStrike.Cast();
                return;
            }
            if (MySettings.UseBladeOfJustice && ObjectManager.Me.HolyPower < 4 && BladeOfJustice.IsSpellUsable && BladeOfJustice.IsHostileDistanceGood)
            {
                BladeOfJustice.Cast();
                return;
            }
            if (MySettings.UseBladeOfWrath && ObjectManager.Me.HolyPower < 4 && BladeOfWrath.IsSpellUsable && BladeOfWrath.IsHostileDistanceGood)
            {
                BladeOfWrath.Cast();
                return;
            }
            if (MySettings.UseDivineHammer && ObjectManager.Me.HolyPower < 4 && DivineHammer.IsSpellUsable && DivineHammer.IsHostileDistanceGood)
            {
                DivineHammer.Cast();
                return;
            }
            if (MySettings.UseCrusaderStrike && ObjectManager.Me.HolyPower < 5 && CrusaderStrike.IsSpellUsable && CrusaderStrike.IsHostileDistanceGood)
            {
                CrusaderStrike.Cast();
                return;
            }
            if ((!MySettings.UseDivineStorm && MySettings.UseTemplarsVerdict && TemplarsVerdict.IsSpellUsable) ||
                (TemplarsVerdict.IsSpellUsable && ObjectManager.GetUnitInSpellRange(DivineStorm.MaxRangeHostile) <= 2) && TemplarsVerdict.IsHostileDistanceGood)
            {
                TemplarsVerdict.Cast();
                return;
            }
            if ((MySettings.UseDivineStorm && !MySettings.UseTemplarsVerdict && DivineStorm.IsSpellUsable) ||
                (DivineStorm.IsSpellUsable && ObjectManager.GetUnitInSpellRange(DivineStorm.MaxRangeHostile) > 2) && DivineStorm.IsHostileDistanceGood)
            {
                DivineStorm.Cast();
                return;
            }
            if (MySettings.UseJudgment && Judgment.IsSpellUsable && Judgment.IsHostileDistanceGood)
            {
                Judgment.Cast(); // fill with Judgment for low level paladins.
                return;
            }
            if (MySettings.UseHammerOfJustice && HammerOfJustice.IsSpellUsable && ObjectManager.Target.IsStunnable && HammerOfJustice.IsHostileDistanceGood)
            {
                HammerOfJustice.Cast();
                return;
            }
        }
        finally
        {
            Memory.WowMemory.GameFrameUnLock();
        }
    }

    private void AvoidMelee()
    {
        if (ObjectManager.Target.GetDistance < MySettings.DoAvoidMeleeDistance && ObjectManager.Target.InCombat)
        {
            Logging.WriteFight("Too Close. Moving Back");
            var maxTimeTimer = new Timer(1000*2);
            MovementsAction.MoveBackward(true);
            while (ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat && !maxTimeTimer.IsReady)
                Others.SafeSleep(300);
            MovementsAction.MoveBackward(false);
            if (maxTimeTimer.IsReady && ObjectManager.Target.GetDistance < 2 && ObjectManager.Target.InCombat)
            {
                MovementsAction.MoveForward(true);
                Others.SafeSleep(1000);
                MovementsAction.MoveForward(false);
                MovementManager.Face(ObjectManager.Target.Position);
            }
        }
    }

    #region Nested type: PaladinRetributionSettings

    [Serializable]
    public class PaladinRetributionSettings : Settings
    {
        public string CombatPotion = "Potion of Mogu Power";
        public bool DoAvoidMelee = false;
        public int DoAvoidMeleeDistance = 0;
        public string FlaskOrBattleElixir = "Flask of Winter's Bite";
        public string GuardianElixir = "";
        public bool RefreshWeakenedBlows = true;
        public string TeasureFindingPotion = "Potion of Luck";
        public bool UseAlchFlask = true;
        public bool UseArcaneTorrentForDecast = true;
        public int UseArcaneTorrentForDecastAtPercentage = 100;
        public bool UseArcaneTorrentForResource = true;
        public int UseArcaneTorrentForResourceAtPercentage = 80;
        public bool UseAvengingWrath = true;
        public bool UseBerserking = true;
        public bool UseGreaterBlessingOfKings = true;
        public bool UseGreaterBlessingOfMight = true;
        public bool UseGreaterBlessingOfWisdom = true;
        public bool UseCombatPotion = false;
        public bool UseCrusaderStrike = true;
        public bool UseBladeOfWrath = true;
        public bool UseBladeOfJustice = true;
        public bool UseDivineHammer = true;
        public bool UseDivineProtection = true;
        public bool UseDivineShield = true;
        public bool UseDivineStorm = true;
        public bool UseExecutionSentence = true;
        public bool UseJusticarsVengeance = true;
        public bool UseFlashOfLight = true;
        public bool UseFlaskOrBattleElixir = false;
        public bool UseGiftoftheNaaru = true;
        public int UseGiftoftheNaaruAtPercentage = 80;
        public bool UseGuardianElixir = false;
        public bool UseHammerOfJustice = true;
        public bool UseHandOfProtection = false;
        public bool UseHolyAvenger = true;
        public bool UseJudgment = true;
        public bool UseLayOnHands = true;

        public bool UseReckoning = true;
        public bool UseSacredShield = true;
        public bool UseStoneform = true;
        public int UseStoneformAtPercentage = 80;
        public bool UseTeasureFindingPotion = false;
        public bool UseTemplarsVerdict = true;
        public bool UseTrinketOne = true;
        public bool UseTrinketTwo = true;
        public bool UseWarStomp = true;
        public int UseWarStompAtPercentage = 80;
        public bool UseWellFedBuff = false;
        public bool UseWordOfGlory = true;

        public string WellFedBuff = "Sleeper Sushi";

        public PaladinRetributionSettings()
        {
            ConfigWinForm("Paladin Retribution Settings");
            /* Professions & Racials */
            AddControlInWinForm("Use Arcane Torrent for Interrupt", "UseArcaneTorrentForDecast", "Professions & Racials", "AtPercentage");
            AddControlInWinForm("Use Arcane Torrent for Resource", "UseArcaneTorrentForResource", "Professions & Racials", "AtPercentage");

            AddControlInWinForm("Use Stoneform", "UseStoneform", "Professions & Racials");
            AddControlInWinForm("Use Gift of the Naaru", "UseGiftoftheNaaru", "Professions & Racials");
            AddControlInWinForm("Use War Stomp", "UseWarStomp", "Professions & Racials");
            AddControlInWinForm("Use Berserking", "UseBerserking", "Professions & Racials");
            /* Paladin Seals & Buffs */
            AddControlInWinForm("Use Greater Blessing of Might", "UseGreaterBlessingOfMight", "Paladin Blessings");
            AddControlInWinForm("Use Greater Blessing of Kings", "UseGreaterBlessingOfKings", "Paladin Blessings");
            AddControlInWinForm("Use Greater Blessing of Wisdom", "UseGreaterBlessingOfWisdom", "Paladin Blessings");
            /* Offensive Spell */
            AddControlInWinForm("Use Templar's Verdict", "UseTemplarsVerdict", "Offensive Spell");
            AddControlInWinForm("Use Justicar's Vengeance", "UseJusticarsVengeance", "Offensive Spell");
            AddControlInWinForm("Use Divine Storm", "UseDivineStorm", "Offensive Spell");
            AddControlInWinForm("Use Crusader Strike", "UseCrusaderStrike", "Offensive Spell");
            AddControlInWinForm("Use Blade of Wrath", "UseBladeOfWrath", "Offensive Spell");
            AddControlInWinForm("Use Blade of Justice", "UseBladeOfJustice", "Offensive Spell");
            AddControlInWinForm("Use Divine Hammer", "UseDivineHammer", "Offensive Spell");
            AddControlInWinForm("Use Judgment", "UseJudgment", "Offensive Spell");
            AddControlInWinForm("Use Hammer of Justice", "UseHammerOfJustice", "Offensive Spell");
            AddControlInWinForm("Use Execution Sentence", "UseExecutionSentence", "Offensive Spell");
            /* Offensive Cooldown */
            AddControlInWinForm("Use Avenging Wrath", "UseAvengingWrath", "Offensive Cooldown");
            /* Defensive Cooldown */
            AddControlInWinForm("Use Reckoning", "UseReckoning", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Protection", "UseDivineProtection", "Defensive Cooldown");
            AddControlInWinForm("Use Sacred Shield", "UseSacredShield", "Defensive Cooldown");
            AddControlInWinForm("Use Divine Shield", "UseDivineShield", "Defensive Cooldown");
            AddControlInWinForm("Use Hand of Protection", "UseHandOfProtection", "Defensive Cooldown");
            /* Healing Spell */
            AddControlInWinForm("Use Flash of Light", "UseFlashOfLight", "Healing Spell");
            AddControlInWinForm("Use Lay on Hands", "UseLayOnHands", "Healing Spell");
            AddControlInWinForm("Use Word of Glory", "UseWordOfGlory", "Healing Spell");
            /* Flask & Potion Management */
            AddControlInWinForm("Use Alchemist Flask", "UseAlchFlask", "Game Settings");
            AddControlInWinForm("Use Trinket One", "UseTrinketOne", "Game Settings");
            AddControlInWinForm("Use Trinket Two", "UseTrinketTwo", "Game Settings");
            AddControlInWinForm("Use Flask or Battle Elixir", "UseFlaskOrBattleElixir", "Flask & Potion Management");
            AddControlInWinForm("Flask or Battle Elixir Name", "FlaskOrBattleElixir", "Flask & Potion Management");
            AddControlInWinForm("Use Guardian Elixir", "UseGuardianElixir", "Flask & Potion Management");
            AddControlInWinForm("Guardian Elixir Name", "GuardianElixir", "Flask & Potion Management");
            AddControlInWinForm("Use Combat Potion", "UseCombatPotion", "Flask & Potion Management");
            AddControlInWinForm("Combat Potion Name", "CombatPotion", "Flask & Potion Management");
            AddControlInWinForm("Use Teasure Finding Potion", "UseTeasureFindingPotion", "Flask & Potion Management");
            AddControlInWinForm("Teasure Finding Potion Name", "TeasureFindingPotion", "Flask & Potion Management");
            AddControlInWinForm("Use Well Fed Buff", "UseWellFedBuff", "Flask & Potion Management");
            AddControlInWinForm("Well Fed Buff Name", "WellFedBuff", "Flask & Potion Management");
            AddControlInWinForm("Do avoid melee (Off Advised!!)", "DoAvoidMelee", "Game Settings");
            AddControlInWinForm("Avoid melee distance (1 to 4)", "DoAvoidMeleeDistance", "Game Settings");
        }

        public static PaladinRetributionSettings CurrentSetting { get; set; }

        public static PaladinRetributionSettings GetSettings()
        {
            string currentSettingsFile = Application.StartupPath + "\\CombatClasses\\Settings\\Paladin_Retribution.xml";
            if (File.Exists(currentSettingsFile))
            {
                return
                    CurrentSetting = Load<PaladinRetributionSettings>(currentSettingsFile);
            }
            return new PaladinRetributionSettings();
        }
    }

    #endregion
}

#endregion

// ReSharper restore ObjectCreationAsStatement
// ReSharper restore EmptyGeneralCatchClause