using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V508
{
    public enum EROLanguage
    {
        French, English, German, Spanish, Italian, Dutch, Portuguese, Japanese, Korean, ChineseSimplified
    }

    public sealed class EROLocalizationV508 : MonoBehaviour
    {
        public static EROLocalizationV508 Instance { get; private set; }
        public EROLanguage CurrentLanguage { get; private set; }
        public event Action<EROLanguage> LanguageChanged;

        readonly Dictionary<EROLanguage, Dictionary<string, string>> tables = new Dictionary<EROLanguage, Dictionary<string, string>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("ERO_V508_Localization");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<EROLocalizationV508>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildTables();
            CurrentLanguage = LoadLanguage();
        }

        EROLanguage LoadLanguage()
        {
            if (PlayerPrefs.HasKey("ERO_LANGUAGE"))
            {
                int value = PlayerPrefs.GetInt("ERO_LANGUAGE", (int)EROLanguage.French);
                if (Enum.IsDefined(typeof(EROLanguage), value)) return (EROLanguage)value;
            }
            switch (Application.systemLanguage)
            {
                case SystemLanguage.French: return EROLanguage.French;
                case SystemLanguage.German: return EROLanguage.German;
                case SystemLanguage.Spanish: return EROLanguage.Spanish;
                case SystemLanguage.Italian: return EROLanguage.Italian;
                case SystemLanguage.Dutch: return EROLanguage.Dutch;
                case SystemLanguage.Portuguese: return EROLanguage.Portuguese;
                case SystemLanguage.Japanese: return EROLanguage.Japanese;
                case SystemLanguage.Korean: return EROLanguage.Korean;
                case SystemLanguage.ChineseSimplified: return EROLanguage.ChineseSimplified;
                default: return EROLanguage.English;
            }
        }

        public void SetLanguage(EROLanguage language)
        {
            if (!tables.ContainsKey(language)) language = EROLanguage.English;
            CurrentLanguage = language;
            PlayerPrefs.SetInt("ERO_LANGUAGE", (int)language);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke(language);
        }

        public string Get(string key)
        {
            Dictionary<string, string> table;
            if (tables.TryGetValue(CurrentLanguage, out table) && table.TryGetValue(key, out var value)) return value;
            if (tables.TryGetValue(EROLanguage.English, out table) && table.TryGetValue(key, out value)) return value;
            return key;
        }

        public string Get(string key, params object[] args)
        {
            var value = Get(key);
            return args == null || args.Length == 0 ? value : string.Format(value, args);
        }

        void BuildTables()
        {
            var en = new Dictionary<string, string>
            {
                ["menu.play"]="Play", ["menu.settings"]="Settings", ["menu.exit"]="Exit", ["menu.language"]="Language",
                ["charselect.title"]="CREATE YOUR CHARACTER", ["charselect.create"]="CREATE CHARACTER", ["charselect.male"]="MALE", ["charselect.female"]="FEMALE",
                ["ui.inventory"]="Inventory", ["ui.character"]="Character", ["ui.skills"]="Skills", ["ui.map"]="Map", ["ui.quests"]="Quests", ["ui.guild"]="Guild", ["ui.pet"]="Pet",
                ["currency.crystals"]="ERO Crystals", ["currency.gold"]="Gold", ["founder.badge"]="Founder", ["founder.free"]="Founder entitlement: paid content unlocked",
                ["progress.level"]="Level {0}", ["progress.evolution"]="Evolution", ["progress.branch"]="Branch", ["progress.base"]="Base Class",
                ["system.connected"]="Connected", ["system.offline"]="Offline", ["system.loading"]="Loading..."
            };
            var fr = new Dictionary<string, string>
            {
                ["menu.play"]="Jouer", ["menu.settings"]="Paramètres", ["menu.exit"]="Quitter", ["menu.language"]="Langue",
                ["charselect.title"]="CRÉER VOTRE PERSONNAGE", ["charselect.create"]="CRÉER LE PERSONNAGE", ["charselect.male"]="HOMME", ["charselect.female"]="FEMME",
                ["ui.inventory"]="Inventaire", ["ui.character"]="Personnage", ["ui.skills"]="Compétences", ["ui.map"]="Carte", ["ui.quests"]="Quêtes", ["ui.guild"]="Guilde", ["ui.pet"]="Familier",
                ["currency.crystals"]="Cristaux ERO", ["currency.gold"]="Or", ["founder.badge"]="Fondateur", ["founder.free"]="Avantage Fondateur : contenu payant débloqué",
                ["progress.level"]="Niveau {0}", ["progress.evolution"]="Évolution", ["progress.branch"]="Branche", ["progress.base"]="Classe de base",
                ["system.connected"]="Connecté", ["system.offline"]="Hors ligne", ["system.loading"]="Chargement..."
            };
            var de = new Dictionary<string, string> { ["menu.play"]="Spielen", ["menu.settings"]="Einstellungen", ["menu.exit"]="Beenden", ["menu.language"]="Sprache", ["charselect.create"]="CHARAKTER ERSTELLEN", ["ui.inventory"]="Inventar", ["ui.guild"]="Gilde", ["currency.crystals"]="ERO-Kristalle", ["currency.gold"]="Gold" };
            var es = new Dictionary<string, string> { ["menu.play"]="Jugar", ["menu.settings"]="Ajustes", ["menu.exit"]="Salir", ["menu.language"]="Idioma", ["charselect.create"]="CREAR PERSONAJE", ["ui.inventory"]="Inventario", ["ui.guild"]="Gremio", ["currency.crystals"]="Cristales ERO", ["currency.gold"]="Oro" };
            var it = new Dictionary<string, string> { ["menu.play"]="Gioca", ["menu.settings"]="Impostazioni", ["menu.exit"]="Esci", ["menu.language"]="Lingua", ["charselect.create"]="CREA PERSONAGGIO", ["ui.inventory"]="Inventario", ["ui.guild"]="Gilda", ["currency.crystals"]="Cristalli ERO", ["currency.gold"]="Oro" };
            var nl = new Dictionary<string, string> { ["menu.play"]="Spelen", ["menu.settings"]="Instellingen", ["menu.exit"]="Afsluiten", ["menu.language"]="Taal", ["charselect.create"]="PERSONAGE MAKEN", ["ui.inventory"]="Inventaris", ["ui.guild"]="Gilde", ["currency.crystals"]="ERO-kristallen", ["currency.gold"]="Goud" };
            var pt = new Dictionary<string, string> { ["menu.play"]="Jogar", ["menu.settings"]="Configurações", ["menu.exit"]="Sair", ["menu.language"]="Idioma", ["charselect.create"]="CRIAR PERSONAGEM", ["ui.inventory"]="Inventário", ["ui.guild"]="Guilda", ["currency.crystals"]="Cristais ERO", ["currency.gold"]="Ouro" };
            var ja = new Dictionary<string, string> { ["menu.play"]="プレイ", ["menu.settings"]="設定", ["menu.exit"]="終了", ["menu.language"]="言語", ["charselect.create"]="キャラクター作成", ["ui.inventory"]="インベントリ", ["ui.guild"]="ギルド", ["currency.crystals"]="EROクリスタル", ["currency.gold"]="ゴールド" };
            var ko = new Dictionary<string, string> { ["menu.play"]="플레이", ["menu.settings"]="설정", ["menu.exit"]="종료", ["menu.language"]="언어", ["charselect.create"]="캐릭터 생성", ["ui.inventory"]="인벤토리", ["ui.guild"]="길드", ["currency.crystals"]="ERO 크리스탈", ["currency.gold"]="골드" };
            var zh = new Dictionary<string, string> { ["menu.play"]="开始游戏", ["menu.settings"]="设置", ["menu.exit"]="退出", ["menu.language"]="语言", ["charselect.create"]="创建角色", ["ui.inventory"]="背包", ["ui.guild"]="公会", ["currency.crystals"]="ERO水晶", ["currency.gold"]="金币" };
            tables[EROLanguage.English]=en; tables[EROLanguage.French]=fr; tables[EROLanguage.German]=Merge(en,de); tables[EROLanguage.Spanish]=Merge(en,es); tables[EROLanguage.Italian]=Merge(en,it); tables[EROLanguage.Dutch]=Merge(en,nl); tables[EROLanguage.Portuguese]=Merge(en,pt); tables[EROLanguage.Japanese]=Merge(en,ja); tables[EROLanguage.Korean]=Merge(en,ko); tables[EROLanguage.ChineseSimplified]=Merge(en,zh);
            AddCharSelectClassTerms();
            AddCharSelectTerms();
            AddV518CharacterCreationTerms();
            AddCharacterCreatorTerms();
        }


        void AddCharacterCreatorTerms()
        {
            string[] k={"charcreate.gender","charcreate.face","charcreate.hair","charcreate.haircolor","charcreate.eyecolor","charcreate.skin","charcreate.gear"};
            string[][] v={
                new[]{"GENRE","GENDER","GESCHLECHT","GÉNERO","GENERE","GESLACHT","GÉNERO","性別","성별","性别"},
                new[]{"VISAGE","FACE","GESICHT","ROSTRO","VISO","GEZICHT","ROSTO","顔","얼굴","脸型"},
                new[]{"COIFFURE","HAIRSTYLE","FRISUR","PEINADO","ACCONCIATURA","KAPSEL","PENTEADO","髪型","헤어스타일","发型"},
                new[]{"COULEUR DES CHEVEUX","HAIR COLOR","HAARFARBE","COLOR DEL CABELLO","COLORE CAPELLI","HAARKLEUR","COR DO CABELO","髪色","머리 색상","发色"},
                new[]{"COULEUR DES YEUX","EYE COLOR","AUGENFARBE","COLOR DE OJOS","COLORE OCCHI","OOGKLEUR","COR DOS OLHOS","目の色","눈 색상","眼睛颜色"},
                new[]{"TEINT DE PEAU","SKIN TONE","HAUTTÖNUNG","TONO DE PIEL","TONO DELLA PELLE","HUIDSKLEUR","TOM DE PELE","肌の色","피부색","肤色"},
                new[]{"APERÇU ÉQUIPEMENT","EQUIPMENT PREVIEW","AUSRÜSTUNGSVORSCHAU","VISTA PREVIA EQUIPO","ANTEPRIMA EQUIPAGGIAMENTO","UITRUSTINGSVOORBEELD","PRÉVIA DO EQUIPAMENTO","装備プレビュー","장비 미리보기","装备预览"}
            };
            EROLanguage[] langs={EROLanguage.French,EROLanguage.English,EROLanguage.German,EROLanguage.Spanish,EROLanguage.Italian,EROLanguage.Dutch,EROLanguage.Portuguese,EROLanguage.Japanese,EROLanguage.Korean,EROLanguage.ChineseSimplified};
            for(int i=0;i<k.Length;i++) for(int j=0;j<langs.Length;j++) Add(langs[j],k[i],v[i][j]);
        }

        void AddCharSelectTerms()
        {
            Add(EROLanguage.English, "charselect.subtitle", "CHARACTER CREATION • AETHERIA"); Add(EROLanguage.English, "charselect.choose", "CHOOSE YOUR CLASS"); Add(EROLanguage.English, "charselect.name", "CHARACTER NAME"); Add(EROLanguage.English, "charselect.style", "STYLE"); Add(EROLanguage.English, "charselect.ready", "READY • WAITING FOR PARTY");
            Add(EROLanguage.French, "charselect.subtitle", "CRÉATION DE PERSONNAGE • AETHERIA"); Add(EROLanguage.French, "charselect.choose", "CHOISISSEZ VOTRE CLASSE"); Add(EROLanguage.French, "charselect.name", "NOM DU PERSONNAGE"); Add(EROLanguage.French, "charselect.style", "STYLE"); Add(EROLanguage.French, "charselect.ready", "PRÊT • EN ATTENTE DU GROUPE");
            Add(EROLanguage.German, "charselect.subtitle", "CHARAKTERERSTELLUNG • AETHERIA"); Add(EROLanguage.German, "charselect.choose", "WÄHLE DEINE KLASSE"); Add(EROLanguage.German, "charselect.name", "CHARAKTERNAME"); Add(EROLanguage.German, "charselect.style", "STIL"); Add(EROLanguage.German, "charselect.ready", "BEREIT • WARTE AUF GRUPPE");
            Add(EROLanguage.Spanish, "charselect.subtitle", "CREACIÓN DE PERSONAJE • AETHERIA"); Add(EROLanguage.Spanish, "charselect.choose", "ELIGE TU CLASE"); Add(EROLanguage.Spanish, "charselect.name", "NOMBRE DEL PERSONAJE"); Add(EROLanguage.Spanish, "charselect.style", "ESTILO"); Add(EROLanguage.Spanish, "charselect.ready", "LISTO • ESPERANDO AL GRUPO");
            Add(EROLanguage.Italian, "charselect.subtitle", "CREAZIONE DEL PERSONAGGIO • AETHERIA"); Add(EROLanguage.Italian, "charselect.choose", "SCEGLI LA CLASSE"); Add(EROLanguage.Italian, "charselect.name", "NOME DEL PERSONAGGIO"); Add(EROLanguage.Italian, "charselect.style", "STILE"); Add(EROLanguage.Italian, "charselect.ready", "PRONTO • IN ATTESA DEL GRUPPO");
            Add(EROLanguage.Dutch, "charselect.subtitle", "PERSONAGE MAKEN • AETHERIA"); Add(EROLanguage.Dutch, "charselect.choose", "KIES JE KLASSE"); Add(EROLanguage.Dutch, "charselect.name", "PERSONAGENAAM"); Add(EROLanguage.Dutch, "charselect.style", "STIJL"); Add(EROLanguage.Dutch, "charselect.ready", "KLAAR • WACHT OP GROEP");
            Add(EROLanguage.Portuguese, "charselect.subtitle", "CRIAÇÃO DE PERSONAGEM • AETHERIA"); Add(EROLanguage.Portuguese, "charselect.choose", "ESCOLHA SUA CLASSE"); Add(EROLanguage.Portuguese, "charselect.name", "NOME DO PERSONAGEM"); Add(EROLanguage.Portuguese, "charselect.style", "ESTILO"); Add(EROLanguage.Portuguese, "charselect.ready", "PRONTO • AGUARDANDO O GRUPO");
            Add(EROLanguage.Japanese, "charselect.subtitle", "キャラクター作成 • AETHERIA"); Add(EROLanguage.Japanese, "charselect.choose", "クラスを選択"); Add(EROLanguage.Japanese, "charselect.name", "キャラクター名"); Add(EROLanguage.Japanese, "charselect.style", "スタイル"); Add(EROLanguage.Japanese, "charselect.ready", "準備完了 • パーティー待機中");
            Add(EROLanguage.Korean, "charselect.subtitle", "캐릭터 생성 • AETHERIA"); Add(EROLanguage.Korean, "charselect.choose", "클래스를 선택하세요"); Add(EROLanguage.Korean, "charselect.name", "캐릭터 이름"); Add(EROLanguage.Korean, "charselect.style", "스타일"); Add(EROLanguage.Korean, "charselect.ready", "준비 완료 • 파티 대기 중");
            Add(EROLanguage.ChineseSimplified, "charselect.subtitle", "创建角色 • AETHERIA"); Add(EROLanguage.ChineseSimplified, "charselect.choose", "选择职业"); Add(EROLanguage.ChineseSimplified, "charselect.name", "角色名称"); Add(EROLanguage.ChineseSimplified, "charselect.style", "外观"); Add(EROLanguage.ChineseSimplified, "charselect.ready", "准备就绪 • 等待队伍");
        }


        void AddCharSelectClassTerms()
        {
            Add(EROLanguage.English, "menu.brand", "ETERNAL REALMS ONLINE");
            Add(EROLanguage.French, "menu.brand", "ETERNAL REALMS ONLINE");
            string[,] data = new string[,] {
                {"class.knight","KNIGHT","CHEVALIER","RITTER","CABALLERO","CAVALIERE","RIDDER","CAVALEIRO","ナイト","기사","骑士"},
                {"class.assassin","ASSASSIN","ASSASSIN","ATTENTÄTER","ASESINO","ASSASSINO","MOORDENAAR","ASSASSINO","アサシン","어쌔신","刺客"},
                {"class.ranger","RANGER","RÔDEUR","WALDLÄUFER","EXPLORADOR","RANGER","JAGER","GUERREIRO","レンジャー","레인저","游侠"},
                {"class.mage","MAGE","MAGE","MAGIER","MAGO","MAGO","TOVENAAR","MAGO","メイジ","메이지","法师"},
                {"class.priest","PRIEST","PRÊTRE","PRIESTER","SACERDOTE","SACERDOTE","PRIester","SACERDOTE","プリースト","프리스트","牧师"},
                {"class.monk","MONK","MOINE","MÖNCH","MONJE","MONACO","MONNIK","MONGE","モンク","몽크","武僧"},
                {"class.summoner","SUMMONER","INVOCATEUR","BESCHWÖRER","INVOCADOR","EVOCATORE","OPROEPER","INVOCADOR","サモナー","소환사","召唤师"},
                {"class.paladin","PALADIN","PALADIN","PALADIN","PALADÍN","PALADINO","PALADIN","PALADINO","パラディン","팔라딘","圣骑士"}
            };
            EROLanguage[] langs = { EROLanguage.English, EROLanguage.French, EROLanguage.German, EROLanguage.Spanish, EROLanguage.Italian, EROLanguage.Dutch, EROLanguage.Portuguese, EROLanguage.Japanese, EROLanguage.Korean, EROLanguage.ChineseSimplified };
            for (int row = 0; row < data.GetLength(0); row++)
                for (int col = 0; col < langs.Length; col++) Add(langs[col], data[row,0], data[row,col+1]);
            Add(EROLanguage.English, "class.firstEvolution", "FIRST EVOLUTION"); Add(EROLanguage.French, "class.firstEvolution", "PREMIÈRE ÉVOLUTION"); Add(EROLanguage.German, "class.firstEvolution", "ERSTE ENTWICKLUNG"); Add(EROLanguage.Spanish, "class.firstEvolution", "PRIMERA EVOLUCIÓN"); Add(EROLanguage.Italian, "class.firstEvolution", "PRIMA EVOLUZIONE"); Add(EROLanguage.Dutch, "class.firstEvolution", "EERSTE EVOLUTIE"); Add(EROLanguage.Portuguese, "class.firstEvolution", "PRIMEIRA EVOLUÇÃO"); Add(EROLanguage.Japanese, "class.firstEvolution", "第一次進化"); Add(EROLanguage.Korean, "class.firstEvolution", "첫 번째 진화"); Add(EROLanguage.ChineseSimplified, "class.firstEvolution", "第一次进化");
            Add(EROLanguage.English, "class.startingRole", "STARTING ROLE"); Add(EROLanguage.French, "class.startingRole", "RÔLE INITIAL"); Add(EROLanguage.German, "class.startingRole", "STARTROLLE"); Add(EROLanguage.Spanish, "class.startingRole", "ROL INICIAL"); Add(EROLanguage.Italian, "class.startingRole", "RUOLO INIZIALE"); Add(EROLanguage.Dutch, "class.startingRole", "STARTROL"); Add(EROLanguage.Portuguese, "class.startingRole", "FUNÇÃO INICIAL"); Add(EROLanguage.Japanese, "class.startingRole", "初期ロール"); Add(EROLanguage.Korean, "class.startingRole", "시작 역할"); Add(EROLanguage.ChineseSimplified, "class.startingRole", "初始定位");
            Add(EROLanguage.English, "class.weapon", "WEAPON"); Add(EROLanguage.French, "class.weapon", "ARME"); Add(EROLanguage.German, "class.weapon", "WAFFE"); Add(EROLanguage.Spanish, "class.weapon", "ARMA"); Add(EROLanguage.Italian, "class.weapon", "ARMA"); Add(EROLanguage.Dutch, "class.weapon", "WAPEN"); Add(EROLanguage.Portuguese, "class.weapon", "ARMA"); Add(EROLanguage.Japanese, "class.weapon", "武器"); Add(EROLanguage.Korean, "class.weapon", "무기"); Add(EROLanguage.ChineseSimplified, "class.weapon", "武器");
            Add(EROLanguage.English, "class.level", "LEVEL"); Add(EROLanguage.French, "class.level", "NIVEAU"); Add(EROLanguage.German, "class.level", "STUFE"); Add(EROLanguage.Spanish, "class.level", "NIVEL"); Add(EROLanguage.Italian, "class.level", "LIVELLO"); Add(EROLanguage.Dutch, "class.level", "NIVEAU"); Add(EROLanguage.Portuguese, "class.level", "NÍVEL"); Add(EROLanguage.Japanese, "class.level", "レベル"); Add(EROLanguage.Korean, "class.level", "레벨"); Add(EROLanguage.ChineseSimplified, "class.level", "等级");
            Add(EROLanguage.English, "class.playstyle", "PLAYSTYLE"); Add(EROLanguage.French, "class.playstyle", "STYLE DE JEU"); Add(EROLanguage.German, "class.playstyle", "SPIELSTIL"); Add(EROLanguage.Spanish, "class.playstyle", "ESTILO DE JUEGO"); Add(EROLanguage.Italian, "class.playstyle", "STILE DI GIOCO"); Add(EROLanguage.Dutch, "class.playstyle", "SPEELSTIJL"); Add(EROLanguage.Portuguese, "class.playstyle", "ESTILO DE JOGO"); Add(EROLanguage.Japanese, "class.playstyle", "プレイスタイル"); Add(EROLanguage.Korean, "class.playstyle", "플레이 스타일"); Add(EROLanguage.ChineseSimplified, "class.playstyle", "玩法");
            Add(EROLanguage.English, "charselect.readyShort", "READY"); Add(EROLanguage.French, "charselect.readyShort", "PRÊT"); Add(EROLanguage.German, "charselect.readyShort", "BEREIT"); Add(EROLanguage.Spanish, "charselect.readyShort", "LISTO"); Add(EROLanguage.Italian, "charselect.readyShort", "PRONTO"); Add(EROLanguage.Dutch, "charselect.readyShort", "KLAAR"); Add(EROLanguage.Portuguese, "charselect.readyShort", "PRONTO"); Add(EROLanguage.Japanese, "charselect.readyShort", "準備完了"); Add(EROLanguage.Korean, "charselect.readyShort", "준비"); Add(EROLanguage.ChineseSimplified, "charselect.readyShort", "准备");
        }

        void AddV518CharacterCreationTerms()
        {
            Add(EROLanguage.English, "charselect.preview", "CHARACTER PREVIEW");
            Add(EROLanguage.French, "charselect.preview", "APERÇU DU PERSONNAGE");
            Add(EROLanguage.German, "charselect.preview", "CHARAKTERVORSCHAU");
            Add(EROLanguage.Spanish, "charselect.preview", "VISTA PREVIA DEL PERSONAJE");
            Add(EROLanguage.Italian, "charselect.preview", "ANTEPRIMA DEL PERSONAGGIO");
            Add(EROLanguage.Dutch, "charselect.preview", "PERSONAGEVOORBEELD");
            Add(EROLanguage.Portuguese, "charselect.preview", "PRÉVIA DO PERSONAGEM");
            Add(EROLanguage.Japanese, "charselect.preview", "キャラクタープレビュー");
            Add(EROLanguage.Korean, "charselect.preview", "캐릭터 미리보기");
            Add(EROLanguage.ChineseSimplified, "charselect.preview", "角色预览");
            Add(EROLanguage.English, "charselect.previewHint", "Select a class to preview your hero");
            Add(EROLanguage.French, "charselect.previewHint", "Sélectionnez une classe pour voir votre héros");
            Add(EROLanguage.German, "charselect.previewHint", "Wähle eine Klasse, um deinen Helden zu sehen");
            Add(EROLanguage.Spanish, "charselect.previewHint", "Elige una clase para ver a tu héroe");
            Add(EROLanguage.Italian, "charselect.previewHint", "Scegli una classe per vedere il tuo eroe");
            Add(EROLanguage.Dutch, "charselect.previewHint", "Kies een klasse om je held te bekijken");
            Add(EROLanguage.Portuguese, "charselect.previewHint", "Escolha uma classe para ver seu herói");
            Add(EROLanguage.Japanese, "charselect.previewHint", "クラスを選択してヒーローを確認");
            Add(EROLanguage.Korean, "charselect.previewHint", "클래스를 선택하여 영웅을 미리 보세요");
            Add(EROLanguage.ChineseSimplified, "charselect.previewHint", "选择职业预览你的英雄");
            Add(EROLanguage.English, "charselect.hair1", "HAIR 1"); Add(EROLanguage.French, "charselect.hair1", "COIFFURE 1"); Add(EROLanguage.German, "charselect.hair1", "HAAR 1"); Add(EROLanguage.Spanish, "charselect.hair1", "PELO 1"); Add(EROLanguage.Italian, "charselect.hair1", "CAPELLI 1"); Add(EROLanguage.Dutch, "charselect.hair1", "HAAR 1"); Add(EROLanguage.Portuguese, "charselect.hair1", "CABELO 1"); Add(EROLanguage.Japanese, "charselect.hair1", "髪型 1"); Add(EROLanguage.Korean, "charselect.hair1", "헤어 1"); Add(EROLanguage.ChineseSimplified, "charselect.hair1", "发型 1");
            Add(EROLanguage.English, "charselect.hair2", "HAIR 2"); Add(EROLanguage.French, "charselect.hair2", "COIFFURE 2"); Add(EROLanguage.German, "charselect.hair2", "HAAR 2"); Add(EROLanguage.Spanish, "charselect.hair2", "PELO 2"); Add(EROLanguage.Italian, "charselect.hair2", "CAPELLI 2"); Add(EROLanguage.Dutch, "charselect.hair2", "HAAR 2"); Add(EROLanguage.Portuguese, "charselect.hair2", "CABELO 2"); Add(EROLanguage.Japanese, "charselect.hair2", "髪型 2"); Add(EROLanguage.Korean, "charselect.hair2", "헤어 2"); Add(EROLanguage.ChineseSimplified, "charselect.hair2", "发型 2");
            Add(EROLanguage.English, "charselect.hair3", "HAIR 3"); Add(EROLanguage.French, "charselect.hair3", "COIFFURE 3"); Add(EROLanguage.German, "charselect.hair3", "HAAR 3"); Add(EROLanguage.Spanish, "charselect.hair3", "PELO 3"); Add(EROLanguage.Italian, "charselect.hair3", "CAPELLI 3"); Add(EROLanguage.Dutch, "charselect.hair3", "HAAR 3"); Add(EROLanguage.Portuguese, "charselect.hair3", "CABELO 3"); Add(EROLanguage.Japanese, "charselect.hair3", "髪型 3"); Add(EROLanguage.Korean, "charselect.hair3", "헤어 3"); Add(EROLanguage.ChineseSimplified, "charselect.hair3", "发型 3");
            Add(EROLanguage.English, "progress.branchAvailable", "Available at Lv. 18"); Add(EROLanguage.French, "progress.branchAvailable", "Disponible au niv. 18"); Add(EROLanguage.German, "progress.branchAvailable", "Ab Stufe 18 verfügbar"); Add(EROLanguage.Spanish, "progress.branchAvailable", "Disponible en Nv. 18"); Add(EROLanguage.Italian, "progress.branchAvailable", "Disponibile al Lv. 18"); Add(EROLanguage.Dutch, "progress.branchAvailable", "Beschikbaar op lvl. 18"); Add(EROLanguage.Portuguese, "progress.branchAvailable", "Disponível no Nv. 18"); Add(EROLanguage.Japanese, "progress.branchAvailable", "Lv.18で解放"); Add(EROLanguage.Korean, "progress.branchAvailable", "Lv.18 해금"); Add(EROLanguage.ChineseSimplified, "progress.branchAvailable", "18级解锁");
            Add(EROLanguage.English, "progress.finalEvolution", "Lv. 75"); Add(EROLanguage.French, "progress.finalEvolution", "Niv. 75"); Add(EROLanguage.German, "progress.finalEvolution", "Stufe 75"); Add(EROLanguage.Spanish, "progress.finalEvolution", "Nv. 75"); Add(EROLanguage.Italian, "progress.finalEvolution", "Lv. 75"); Add(EROLanguage.Dutch, "progress.finalEvolution", "Lvl. 75"); Add(EROLanguage.Portuguese, "progress.finalEvolution", "Nv. 75"); Add(EROLanguage.Japanese, "progress.finalEvolution", "Lv.75"); Add(EROLanguage.Korean, "progress.finalEvolution", "Lv.75"); Add(EROLanguage.ChineseSimplified, "progress.finalEvolution", "75级");
            Add(EROLanguage.English, "progress.final", "FINAL"); Add(EROLanguage.French, "progress.final", "FINAL"); Add(EROLanguage.German, "progress.final", "FINAL"); Add(EROLanguage.Spanish, "progress.final", "FINAL"); Add(EROLanguage.Italian, "progress.final", "FINALE"); Add(EROLanguage.Dutch, "progress.final", "EIND"); Add(EROLanguage.Portuguese, "progress.final", "FINAL"); Add(EROLanguage.Japanese, "progress.final", "最終"); Add(EROLanguage.Korean, "progress.final", "최종"); Add(EROLanguage.ChineseSimplified, "progress.final", "最终");
            string[] keys={"knight","assassin","ranger","mage","priest","monk","summoner","paladin"};
            string[][] evo={
                new[]{"Guardian","Gardien","Wächter","Guardián","Guardiano","Bewaker","Guardião","ガーディアン","가디언","守护者"},
                new[]{"Shadow","Ombre","Schatten","Sombra","Ombra","Schaduw","Sombra","シャドウ","섀도우","暗影"},
                new[]{"Sniper","Sniper","Scharfschütze","Francotirador","Cecchino","Sluipschutter","Atirador","スナイパー","스나이퍼","狙击手"},
                new[]{"Archmage","Archimage","Erzmagier","Archimago","Arcimago","Aartsmeester","Arquimago","アークメイジ","대마법사","大法师"},
                new[]{"High Priest","Grand Prêtre","Hohepriester","Sumo Sacerdote","Gran Sacerdote","Hogepriester","Sumo Sacerdote","ハイプリースト","하이 프리스트","大祭司"},
                new[]{"Champion","Champion","Champion","Campeón","Campione","Kampioen","Campeão","チャンピオン","챔피언","武斗家"},
                new[]{"Demonologist","Démonologue","Dämonologe","Demonólogo","Demonologo","Demonoloog","Demonologista","デモノロジスト","악마학자","恶魔学者"},
                new[]{"Crusader","Croisé","Kreuzritter","Cruzado","Crociato","Kruisvaarder","Cruzado","クルセイダー","크루세이더","圣殿骑士"}};
            string[][] weapons={
                new[]{"Sword + Shield","Épée + Bouclier","Schwert + Schild","Espada + Escudo","Spada + Scudo","Zwaard + Schild","Espada + Escudo","剣 + 盾","검 + 방패","剑 + 盾"},
                new[]{"Twin Blades","Lames doubles","Doppelklingen","Hojas gemelas","Lame gemelle","Dubbele messen","Lâminas duplas","双刃","쌍검","双刃"},
                new[]{"Bow","Arc","Bogen","Arco","Arco","Boog","Arco","弓","활","弓"},
                new[]{"Arcane Staff","Bâton arcanique","Arkanstab","Bastón arcano","Bastone arcano","Arcane staf","Cajado arcano","秘術の杖","비전 지팡이","奥术法杖"},
                new[]{"Holy Staff","Bâton sacré","Heiliger Stab","Bastón sagrado","Bastone sacro","Heilige staf","Cajado sagrado","聖杖","신성한 지팡이","圣杖"},
                new[]{"Fists","Poings","Fäuste","Puños","Pugni","Vuisten","Punhos","拳","권격","拳"},
                new[]{"Grimoire","Grimoire","Grimoire","Grimorio","Grimorio","Grimoire","Grimório","魔導書","마도서","魔导书"},
                new[]{"Holy Sword","Épée sacrée","Heiliges Schwert","Espada sagrada","Spada sacra","Heilig zwaard","Espada sagrada","聖剣","성검","圣剑"}};
            string[][] roles={
                new[]{"Tank / Frontline","Tank / Première ligne","Tank / Frontlinie","Tanque / Primera línea","Tank / Prima linea","Tank / Frontlinie","Tank / Linha de frente","タンク / 前衛","탱커 / 전열","坦克 / 前排"},
                new[]{"Melee DPS / Burst","DPS mêlée / Burst","Nahkampf-DPS / Burst","DPS cuerpo a cuerpo / Explosión","DPS corpo a corpo / Burst","Melee DPS / Burst","DPS corpo a corpo / Explosão","近接DPS / バースト","근접 DPS / 폭딜","近战DPS / 爆发"},
                new[]{"Ranged DPS / Mobility","DPS distance / Mobilité","Fernkampf-DPS / Mobilität","DPS a distancia / Movilidad","DPS a distanza / Mobilità","Ranged DPS / Mobiliteit","DPS à distância / Mobilidade","遠距離DPS / 機動力","원거리 DPS / 기동성","远程DPS / 机动"},
                new[]{"Magic DPS / Control","DPS magique / Contrôle","Magie-DPS / Kontrolle","DPS mágico / Control","DPS magico / Controllo","Magische DPS / Controle","DPS mágico / Controle","魔法DPS / 制御","마법 DPS / 제어","魔法DPS / 控制"},
                new[]{"Healer / Support","Soigneur / Support","Heiler / Support","Sanador / Apoyo","Curatore / Supporto","Healer / Support","Curador / Suporte","ヒーラー / 支援","힐러 / 지원","治疗 / 辅助"},
                new[]{"Melee DPS / Combo","DPS mêlée / Combo","Nahkampf-DPS / Combo","DPS cuerpo a cuerpo / Combo","DPS corpo a corpo / Combo","Melee DPS / Combo","DPS corpo a corpo / Combo","近接DPS / コンボ","근접 DPS / 콤보","近战DPS / 连击"},
                new[]{"Summoner / Control","Invocateur / Contrôle","Beschwörer / Kontrolle","Invocador / Control","Evocatore / Controllo","Oproeper / Controle","Invocador / Controle","召喚 / 制御","소환 / 제어","召唤 / 控制"},
                new[]{"Tank / Holy DPS","Tank / DPS sacré","Tank / Heiliger DPS","Tanque / DPS sagrado","Tank / DPS sacro","Tank / Heilige DPS","Tank / DPS sagrado","タンク / 聖属性DPS","탱커 / 신성 DPS","坦克 / 神圣DPS"}};
            string[][] desc={
                new[]{"A disciplined frontline warrior built to protect allies and control the battlefield.","Guerrier discipliné en première ligne, conçu pour protéger ses alliés et contrôler le champ de bataille."},
                new[]{"A swift assassin focused on burst damage, mobility, stealth and executions.","Assassin rapide axé sur les dégâts explosifs, la mobilité, la furtivité et les exécutions."},
                new[]{"A ranged hunter combining precision, critical hits and superior mobility.","Chasseur à distance combinant précision, coups critiques et excellente mobilité."},
                new[]{"An arcane caster wielding devastating spells and powerful battlefield control.","Mage arcanique maniant des sorts dévastateurs et un puissant contrôle du champ de bataille."},
                new[]{"A divine support specialist able to heal, shield and restore allies.","Spécialiste du soutien divin capable de soigner, protéger et restaurer ses alliés."},
                new[]{"A martial fighter mastering chi, combos and close-range physical damage.","Combattant martial maîtrisant le chi, les combos et les dégâts physiques au corps à corps."},
                new[]{"A master of summoned creatures, souls and demonic forces.","Maître des créatures invoquées, des âmes et des forces démoniaques."},
                new[]{"A holy warrior combining heavy defense, protection and sacred damage.","Guerrier sacré combinant défense lourde, protection et dégâts sacrés."}};
            EROLanguage[] langs={EROLanguage.English,EROLanguage.French,EROLanguage.German,EROLanguage.Spanish,EROLanguage.Italian,EROLanguage.Dutch,EROLanguage.Portuguese,EROLanguage.Japanese,EROLanguage.Korean,EROLanguage.ChineseSimplified};
            for(int i=0;i<keys.Length;i++){ for(int l=0;l<langs.Length;l++){ Add(langs[l],"class."+keys[i]+".evolution",evo[i][l]); Add(langs[l],"class."+keys[i]+".weapon",weapons[i][l]); Add(langs[l],"class."+keys[i]+".role",roles[i][l]); } Add(EROLanguage.English,"class."+keys[i]+".description",desc[i][0]); Add(EROLanguage.French,"class."+keys[i]+".description",desc[i][1]); }
        }

        void Add(EROLanguage language, string key, string value)
        {
            Dictionary<string,string> table;
            if (!tables.TryGetValue(language, out table)) return;
            table[key] = value;
        }

        static Dictionary<string,string> Merge(Dictionary<string,string> fallback, Dictionary<string,string> local)
        {
            var result = new Dictionary<string,string>(fallback);
            foreach (var pair in local) result[pair.Key]=pair.Value;
            return result;
        }
    }
}
