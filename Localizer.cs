using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Localizer : MonoBehaviour
{
    public static Language language = Language.jp;
    public Language testOverRide = Language.jp;
    [SerializeField] List<TextMeshProUGUI> targetTMPs;
    [SerializeField] List<InGameText> targetTexts;
    const string PATH_JP = "jp";
    const string PATH_ENG = "eng";
    const char splitSymbol = '\n';
    const char newLineSymbol = '$';

    void Start()
    {
        //デバッグ用の切り替えオプション
        language = testOverRide;
        string[] texts = GetTexts();
        for (int i = 0; i < targetTMPs.Count; i++)
        {
            targetTMPs[i].text = texts[(int)targetTexts[i]].Replace(newLineSymbol, '\n');
        }
    }
    //例えば徐々に増えていくダイアログや一つのTMPに複数のテキストが入る場合は別で処理を書く
    public static string[] GetTexts()
    {
        TextAsset textAsset = language == Language.jp ? Resources.Load<TextAsset>(PATH_JP) : Resources.Load<TextAsset>(PATH_ENG);
        return textAsset.text.Split(splitSymbol);
    }
}
public enum Language
{
    jp,
    eng,
}
public enum InGameText
{
    yes,
    no,
    Title_SoundWarning,
    Title_Credit,
    //
    //(略)
    //すべての文字列に名前をつける必要がある。
    //
    Ending_Dialog,
    Ending_Warning
}
