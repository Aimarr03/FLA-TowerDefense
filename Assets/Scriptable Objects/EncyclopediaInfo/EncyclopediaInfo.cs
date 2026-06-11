using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encyclopedia Data", menuName = "Scriptable Objects/Create Encyclopedia Data")]
public class EncyclopediaInfo : ScriptableObject
{
    public EncyclopediaType EncyType;
    public Sprite Profile;
    public string Name;

    [TextArea(3,6)] public string Description;
    [TextArea(3,6)] public string Deskripsi;
    [SerializeField] public List<Vocab> Vocabularies;
    
    [Serializable]
    public class Vocab
    {
        public string ID;
        public string EN;    
    }
}
public enum EncyclopediaType
{
    Enemy,
    Tower
}
