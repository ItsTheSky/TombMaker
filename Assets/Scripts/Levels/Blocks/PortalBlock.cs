using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PortalBlock : Block
{

    public TMP_Text idIndicator;
    public Animator animator;

    [HideInInspector] public bool justTPed;
    [HideInInspector] public PortalBlock linked;
    [HideInInspector] public long portalId;

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.SliderOption("portal_id",
            "Portal ID", "This portal will lead to the other portal with the same ID.",
            0, 100, true, 0, value =>
            {
                portalId = (long) value;
                idIndicator.text = portalId.ToString();
            }));
    }
    
    public override void Enable()
    {
        CheckInit();
        
        animator.Play("PortalStatic");
        idIndicator.gameObject.SetActive(true);
    }
    
    public override void Disable()
    {
        CheckInit();
        
        animator.Play("Portal");
        idIndicator.gameObject.SetActive(false);
    }

    public void CollideWithPlayer()
    {
        if (linked == null)
            return;
        if (justTPed)
        {
            justTPed = false;
            _playerScript.EnableTrail();
            return;
        }

        _playerScript.DisableTrail();
        linked.justTPed = true;
        _playerScript.SetPos(linked.GetPos());
    }

    public override Dictionary<string, object> SaveSettings()
    {
        return new Dictionary<string, object>()
        {
            {"portal_id", portalId}
        };
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        portalId = (settings.ContainsKey("portal_id") ? (long)settings["portal_id"] : 0);
        idIndicator.text = portalId.ToString();
    }
}