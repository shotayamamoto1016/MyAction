# ぽんたのワナワナアドベンチャー

**「常識を捨てろ。このゲームに正解はない。」**

見た目と効果が真逆の理不尽な罠が次々と襲いかかる横スクロールアクションゲームです。

---

## 🎮 ゲームURL

▶️ [Unityroomでプレイする](https://unityroom.com/games/sypwa_029187)

---

## 📖 ゲーム概要

山奥のひっそりとした村でお父さん・お母さんと仲良く暮らしているレッサーパンダの「ぽんた」。
ある日のこと、両親から「隣町までお使いに行ってきて」と頼まれました。

ところが道中で大切なお金をなくしてしまい、
不思議な大穴へと吸い込まれてしまったぽんたが待ち受けるのは……
**常識の通用しない「ワナワナ」な罠だらけの迷宮**でした！

---

## 🕹️ 操作方法

| 操作 | キー |
|------|------|
| 右に進む | → キー / D キー |
| 左に進む | ← キー / A キー |
| ジャンプ | Space / W / J |
| ポーズ | Esc / P |

---

## 🌟 ゲームの特徴

- **理不尽な罠** — キノコを食べると死亡、土管から大量のころもち出現
- **裏ルート** — 隠し壁やふすまを使った隠し通路が存在
- **ボス戦** — ステージ5のラスボス「もち天さま」との激闘
- **多彩な敵** — 20種類以上の個性的な敵キャラクター
- **4種類のアイテム** — 無敵・混乱・凍り・毒キノコ

---

## 📁 ステージ構成

| ステージ | テーマ |
|----------|--------|
| 壱ノ巻 | 土の洞穴 |
| 弐ノ巻 | 岩の深層 |
| 参ノ巻 | 天空の罠 |
| 四ノ巻 | 漆黒の深淵 |
| 伍ノ巻 | マグマ地獄（ラスボス戦） |
| 終ノ巻 | 帰り道の罠（エンディング） |

---

## 👾 敵キャラクター一覧

| 敵名 | 特徴 |
|------|------|
| ころもち | 左右に歩く。踏むと倒せる |
| うらめし提灯 | ぽんたを追い越すと追いかけてくる |
| 竹の子スナイパー | 地面から出現し山なりの弾を発射 |
| のっかり石 | ブロックに偽装。触れると針が出る |
| つらら小僧 | 真下を通ると落下してくる |
| お助けガメ | 近づくと空高く投げ飛ばす |
| 火吹き鴉 | 空中を飛びながら左方向に炎を発射 |
| 一反ひらり | 触れると空高く連れ去られる |
| 影法師 | ぽんたを追い越すと全く同じ動きで追いかけてくる |
| マキコミガエル | ジャンプ攻撃と舌攻撃。2回踏むと倒せる |
| 爆炎だるま | 炎でブロックを壊しながら攻撃 |
| 溶岩もっち | ジャンプと這い移動を繰り返す |
| もち天さま | ステージ5のラスボス。3回の踏みつけ攻撃チャンスで撃破 |


---

## 🛠️ 使用技術

| 技術 | 用途 |
|------|------|
| Unity 6.0 (6000.0.49f1) | ゲームエンジン |
| C# | スクリプト全般 |
| DOTween | UIアニメーション・演出 |
| Cinemachine | カメラ追従・Dead Zone設定 |
| Unity Tilemap | ステージ地形 |
| TextMeshPro | UI文字表示 |
| Git / GitHub | バージョン管理 |

---

## 📂 プロジェクト構成

```
MyAction/
├── Assets/
│   └── _MyFolder/
│       ├── Scripts/                    # C#スクリプト一式
│       │   ├── PlayerController.cs     # ぽんたの移動・ジャンプ・状態管理
│       │   ├── GameManager.cs          # ライフ管理・フェード・リスポーン
│       │   ├── CheckpointManager.cs    # チェックポイント座標・提灯撃破記録
│       │   ├── StageResetManager.cs    # チェックポイント復活時のリセット管理
│       │   ├── PauseManager.cs         # ポーズ・タイムスケール管理
│       │   ├── GSound.cs               # BGM・SE管理（シングルトン）
│       │   ├── IResettable.cs          # リセット用インターフェース
│       │   ├── GoalDoor.cs             # ゴール扉・シーン遷移
│       │   ├── Fusuma.cs               # ふすま（同シーン内転送・シーン遷移）
│       │   ├── CheckpointFlag.cs       # チェックポイント旗
│       │   │
│       │   ├── Blocks/                 # ブロック系スクリプト
│       │   │   ├── CrumbleBlock.cs     # 崩れるブロック（復活あり）
│       │   │   ├── CollapseBlock.cs    # 崩れるブロック（復活なし・破片あり）
│       │   │   ├── BreakBlock.cs       # 頭突きで壊れるブロック
│       │   │   ├── HatenaBlock.cs      # ハテナブロック（茶色に変化）
│       │   │   ├── ItemBlock.cs        # アイテムブロック（キノコ出現）
│       │   │   ├── HiddenBlock.cs      # 隠しブロック
│       │   │   └── SpikeTrap.cs        # トゲトラップ（ボス戦エリア）
│       │   │
│       │   ├── Items/                  # アイテム系スクリプト
│       │   │   ├── GoldenMushroom.cs   # 金色キノコ（無敵）
│       │   │   ├── FreezeMushroomItem.cs   # 氷キノコ（即死）
│       │   │   ├── PoisonMushroomItem.cs   # 毒キノコ（即死）
│       │   │   └── ConfusedMushroomItem.cs # 混乱キノコ（20秒間左右反転）
│       │   │
│       │   ├── Enemys/                 # 敵スクリプト
│       │   │   ├── Koromochi.cs        # ころもち
│       │   │   ├── KoromochiLeft.cs    # 土管用ころもち（左のみ移動）
│       │   │   ├── Chochin.cs          # うらめし提灯
│       │   │   ├── TakenokoSniper.cs   # 竹の子スナイパー
│       │   │   ├── TakenokoBullet.cs   # 竹の子スナイパーの弾
│       │   │   ├── NokkariIshi.cs      # のっかり石
│       │   │   ├── TsuraraBoy.cs       # つらら小僧
│       │   │   ├── OtasukeTurtle.cs    # お助けガメ
│       │   │   ├── FireCrow.cs         # 火吹き鴉
│       │   │   ├── CrowFireball.cs     # 火吹き鴉の炎
│       │   │   ├── IttanHirari.cs      # 一反ひらり
│       │   │   ├── KageBoshi.cs        # 影法師
│       │   │   ├── MakikomiFrog.cs     # マキコミガエル
│       │   │   ├── LavaMocchi.cs       # 溶岩もっち
│       │   │   ├── BakuenDaruma.cs     # 爆炎だるま
│       │   │   ├── BakuenFlame.cs      # 爆炎だるまの炎
│       │   │   └── MochiTenBoss.cs     # ラスボス もち天さま
│       │   │
│       │   ├── Gimmicks/               # ギミック系スクリプト
│       │   │   ├── DokanSpawner.cs     # 土管スポーナー
│       │   │   ├── DokanBlock.cs       # 土管ON/OFFブロック
│       │   │   ├── BossStageGroundManager.cs # ボス戦地面管理
│       │   │   ├── BossTriggerZone.cs  # ボス戦トリガー
│       │   │   ├── FrogDefeatWall.cs   # カエル撃破後の壁崩壊
│       │   │   ├── PostBossGroundReveal.cs # ボス撃破後の地面出現
│       │   │   ├── EndingSpikeWall.cs  # エンディングのとげ壁
│       │   │   └── EndingBreakBlock.cs # エンディングの崩壊ブロック
│       │   │
│       │   └── Utils/                  # ユーティリティ
│       │       └── CoroutineRunner.cs  # 非アクティブオブジェクト用コルーチン実行
│       │
│       ├── Prefabs/                    # プレハブ一式
│       │   ├── Enemys/                 # 敵プレハブ
│       │   └── Items/                  # アイテムプレハブ
│       │
│       ├── Sprites/                    # 画像素材（Google AI生成）
│       │   ├── Player/                 # ぽんた関連
│       │   ├── Enemys/                 # 敵キャラクター
│       │   ├── Blocks/                 # ブロック
│       │   ├── Items/                  # アイテム
│       │   └── Backgrounds/            # 背景
│       │
│       ├── Scenes/                     # シーンファイル
│       │   ├── 00_Title.unity
│       │   ├── 01_Select.unity
│       │   ├── 02_Stage1.unity
│       │   ├── 03_Stage2.unity
│       │   ├── 04_Stage3.unity
│       │   ├── 05_Stage4.unity
│       │   ├── 06_Stage5.unity
│       │   └── 07_Ending.unity
│       │
│       ├── Sounds/                     # サウンド素材
│       │   ├── BGM/
│       │   │   ├── Start.mp3           # 通常ステージBGM
│       │   │   ├── Title.mp3           # タイトル画面BGM
│       │   │   ├── item_Gorden.mp3           # 金色キノコBGM
│       │   │   ├── Boss.mp3           # ボス戦BGM
│       │   │   └── Endhing.mp3            # エンディング画面BGM
│       │   └── SE/                     # 効果音
│       │
│       └── TilePalette/                # タイルパレット
│
├── Packages/
├── ProjectSettings/
└── README.md
```

---

## 🎨 素材について

- キャラクター・敵・背景画像：Google AI（Gemini）で生成
- BGM・SE：フリー素材を使用

---

## 🎨 素材について

- キャラクター・敵・背景画像：Google AIで生成
- BGM・SE：フリー素材を使用

---

## 📝 詳細記事

開発で工夫した点・難しかった点などの詳細はQiitaにまとめています。

👉 [ゲーム開発ポートフォリオ(ぽんたのワナワナアドベンチャー) - Qiita](https://qiita.com/shota20041016/items/fa95a14ecafbff5e6940)

---

## 👤 開発者

- **GitHub**：[shotayamamoto1016](https://github.com/shotayamamoto1016)

---
