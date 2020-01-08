

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceablesData : MonoBehaviour {

    public float MaxUpAngle;
    public float MetalProduction;
    public float EnergyProduction;
    public float BuildTime;
    public float MetalCost;
    public float EnergyCost;
    public float ReclaimationTime;
    public float MetalReclaim;
    public float EnergyReclaim;
    public float MetalStorage;
    public float EnergyStorage;
    public bool IsWeapon;
    public bool IsTech;
    public bool IsGroundOnly;
    public bool IsWindBasedEnergyProduction;
    public int LimitOfLikePlaceables = 0;
    public string RequiresTech = "";
    public Sprite HUDSprite;
}
