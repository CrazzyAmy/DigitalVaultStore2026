# Digital Vault

![Tests](https://github.com/CrazzyAmy/DigitalVaultStore2026
/actions/workflows/test.yml/badge.svg)

數位商品電商平台

## 用假的 OrderId 測試評論表單
### 原。對完成訂單的評論流程
使用者購買商品
  └── 建立 Order（Status: Paid）
        └── Order 底下有 OrderItems（含 ProductId）
              └── 進入商品頁，找到對應 OrderId
                    └── 才能送出 Review（需要 ProductId + OrderId）
### 測試用。用假的 OrderId 測試評論表單
在 `DetailPage.jsx` 裡直接 hardcode 一個假的 `OrderId` 來繞過訂單驗證：



```jsx
// DetailPage.jsx
const { sessionCart, addToCart, isGuest, currentUserId } = useApp();

// 模擬：交易功能未實作，先用假的 OrderId 讓評論表單可以顯示
const MOCK_ORDER_ID = "00000000-0000-0000-0000-000000000001";
```

但後端的 `CreateAsync` 會去驗證這個 `OrderId` 是否真實存在，所以後端也要配合放寬驗證：

**後端 ReviewService 暫時跳過 OrderId 驗證：**

```csharp
// Services/ReviewService.cs
public async Task<(bool Success, string Message)> CreateAsync(Guid userId, CreateReviewRequest request)
{
    if (request.Rating < 1 || request.Rating > 5)
        return (false, "評分必須介於 1 到 5 之間");

    var exists = await _reviewRepository.ExistsAsync(userId, request.ProductId, request.OrderId);
    if (exists)
        return (false, "此訂單已對該商品評論過");

    var review = new Review
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ProductId = request.ProductId,
        OrderId = request.OrderId, // 模擬期間直接存入，不驗證是否真實存在
        Rating = request.Rating,
        Comment = request.Comment,
        CreatedAt = DateTime.UtcNow
    };

    var created = await _reviewRepository.CreateAsync(review);
    return created ? (true, "評論新增成功") : (false, "評論新增失敗，請稍後再試");
}
```

同時在資料庫把 `OrderId` 的外鍵約束暫時設為不強制：

```csharp
// DigitalVaultStoreDbContext.cs OnModelCreating 補上
modelBuilder.Entity<Review>()
    .HasOne(r => r.Order)
    .WithMany(o => o.Reviews)
    .HasForeignKey(r => r.OrderId)
    .OnDelete(DeleteBehavior.NoAction)
    .IsRequired(false); // 模擬期間允許 OrderId 不存在
```

**前端 DetailPage 完整修改：**

```jsx
// DetailPage.jsx
import { useParams } from "react-router-dom";
import { useApp } from "../../context/AppContext";
import PageStatus from "../ui/PageStatus";
import { useProductDetail } from "../../hook/useProduct";
import ReviewSection from "../review/ReviewSection";

const INCLUDES = [
  "即時數位下載",
  "永久存取權限",
  "購買憑證（OrderItems 記錄）",
  "30 天退款保障",
];

// 模擬用，交易實作後移除
const MOCK_ORDER_ID = "00000000-0000-0000-0000-000000000001";

const DetailPage = () => {
  const { id } = useParams();
  const { sessionCart, addToCart, isGuest, currentUserId } = useApp();
  const { product, loading, error } = useProductDetail(id);

  if (loading || error) return <PageStatus loading={loading} error={error} />;
  if (!product) return null;

  const inCart = sessionCart.some((p) => p.id === product.id);

  return (
    <>
      <div className="detail-layout">
        <div className="detail-img">
          <img src={product.thumbnailUrl} alt={product.name} />
        </div>
        <div className="detail-info">
          <div className="detail-cat">{product.categoryName}</div>
          <div className="detail-title">{product.name}</div>
          <p className="detail-desc">{product.description}</p>
          <div className="detail-includes">
            <h4>包含內容</h4>
            {INCLUDES.map((item) => (
              <div key={item} className="include-item">{item}</div>
            ))}
          </div>
          <div className="detail-price-row">
            <span className="detail-price">${product.price}</span>
            <button
              className={`btn-add-cart ${inCart ? "added" : ""}`}
              onClick={() => addToCart(product)}
            >
              {inCart ? "✓ 已加入購物車" : "加入購物車"}
            </button>
          </div>
          <div style={{ fontSize: "0.78rem", color: "var(--muted)" }}>
            {isGuest()
              ? "🔒 結帳時需要登入，加入購物車無需帳號"
              : "付款後狀態：Orders.Status = Paid → Completed"}
          </div>
        </div>
      </div>

      <ReviewSection
        productId={product.id}
        currentUserId={currentUserId}
        userOrderId={isGuest() ? null : MOCK_ORDER_ID}
      />
    </>
  );
};

export default DetailPage;
```

---

這樣的效果：

| 狀態 | 評論表單 |
|------|---------|
| 未登入 | 隱藏表單 |
| 已登入 + 未評論過 | 顯示表單 |
| 已登入 + 已評論過 | 隱藏表單，只顯示自己的評論 |

之後交易功能實作完成後，只需要把 `MOCK_ORDER_ID` 換成從真實訂單查到的 `OrderId`，後端的 `IsRequired(false)` 也改回來就好。
