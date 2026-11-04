using System;
using UnityEngine;

namespace TowerDefenseBuilderSimple
{
    [Serializable]
    public class BuildingConfig
    {
        public BuildingType type;
        public GameObject prefab;
        public Sprite icon;
    }
}