-- Read-only audit. Run on the actual HL25 production database, then compare the
-- outputs with the local/staging data. Set @TenantId for a shared database.
DECLARE @TenantId uniqueidentifier = NULL;

SELECT DB_NAME() AS DatabaseName, c.TenantId, c.Id AS WheelConfigId,
       c.IsActive, c.IsDeleted, c.CreationTime, c.LastModificationTime,
       COUNT(s.Id) AS SlotCount,
       CAST(COALESCE(SUM(s.WinRate), 0) AS decimal(12,4)) AS TotalWinRate
FROM hl25.AppHl25WheelConfig c
LEFT JOIN hl25.AppHl25WheelSlots s ON s.WheelConfigId = c.Id AND s.IsDeleted = 0
WHERE (@TenantId IS NULL OR c.TenantId = @TenantId)
GROUP BY c.TenantId, c.Id, c.IsActive, c.IsDeleted, c.CreationTime, c.LastModificationTime
ORDER BY c.IsActive DESC, c.CreationTime DESC;

SELECT s.TenantId, s.WheelConfigId, s.Id AS SlotId, s.DisplayOrder,
       s.GiftId, g.Name AS GiftName, s.WinRate,
       g.TotalQuantity, g.RemainingQuantity, g.Status AS GiftStatus,
       s.CreationTime AS SlotCreated, s.LastModificationTime AS SlotModified
FROM hl25.AppHl25WheelSlots s
LEFT JOIN hl25.AppHl25Gifts g ON g.Id = s.GiftId AND g.IsDeleted = 0
WHERE s.IsDeleted = 0 AND (@TenantId IS NULL OR s.TenantId = @TenantId)
ORDER BY s.TenantId, s.WheelConfigId, s.DisplayOrder, s.Id;

;WITH Spins AS (
    SELECT s.*,
           CASE WHEN EXISTS (
               SELECT 1 FROM hl25.AppHl25SpinLogs w
               WHERE w.IsDeleted = 0
                 AND (w.TenantId = s.TenantId OR (w.TenantId IS NULL AND s.TenantId IS NULL))
                 AND w.ParticipantId = s.ParticipantId AND w.GiftId IS NOT NULL
                 AND (w.SpinTime < s.SpinTime OR
                     (w.SpinTime = s.SpinTime AND CONVERT(binary(16),w.Id) < CONVERT(binary(16),s.Id)))
           ) THEN 0 ELSE 1 END AS WasEligible
    FROM hl25.AppHl25SpinLogs s
    WHERE s.IsDeleted = 0 AND (@TenantId IS NULL OR s.TenantId = @TenantId)
)
SELECT TenantId, COUNT(*) AS TotalSpins, SUM(WasEligible) AS EligibleSpins,
       SUM(CASE WHEN WasEligible = 0 THEN 1 ELSE 0 END) AS PriorWinnerSpins,
       SUM(CASE WHEN GiftId IS NOT NULL THEN 1 ELSE 0 END) AS GiftAllocatedSpins,
       SUM(CASE WHEN GiftId IS NULL THEN 1 ELSE 0 END) AS NotAllocatedSpins,
       SUM(CASE WHEN RewardStatus = 1 THEN 1 ELSE 0 END) AS WonPending,
       SUM(CASE WHEN RewardStatus = 3 THEN 1 ELSE 0 END) AS Delivered,
       SUM(CASE WHEN GiftId IS NOT NULL AND RewardStatus NOT IN (1,3) THEN 1 ELSE 0 END) AS GiftWithOtherStatus,
       MIN(SpinTime) AS FirstSpin, MAX(SpinTime) AS LastSpin
FROM Spins GROUP BY TenantId;

;WITH Ordered AS (
    SELECT s.TenantId, s.Id, s.ParticipantId, s.GiftId, s.WheelSlotId, s.RewardStatus,
           s.SpinTime,
           ROW_NUMBER() OVER (PARTITION BY s.TenantId ORDER BY s.SpinTime, s.Id) AS SpinOrdinal
    FROM hl25.AppHl25SpinLogs s
    WHERE s.IsDeleted = 0 AND (@TenantId IS NULL OR s.TenantId = @TenantId)
), GiftAwards AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY TenantId, GiftId ORDER BY SpinTime, Id) AS AwardOrdinal
    FROM Ordered WHERE GiftId IS NOT NULL
)
SELECT g.TenantId, g.Id AS GiftId, g.Name AS GiftName,
       g.TotalQuantity, g.RemainingQuantity,
       COUNT(a.Id) AS AllocatedLogCount,
       g.TotalQuantity - g.RemainingQuantity AS StockDelta,
       MAX(CASE WHEN a.AwardOrdinal = 50 THEN a.SpinOrdinal END) AS GlobalSpinAt50thAward,
       MAX(CASE WHEN a.AwardOrdinal = 50 THEN a.SpinTime END) AS TimeAt50thAward,
       COUNT(DISTINCT a.ParticipantId) AS DistinctWinners
