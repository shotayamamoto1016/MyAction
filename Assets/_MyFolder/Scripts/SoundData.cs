using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData")]
public class SoundData : ScriptableObject
{
    //BGM定義
    public enum BgmType
    {
        none,
        Title,
        Start,
        Boss,
    }

    //SE定義
    public enum SeType
    {
        none,
        Button_Click,      
        Block_Break,       
        Block_Hit,         
        Item_Appear,       
        Enemy_Koromochi,   
        Enemy_Chouchin,    
        Enemy_Takenoko,    
        Item_Freeze,       
        Item_Poison,       
        Item_Confuse
    }

    //BGMを管理するクラス
    [System.Serializable]
    public class BgmSound
    {
        //タイプ
        public BgmType name = BgmType.none;

        //オーディタイプ
        public AudioClip clip;
    }

    //SE情報を管理するクラス
    [System.Serializable]
    public class SeSound
    {
        //タイプ
        public SeType name = SeType.none;

        //オーディタイプ
        public AudioClip clip;
    }

    //BGM情報を保管するリスト
    public List<BgmSound> BgmSounds = new List<BgmSound>();

    //SE情報を保管するリスト
    public List<SeSound> SeSounds = new List<SeSound>();
}