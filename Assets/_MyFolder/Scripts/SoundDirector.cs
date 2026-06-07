using UnityEngine;

public class SoundDirector : MonoBehaviour
{
    [SerializeField] SoundData soundData;

    private void Awake()
    {
        //BGMで使用するサウンドデータをどこからでも再生できるようにBGMプールへ登録
        foreach (SoundData.BgmSound bgm in soundData.BgmSounds)
        {
            GSound.Instance.SetBgm(bgm.name.ToString(), bgm.clip);
        }

        //SEで使用するサウンドデータをどこからでも再生できるようにSEプールへ登録
        foreach (SoundData.SeSound se in soundData.SeSounds)
        {
            GSound.Instance.SetSe(se.name.ToString(), se.clip);
        }
    }
}