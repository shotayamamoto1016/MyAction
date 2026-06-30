using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class GSound
{
    //シングルトン化
    private static GSound instance;

    public static GSound Instance
    {
        get
        {
            if (instance == null) instance = new GSound();
            return instance;
        }
    }

    //サウンド種別
    enum Type
    {
        bgm,
        se,
    }

    //サウンド再生のためのゲームオブジェクト
    GameObject obj;

    //サウンドリソース
    AudioSource sourceBgm;
    AudioSource sourceSe;

    //BGMデータプール
    Dictionary<string, AudioClip> poolBgm = new Dictionary<string, AudioClip>();

    //SEデータプール
    Dictionary<string, AudioClip> poolSe = new Dictionary<string, AudioClip>();

    //音量
    public float bgmVolume = 1.0f;
    public float seVolume = 1.0f;

    //SE同時防止キュー
    //Queue<string> seQueue = new Queue<string>();
    Queue<SeRequest> seQueue = new Queue<SeRequest>();

    //最大SE同時発生数
    int maxSeCount = 10;

    //同名SE排除フラグ
    bool sameSeCheck = true;


    //AudioSourceを取得する
    AudioSource GetAudioSource(Type type)
    {
        //GameObjectがなければ作る
        if (obj == null)
        {
            obj = new GameObject("Sound");

            // シーンをまたいでもこのオブジェクトを消さないように設定
            Object.DontDestroyOnLoad(obj);

            //AudioSourceの作成
            sourceBgm = obj.AddComponent<AudioSource>();
            sourceSe = obj.AddComponent<AudioSource>();
        }

        if (type == Type.bgm)
        {
            //BGM
            return sourceBgm;
        }

        else
        {
            //SE
            return sourceSe;
        }
    }
    //BGMをプールにセット
    public void SetBgm(string fileName, AudioClip clip)
    {
        //既に登録済みなのでいったん消す
        if (poolBgm.ContainsKey(fileName))
        {
            poolBgm.Remove(fileName);
        }

        poolBgm.Add(fileName, clip);
    }

    //SEをプールにセット
    public void SetSe(string fileName, AudioClip clip)
    {
        //既に登録済みなのでいったん消す
        if (poolSe.ContainsKey(fileName))
        {
            poolSe.Remove(fileName);
        }

        poolSe.Add(fileName, clip);
    }

    //BGMの再生
    public bool PlayBgm(string fileName, bool loop)
    {
        //指定ファイルがない
        if (poolBgm.ContainsKey(fileName) == false) return false;

        //現在のBGMを止める
        //StopBgm();

        //リソースの取得
        AudioSource source = GetAudioSource(Type.bgm);

        AudioClip clip = poolBgm[fileName];

        // もし今流れている曲がこれから流そうとする曲と同じならそのままにする
        if (source.clip == clip && source.isPlaying)
        {
            return true; // 再生し直さずに終了
        }

        // ここから下は別の曲を流す時だけ実行される
        StopBgm();
        source.clip = clip;
        source.volume = bgmVolume;
        source.loop = loop;

        //再生
        source.Play();

        return true;
    }

    //SEの再生
    public bool PlaySe(string fileName, float volumeScale = 1.0f)
    {
        //指定フィアルがない
        if (poolSe.ContainsKey(fileName) == false) return false;

        //SEをキューに登録
        if (seQueue.Count < maxSeCount)
        {
            //同名SE排除チェックi
            if (sameSeCheck)
            {
                //同じSEがキュー内にある場合は登録しない
                foreach (var req in seQueue)
                {
                    if (req.name == fileName) return true;
                }

            }
            // 名前と音量倍率をセットで入れる
            seQueue.Enqueue(new SeRequest { name = fileName, volume = volumeScale });
        }

        return true;
    }


    //1フレームに1回呼ばれたキューは消化する
    public void CheckSeQueue()
    {
        if (seQueue.Count > 0)
        {
            AudioSource source = GetAudioSource(Type.se);
            SeRequest request = seQueue.Dequeue(); 

            AudioClip clip = poolSe[request.name];

            source.PlayOneShot(clip, seVolume * request.volume);
        }
    }


    //BGMの停止
    public void StopBgm()
    {
        GetAudioSource(Type.bgm).Stop();
    }

    //BGMの音量変更
    public void bgmVolumeChange(float volume)
    {
        bgmVolume = volume;
        AudioSource source = GetAudioSource(Type.bgm);
        if (source != null)
        {
            source.volume = bgmVolume;
        }
        PlayerPrefs.SetFloat("BGM_VOLUME", volume); // 数値を保存
        PlayerPrefs.Save();
    }

    public void seVolumeChange(float volume)
    {
        seVolume = volume;
    }

    struct SeRequest
    {
        public string name;
        public float volume;
    }
}