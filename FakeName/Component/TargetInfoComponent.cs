using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FakeName.Config;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace FakeName.Component;

public class TargetInfoComponent : IDisposable
{
    private readonly PluginConfig config;
    
    private DateTime lastUpdate = DateTime.Today;
    
    public TargetInfoComponent(PluginConfig config)
    {
        this.config = config;
        
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfoMainTarget", TargetInfoUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_TargetInfoMainTarget", TargetTargetUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_FocusTargetInfo", FocusTargetInfoUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "_WideText", WideTextUpdate);
    }

    public void Dispose()
    {
        Service.AddonLifecycle.UnregisterListener(TargetInfoUpdate);
        Service.AddonLifecycle.UnregisterListener(TargetTargetUpdate);
        Service.AddonLifecycle.UnregisterListener(FocusTargetInfoUpdate);
        Service.AddonLifecycle.UnregisterListener(WideTextUpdate);
    }

    private unsafe void TargetInfoUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshTargetInfo(addon);
        }
    }

    private unsafe void TargetTargetUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            var resNode = addon->GetNodeById(3);
            if (resNode->IsVisible)
            {
                RefreshTargetTarget((AtkUnitBase*)args.Addon);
            }
        }
    }
    
    private unsafe void FocusTargetInfoUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshFocusTargetInfo(addon);
        }
    }
    
    private unsafe void WideTextUpdate(AddonEvent type, AddonArgs args)
    {
        var addon = (AtkUnitBase*)args.Addon;
        if (addon->IsVisible)
        {
            RefreshWideText(addon);
        }
    }

    private unsafe bool RefreshTargetInfo(AtkUnitBase* addon)
    {
        if (!config.Enabled)
        {
            return false;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        var targetObj = Service.Targets.Target;
        if (targetObj == null)
        {
            return false;
        }

        if (targetObj is not PlayerCharacter targetChar)
        {
            return false;
        }

        if (!config.TryGetCharacterConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        var newFcName = characterConfig.FakeFcNameText.Length > 0 ? $"«{characterConfig.FakeFcNameText}»" : $"«{targetChar.CompanyTag.TextValue}»";
        textNode->NodeText.SetString(text.Replace(targetChar.Name.TextValue, newName).Replace($"«{targetChar.CompanyTag.TextValue}»", newFcName));

        return true;
    }

    private unsafe bool RefreshTargetTarget(AtkUnitBase* addon)
    {
        if (!config.Enabled)
        {
            Service.Log.Debug($"a");
            return false;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            Service.Log.Debug($"b");
            return false;
        }
        
        var targetObj = Service.Targets.Target;
        if (targetObj == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(7);
        var text = textNode->NodeText.ToString();
        
        PlayerCharacter? targetTargetChara = null;
        var targetTargetObj = targetObj.TargetObject;
        if (targetTargetObj != null)
        {
            if (targetTargetObj is PlayerCharacter obj && text.Contains(obj.Name.TextValue))
            {
                targetTargetChara = obj;
            }
            else if (text.Contains(localPlayer.Name.TextValue))
            {
                targetTargetChara = localPlayer;
            }
        }
        else
        {
            targetTargetChara = localPlayer;
        }

        if (targetTargetChara == null)
        {
            return false;
        }

        if (!config.TryGetCharacterConfig(targetTargetChara.Name.TextValue, targetTargetChara.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return false;
        }
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetTargetChara.Name.TextValue;
        textNode->NodeText.SetString(text.Replace(targetTargetChara.Name.TextValue, newName));

        return true;
    }
    
    private unsafe bool RefreshFocusTargetInfo(AtkUnitBase* addon)
    {
        if (!config.Enabled)
        {
            return false;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }

        var focusTarget = Service.Targets.FocusTarget;
        if (focusTarget == null)
        {
            return false;
        }

        if (focusTarget is not PlayerCharacter targetChar)
        {
            return false;
        }

        if (!config.TryGetCharacterConfig(targetChar.Name.TextValue, targetChar.HomeWorld.Id, out var characterConfig) ||characterConfig == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = addon->GetTextNodeById(10);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : targetChar.Name.TextValue;
        textNode->NodeText.SetString(text.Replace(targetChar.Name.TextValue, newName));

        return true;
    }

    private unsafe bool RefreshWideText(AtkUnitBase* addon)
    {
        if (!config.Enabled)
        {
            return false;
        }
        
        var localPlayer = Service.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }
        
        // AtkUnitBase* addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_WideText", 2);
        // if (addon == null)
        // {
        //     return false;
        // }

        return RefreshPlayerWideText(localPlayer.Name.TextValue, localPlayer.HomeWorld.Id, addon);
        
        // 增加小队其他成员倒计时
    }

    private unsafe bool RefreshPlayerWideText(string name, uint worldId, AtkUnitBase* wideTextAddon)
    {
        if (!config.TryGetCharacterConfig(name, worldId, out var characterConfig) || characterConfig == null)
        {
            return false;
        }
        
        AtkTextNode* textNode = wideTextAddon->GetTextNodeById(3);
        var text = textNode->NodeText.ToString();
        
        var newName = characterConfig.FakeNameText.Length > 0 ? characterConfig.FakeNameText : name;
        if (text.EndsWith($"（{name}）") || text.StartsWith($"{name}取消了"))
        {
            textNode->NodeText.SetString(text.Replace(name, newName));
        }

        return true;
    }
}