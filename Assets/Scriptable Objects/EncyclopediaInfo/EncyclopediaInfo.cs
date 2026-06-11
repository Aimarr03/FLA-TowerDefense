using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encyclopedia Data", menuName = "Scriptable Objects/Create Encyclopedia Data")]
public class EncyclopediaInfo : ScriptableObject
{
    public EncyclopediaType EncyType;
    public Sprite Profile;
    public string Name;
    public HPType HP;
    public MSType MS;

    [TextArea(3,6)] public string Description;
    [TextArea(3,6)] public string Deskripsi;
    [SerializeField] public List<Vocab> Vocabularies;
    
    [Serializable]
    public class Vocab
    {
        public string ID;
        public string EN;

        public override string ToString()
        {
            return $"{EN} = {ID}";
        }
    }
    public enum HPType
    {
        Low,
        Medium,
        High
    }
    public enum MSType
    {
        VerySlow,
        Slow,
        Medium,
        Fast,
        VeryFast
    }
}
public enum EncyclopediaType
{
    Enemy,
    Tower
}