FROM hl25.AppHl25Gifts g
LEFT JOIN GiftAwards a ON a.GiftId = g.Id
    AND (a.TenantId = g.TenantId OR (a.TenantId IS NULL AND g.TenantId IS NULL))
WHERE g.IsDeleted = 0 AND (@TenantId IS NULL OR g.TenantId = @TenantId)
GROUP BY g.TenantId, g.Id, g.Name, g.TotalQuantity, g.RemainingQuantity
ORDER BY AllocatedLogCount DESC, g.Name;

SELECT s.TenantId, s.ParticipantId, COUNT(*) AS GiftAwardCount,
       MIN(s.SpinTime) AS FirstAward, MAX(s.SpinTime) AS LastAward
FROM hl25.AppHl25SpinLogs s
WHERE s.IsDeleted = 0 AND s.GiftId IS NOT NULL
  AND (@TenantId IS NULL OR s.TenantId = @TenantId)
GROUP BY s.TenantId, s.ParticipantId HAVING COUNT(*) > 1;

-- Gift result slot and award status. A GiftId on a no-gift slot, or a gift slot
-- with NotWon, deserves individual review; historical slot edits can also cause this.
SELECT s.TenantId, s.WheelSlotId, slot.WheelConfigId,
       slot.GiftId AS CurrentSlotGiftId, s.GiftId AS IssuedGiftId,
       s.RewardStatus, COUNT(*) AS SpinCount
FROM hl25.AppHl25SpinLogs s
LEFT JOIN hl25.AppHl25WheelSlots slot ON slot.Id = s.WheelSlotId AND slot.IsDeleted = 0
WHERE s.IsDeleted = 0 AND (@TenantId IS NULL OR s.TenantId = @TenantId)
GROUP BY s.TenantId, s.WheelSlotId, slot.WheelConfigId,
         slot.GiftId, s.GiftId, s.RewardStatus
ORDER BY SpinCount DESC;

;WITH Eligible AS (
    SELECT s.TenantId, COUNT_BIG(*) AS EligibleSpinCount
    FROM hl25.AppHl25SpinLogs s
    WHERE s.IsDeleted = 0 AND (@TenantId IS NULL OR s.TenantId = @TenantId)
      AND NOT EXISTS (
          SELECT 1 FROM hl25.AppHl25SpinLogs w
          WHERE w.IsDeleted = 0 AND w.ParticipantId = s.ParticipantId
            AND (w.TenantId = s.TenantId OR (w.TenantId IS NULL AND s.TenantId IS NULL))
            AND w.GiftId IS NOT NULL
            AND (w.SpinTime < s.SpinTime OR
                (w.SpinTime = s.SpinTime AND CONVERT(binary(16),w.Id) < CONVERT(binary(16),s.Id))))
    GROUP BY s.TenantId
)
SELECT g.TenantId, g.Id AS GiftId, g.Name AS GiftName,
       COALESCE(e.EligibleSpinCount, 0) AS EligibleSpinCount,
       awards.AllocatedCount,
       CAST(CASE WHEN COALESCE(e.EligibleSpinCount, 0) = 0 THEN 0
                 ELSE 100.0 * awards.AllocatedCount / e.EligibleSpinCount END AS decimal(9,4))
           AS ObservedWinRatePercent
FROM hl25.AppHl25Gifts g
OUTER APPLY (
    SELECT COUNT_BIG(*) AS AllocatedCount FROM hl25.AppHl25SpinLogs s
    WHERE s.IsDeleted = 0 AND s.GiftId = g.Id
      AND (s.TenantId = g.TenantId OR (s.TenantId IS NULL AND g.TenantId IS NULL))
) awards
LEFT JOIN Eligible e ON e.TenantId = g.TenantId OR (e.TenantId IS NULL AND g.TenantId IS NULL)
WHERE g.IsDeleted = 0 AND (@TenantId IS NULL OR g.TenantId = @TenantId)
ORDER BY g.Name;
