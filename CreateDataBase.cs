using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class CreateDataBase : EditorWindow
{
    private const string DESTINATION_PATH = "Assets/Resources/CardData/";
    const string CARD_IMAGE_PATH =          "Assets/MyAssets/CardImage/C";
    string csvPath =                        "Assets/MyAssets/DataBase.csv";

    //Editor→CreateDataBaseで選べるようにする
    [MenuItem("Editor/CreateDataBase")]
    private static void CreateWindow()
    {
        //ウィンドウ生成
        GetWindow<CreateDataBase>("カードデータ管理画面");
    }

    //ウィンドウ内のGUI。左右に並べたい場合はHorizontalScopeの中に二個GUI要素を入れれば良い
    private void OnGUI()
    {
        using(new GUILayout.HorizontalScope())
        {
            csvPath = EditorGUILayout.TextField("DataBase Path:", csvPath);
        }

        using(new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("データ更新"))
            {
                UpdateDataBase();
            }
        }       
    }
    private void UpdateDataBase()
    {
        TextAsset databaseText = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
        if (databaseText == null)
        {
            Debug.LogError("Failed to load Database");
            return;
        }
        string[] lines = databaseText.ToString().Split('\n');
        Debug.Log(lines.Length-2 + "data loaded");
        for(int i=1; i<lines.Length-1; i++)
        {
            CardData cardData = ConvertCSVline2CardData(lines[i]);
            if(cardData == null)
            {
                Debug.LogError("Broken data or empty field found");
                return;
            }           
            string directory = Path.GetDirectoryName(DESTINATION_PATH);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            AssetDatabase.CreateAsset(cardData, DESTINATION_PATH + "C" + i.ToString() + ".asset");
            cardData.hideFlags = HideFlags.NotEditable;
            EditorUtility.SetDirty(cardData);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("updated Database");
    }

    public static CardData ConvertCSVline2CardData(string line)
    {
        CardData cardData = CreateInstance<CardData>();
        string[] columns = line.Split(',');
        //foreach (string i in columns) if (i == "") return null;
        /*
            0id
            1kind    
            2rarerity 
            3cardName
            4cost
            5element
            6openEffect
            7skill
            8passive
            9hp
            10attack
            11moveCount
            12spped 
            13description
        */
        cardData.ID = int.Parse(columns[0]);
        if (Enum.TryParse(columns[1], out CardKind kind)) cardData.kind = kind;
        else return null;
        if (Enum.TryParse(columns[2], out Rarerity rare)) cardData.rarerity = rare;
        else return null;
        cardData.cardName = columns[3];
        cardData.cost = int.Parse(columns[4]);
        if (Enum.TryParse(columns[5], out Element element)) cardData.element = element;
        else return null;
        if (Enum.TryParse(columns[6], out OpenEffect openEffect)) cardData.openEffect = openEffect;
        else return null;
        if (Enum.TryParse(columns[7], out Skill skill)) cardData.skill = skill;
        else return null;
        if (Enum.TryParse(columns[8], out Passive passive)) cardData.passive = passive;
        else return null;
        cardData.hp = int.Parse(columns[9]);
        cardData.attack = int.Parse(columns[10]);
        cardData.moveCount = int.Parse(columns[11]);
        cardData.spped = int.Parse(columns[12]);
        cardData.magicDescription = columns[13];
        Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(CARD_IMAGE_PATH + cardData.ID.ToString() + ".JPG");
        if (image != null) cardData.image = image;
        return cardData;
    }
}
