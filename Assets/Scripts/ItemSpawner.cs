using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý xuất hiện ngẫu nhiên các vật phẩm văn hóa Việt - Hàn trên sới đấu:
/// Bánh Chưng, Bánh Gạo Tteokbokki, Nón Lá, Quạt Lửa Buchaechum.
/// Dự án: Mythic Bumper (KGW2026).
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("--- THIẾT LẬP SPAWN VẬT PHẨM ---")]
    [Tooltip("Thời gian giữa các lần xuất hiện vật phẩm (giây)")]
    [SerializeField] private float spawnInterval = 12.0f;

    [Tooltip("Số vật phẩm tối đa tồn tại cùng lúc trên sân")]
    [SerializeField] private int maxActiveItems = 2;

    [Tooltip("Bán kính vùng an toàn trong sới đấu để thả vật phẩm")]
    [SerializeField] private float spawnRadius = 2.4f;

    [Header("--- PREFABS HOẶC SPRITES VẬT PHẨM ---")]
    [SerializeField] private Sprite[] itemSprites;

    private float timer = 0f;
    private List<GameObject> activeItems = new List<GameObject>();

    private void Awake()
    {
        LoadItemSpritesIfEmpty();
    }

    private void Start()
    {
        timer = spawnInterval * 0.5f; // Thả vật phẩm đầu tiên sau 6s
        LoadItemSpritesIfEmpty();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        // CHỈ sinh vật phẩm khi đang ở đúng trạng thái BattlePlaying
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.BattlePlaying)
            return;

        // Dọn dẹp danh sách các vật phẩm đã bị nhặt
        activeItems.RemoveAll(item => item == null);

        if (activeItems.Count >= maxActiveItems) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnRandomItem();
        }
    }

    private void LoadItemSpritesIfEmpty()
    {
        if (itemSprites == null || itemSprites.Length == 0)
        {
#if UNITY_EDITOR
            itemSprites = new Sprite[]
            {
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_banh_chung.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_banh_gao_tteokbokki.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_non_la_phap_bao.png"),
                UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Items/item_quat_lua_buchaechum.png")
            };
#endif
        }
    }

    public void SpawnRandomItem()
    {
        LoadItemSpritesIfEmpty();
        if (itemSprites == null || itemSprites.Length == 0) return;

        int index = Random.Range(0, itemSprites.Length);
        Sprite chosenSprite = itemSprites[index];
        if (chosenSprite == null) return;

        // Vị trí ngẫu nhiên bên trong sới đấu, tránh tâm vũng bùn (x=0, y=0)
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        if (randomPos.magnitude < 0.6f) randomPos = randomPos.normalized * 1.2f;

        GameObject itemObj = new GameObject($"CulturalItem_{index}");
        itemObj.transform.position = new Vector3(randomPos.x, randomPos.y, 0f);
        itemObj.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

        var sr = itemObj.AddComponent<SpriteRenderer>();
        sr.sprite = chosenSprite;
        sr.sortingLayerName = "Gameplay";
        sr.sortingOrder = 3;

        var col = itemObj.AddComponent<CircleCollider2D>();
        col.radius = 0.55f;
        col.isTrigger = true;

        var cultural = itemObj.AddComponent<CulturalItem>();
        cultural.SetItemType((CulturalItem.ItemType)(index % 4));

        activeItems.Add(itemObj);

        // Hiệu ứng mây vàng xuất hiện
        var vfx = Object.FindAnyObjectByType<VFXManager>();
        if (vfx != null)
        {
            vfx.PlayGoldenCloud(itemObj.transform.position);
        }
    }

    public void ClearAllItems()
    {
        foreach (var it in activeItems)
        {
            if (it != null) Destroy(it);
        }
        activeItems.Clear();
    }
}
