using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;

public class TMPFontAssetBuilder
{
    private const string VN_CHARS = "aáàảãạăắằẳẵặâấầẩẫậeéèẻẽẹêếềểễệiíìỉĩịoóòỏõọôốồổỗộơớờởỡợuúùủũụưứừửữựyýỳỷỹỵdđAÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬEÉÈẺẼẸÊẾỀỂỄỆIÍÌỈĨỊOÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢUÚÙỦŨỤƯỨỪỬỮỰYÝỲỶỸỴDĐ";
    private const string KR_CHARS = "가간감갓강개거게겨격결경계고곰과관괴교구국군굴권금기길김깨께나남내냈네념높놓누눕는능니다단닫당대덩도돌동뒤드득등떡뜨라락랑래량려력로룡루룸류를름리릭림립링마망맨맵먹며면명모목무문물민밀밖반방배백버번범법베벽볶봉부불브비빙빛사산살삼삿상새샷서선설세소속손수쉬스슬습승시식신심십씨아악안야양어언에열영옌오와요용우운웅워으을음의이일임입자작장쟁저전절점정제조족주죽중즉지진질쭝쯩천철첩청쳐초최축출춤충츠치친캐커컬컵코크킬타탈태택탱터템통퇴트팀팅파판패퍼펼포폭표풍프피필하한함합해했향헤호화환황획효흙희힘";

    [MenuItem("Mythic Bumper/Tạo Font Asset TextMeshPro Chuẩn Việt-Hàn")]
    public static void GenerateFontAssets()
    {
        Debug.Log("<color=cyan>[TMP FONT]</color> Bắt đầu tạo Font Asset cho BeVietnamPro và MalgunGothic...");

        Font vnFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/BeVietnamPro-ExtraBold.ttf");
        Font krFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/MalgunGothic-Bold.ttf");

        if (vnFont == null)
        {
            Debug.LogError("[TMP FONT] Không tìm thấy Assets/Fonts/BeVietnamPro-ExtraBold.ttf");
            return;
        }

        // 1. Tạo Font Asset cho BeVietnamPro-ExtraBold
        string vnCharacterSet = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" + VN_CHARS;
        
        TMP_FontAsset vnFontAsset = TMP_FontAsset.CreateFontAsset(vnFont, 36, 4, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
        if (vnFontAsset != null)
        {
            vnFontAsset.name = "BeVietnamPro-ExtraBold SDF";
            vnFontAsset.TryAddCharacters(vnCharacterSet);
            
            // Cấu hình Material với Outline đậm đà và Underlay (đổ bóng) phong cách hoạt hình
            Material mat = vnFontAsset.material;
            if (mat != null)
            {
                mat.EnableKeyword("OUTLINE_ON");
                mat.EnableKeyword("UNDERLAY_ON");
                mat.SetFloat("_OutlineWidth", 0.22f);
                mat.SetColor("_OutlineColor", new Color(0.18f, 0.08f, 0.02f, 1f)); // Viền nâu đậm hoạt hình
                
                mat.SetFloat("_UnderlayOffsetX", 0.40f);
                mat.SetFloat("_UnderlayOffsetY", -0.40f);
                mat.SetFloat("_UnderlayDilate", 0.20f);
                mat.SetFloat("_UnderlaySoftness", 0.15f);
                mat.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.70f)); // Đổ bóng nhẹ
            }

            string vnPath = "Assets/Fonts/BeVietnamPro-ExtraBold SDF.asset";
            AssetDatabase.CreateAsset(vnFontAsset, vnPath);
            AssetDatabase.AddObjectToAsset(vnFontAsset.material, vnFontAsset);
            if (vnFontAsset.atlasTexture != null)
            {
                AssetDatabase.AddObjectToAsset(vnFontAsset.atlasTexture, vnFontAsset);
            }
            string resFolder = "Assets/Resources/Fonts";
            if (!Directory.Exists(resFolder)) Directory.CreateDirectory(resFolder);
            AssetDatabase.CopyAsset(vnPath, "Assets/Resources/Fonts/BeVietnamPro-ExtraBold SDF.asset");
            Debug.Log($"<color=green>[TMP FONT]</color> Đã tạo thành công {vnPath}");
        }

        // 2. Tạo Font Asset cho MalgunGothic (Korean Hangul) làm Fallback
        if (krFont != null)
        {
            TMP_FontAsset krFontAsset = TMP_FontAsset.CreateFontAsset(krFont, 36, 4, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
            if (krFontAsset != null)
            {
                krFontAsset.name = "MalgunGothic-Bold SDF";
                krFontAsset.TryAddCharacters(KR_CHARS + " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~");
                
                string krPath = "Assets/Fonts/MalgunGothic-Bold SDF.asset";
                AssetDatabase.CreateAsset(krFontAsset, krPath);
                AssetDatabase.AddObjectToAsset(krFontAsset.material, krFontAsset);
                if (krFontAsset.atlasTexture != null)
                {
                    AssetDatabase.AddObjectToAsset(krFontAsset.atlasTexture, krFontAsset);
                }
                AssetDatabase.CopyAsset(krPath, "Assets/Resources/Fonts/MalgunGothic-Bold SDF.asset");

                // Gán krFontAsset vào fallback table của vnFontAsset
                if (vnFontAsset != null)
                {
                    if (vnFontAsset.fallbackFontAssetTable == null)
                    {
                        vnFontAsset.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                    }
                    if (!vnFontAsset.fallbackFontAssetTable.Contains(krFontAsset))
                    {
                        vnFontAsset.fallbackFontAssetTable.Add(krFontAsset);
                        EditorUtility.SetDirty(vnFontAsset);
                    }
                    Debug.Log("<color=green>[TMP FONT]</color> Đã liên kết MalgunGothic làm Fallback Font cho tiếng Hàn!");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
